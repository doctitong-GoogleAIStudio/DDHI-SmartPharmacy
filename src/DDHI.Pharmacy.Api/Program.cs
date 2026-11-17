using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Infrastructure;
using DDHI.Pharmacy.Integration;
using DDHI.Pharmacy.Barcode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
var builder=WebApplication.CreateBuilder(args);
var protectedConfig=Environment.GetEnvironmentVariable("DDHI_CONFIG_FILE");
if(!string.IsNullOrWhiteSpace(protectedConfig))builder.Configuration.AddJsonFile(protectedConfig,false,true);
ProtectedServiceConfiguration.Apply(builder.Configuration);
var keyDirectory=builder.Configuration["DDHI:KeyDirectory"]??Path.Combine(AppContext.BaseDirectory,"keys");
var protection=builder.Services.AddDataProtection().SetApplicationName("DDHI.SmartPharmacy").PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
if(!builder.Environment.IsDevelopment()){
 var certPath=builder.Configuration["DDHI:CertificatePath"];
 if(string.IsNullOrWhiteSpace(certPath)&&!args.Contains("--migrate")&&!args.Contains("--bootstrap"))throw new InvalidOperationException("Production requires an approved certificate for data-protection key encryption.");
 if(!string.IsNullOrWhiteSpace(certPath))protection.ProtectKeysWithCertificate(X509CertificateLoader.LoadPkcs12FromFile(certPath,builder.Configuration["DDHI:CertificatePassword"],X509KeyStorageFlags.EphemeralKeySet));
}
builder.Host.UseWindowsService(o=>o.ServiceName="DDHIPharmacyApi");
builder.Host.UseSerilog((c,l)=>l.WriteTo.File(Path.Combine(builder.Configuration["DDHI:LogDirectory"]??Path.Combine(AppContext.BaseDirectory,"logs"),"api-.log"),rollingInterval:RollingInterval.Day,retainedFileCountLimit:30));
var connection=builder.Configuration.GetConnectionString("Pharmacy")??throw new InvalidOperationException("Configure ConnectionStrings__Pharmacy using a protected service configuration.");
builder.Services.AddDbContext<PharmacyDb>(o=>o.UseNpgsql(connection));
builder.Services.AddIdentityCore<PharmacyUser>(o=>{o.Password.RequiredLength=12;o.Password.RequireNonAlphanumeric=true;o.Lockout.MaxFailedAccessAttempts=5;o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);o.User.RequireUniqueEmail=true;})
 .AddRoles<IdentityRole>().AddEntityFrameworkStores<PharmacyDb>().AddSignInManager().AddDefaultTokenProviders();
builder.Services.AddAuthentication(IdentityConstants.BearerScheme).AddBearerToken(IdentityConstants.BearerScheme,o=>{o.BearerTokenExpiration=TimeSpan.FromMinutes(20);o.RefreshTokenExpiration=TimeSpan.FromHours(8);});
builder.Services.AddAuthorization(o=>{
 o.AddPolicy("Inventory",p=>p.RequireRole(Roles.Inventory));o.AddPolicy("Catalog",p=>p.RequireRole(Roles.Catalog));o.AddPolicy("Clinical",p=>p.RequireRole(Roles.Clinical));
 o.AddPolicy("Admin",p=>p.RequireRole(Roles.SystemAdmin));o.AddPolicy("Report",p=>p.RequireRole(Roles.PharmacyAdmin,Roles.Chief,Roles.Pharmacist,Roles.Auditor));
});
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.GlobalLimiter=PartitionedRateLimiter.Create<HttpContext,string>(ctx=>RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"local",_=>new FixedWindowRateLimiterOptions{PermitLimit=240,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));o.AddFixedWindowLimiter("login",o=>{o.PermitLimit=15;o.Window=TimeSpan.FromMinutes(1);});});
builder.Services.ConfigureHttpJsonOptions(o=>{o.SerializerOptions.ReferenceHandler=ReferenceHandler.IgnoreCycles;o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());});
builder.Services.AddOpenApi();builder.Services.AddScoped<InventoryService>();builder.Services.AddScoped<LifecycleService>();builder.Services.AddSingleton<ICerebroIntegration,MockCerebroIntegration>();
var app=builder.Build();
if(args.Contains("--migrate")||args.Contains("--bootstrap")) {
 await using var scope=app.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();
 if(args.Contains("--migrate"))await db.Database.MigrateAsync();
 if(args.Contains("--bootstrap")){
  var users=scope.ServiceProvider.GetRequiredService<UserManager<PharmacyUser>>();var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
  foreach(var role in Roles.All)if(!await roles.RoleExistsAsync(role))await roles.CreateAsync(new IdentityRole(role));
  var email=Environment.GetEnvironmentVariable("DDHI_BOOTSTRAP_EMAIL");var password=Environment.GetEnvironmentVariable("DDHI_BOOTSTRAP_PASSWORD");
  if(await users.Users.AnyAsync())throw new InvalidOperationException("Bootstrap refused: users already exist.");
  if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password))throw new InvalidOperationException("Set DDHI_BOOTSTRAP_EMAIL and DDHI_BOOTSTRAP_PASSWORD in this process only.");
  var user=new PharmacyUser{UserName=email,Email=email,EmailConfirmed=true};var result=await users.CreateAsync(user,password);if(!result.Succeeded)throw new InvalidOperationException(string.Join(";",result.Errors.Select(x=>x.Description)));await users.AddToRoleAsync(user,Roles.SystemAdmin);
 }
 return;
}
app.Use(async(ctx,next)=>{
 if(!app.Environment.IsDevelopment()&&!ctx.Request.IsHttps){ctx.Response.StatusCode=400;await ctx.Response.WriteAsJsonAsync(new{error="HTTPS is mandatory."});return;}
 ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["Cache-Control"]="no-store";
 try{await next();}catch(Exception ex) when(ex is InvalidOperationException or FormatException or DbUpdateException or KeyNotFoundException){ctx.Response.StatusCode=409;await ctx.Response.WriteAsJsonAsync(new{error=ex is DbUpdateException?"Database constraint conflict. Review references, quantities and duplicate transactions.":ex.Message});}
});
app.UseRateLimiter();app.UseAuthentication();
// Opaque Identity bearer tickets must also refer to an existing, enabled account with unchanged roles.
app.Use(async(ctx,next)=>{if(ctx.User.Identity?.IsAuthenticated==true){var manager=ctx.RequestServices.GetRequiredService<UserManager<PharmacyUser>>();var u=await manager.GetUserAsync(ctx.User);if(u==null||await manager.IsLockedOutAsync(u)){ctx.Response.StatusCode=401;return;}var signIn=ctx.RequestServices.GetRequiredService<SignInManager<PharmacyUser>>();if(!await signIn.ValidateSecurityStampAsync(u,ctx.User.FindFirstValue(signIn.Options.ClaimsIdentity.SecurityStampClaimType))){ctx.Response.StatusCode=401;return;}var current=await manager.GetRolesAsync(u);if(!current.Order().SequenceEqual(ctx.User.FindAll(ClaimTypes.Role).Select(c=>c.Value).Order())){ctx.Response.StatusCode=401;return;}}await next();});
app.UseAuthorization();
app.MapGet("/health",async(PharmacyDb db)=>Results.Json(new{status=await db.Database.CanConnectAsync()?"ready":"database unavailable",version="0.2.1",liveCerebro=false}));
app.MapPost("/auth/login",async(LoginInput r,UserManager<PharmacyUser> users,SignInManager<PharmacyUser> signIn,HttpContext ctx,InventoryService service)=>{
 var user=await users.FindByEmailAsync(r.Email);if(user==null)return Results.Unauthorized();
 var check=await signIn.CheckPasswordSignInAsync(user,r.Password,true);if(!check.Succeeded)return Results.Unauthorized();
 var principal=await signIn.CreateUserPrincipalAsync(user);
 await service.Execute(Guid.NewGuid().ToString(),new Actor(user.Id,principal.FindFirstValue(ClaimTypes.Role)??"",ctx.Request.Headers["X-Workstation"].ToString() is {Length:>0} w?w:"sign-in"),new{Action="Successful sign-in"},()=>Task.FromResult<object>(new{authenticated=true}));
 await ctx.SignInAsync(IdentityConstants.BearerScheme,principal);return Results.Empty;
}).RequireRateLimiting("login");
var api=app.MapGroup("/api").RequireAuthorization();
api.MapGet("/me",(HttpContext c)=>new{id=c.User.FindFirstValue(ClaimTypes.NameIdentifier),user=c.User.Identity?.Name,roles=c.User.FindAll(ClaimTypes.Role).Select(x=>x.Value)});
api.MapGet("/openapi",()=>Results.Redirect("/openapi/v1.json")).RequireAuthorization("Admin");
if(app.Environment.IsDevelopment())app.MapOpenApi().RequireAuthorization("Admin");
static Actor ActorOf(HttpContext c)=>new(c.User.FindFirstValue(ClaimTypes.NameIdentifier)??"",ActorRoles.ForOperation(c.User.FindAll(ClaimTypes.Role).Select(x=>x.Value),c.Request.Path.ToString()),c.Request.Headers["X-Workstation"].ToString());
static async Task<IResult> Mutate<T>(T r,HttpContext c,InventoryService service,Func<Actor,Task<object>> action){var actor=ActorOf(c);var result=await service.Execute(c.Request.Headers["Idempotency-Key"].ToString(),actor,new MutationEnvelope<T>(c.Request.Path.ToString(),r),()=>action(actor));return Results.Content(result,"application/json");}
static string MedicineTradeText(string? value){var text=value?.Trim()??"";Safety.Require(text.Length<=2000&&!text.Any(char.IsControl),"Importer/distributor must be at most 2000 characters without control characters.");return text;}
api.MapPost("/medicines/{id:guid}/trade",async(Guid id,MedicineTradeInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(new{id,r},c,s,async a=>{Safety.Require(r.Confirmed&&!string.IsNullOrWhiteSpace(r.Reason)&&r.Reason.Length<=500,"Confirm the catalog change and provide a reason (up to 500 characters).");var medicine=await db.Medicines.SingleAsync(m=>m.Id==id);medicine.ImportedBy=MedicineTradeText(r.ImportedBy);medicine.DistributedBy=MedicineTradeText(r.DistributedBy);medicine.UpdatedAt=DateTime.UtcNow;return new{medicine.Id,medicine.ImportedBy,medicine.DistributedBy};})).RequireAuthorization("Catalog");
api.MapGet("/medicines",async(PharmacyDb db)=>await db.Medicines.AsNoTracking().OrderBy(x=>x.GenericName).Take(10000).ToListAsync());
api.MapPost("/medicines",async(MedicineInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{
 Safety.Require(!string.IsNullOrWhiteSpace(r.GenericName)&&!string.IsNullOrWhiteSpace(r.Strength)&&!string.IsNullOrWhiteSpace(r.DosageForm)&&!string.IsNullOrWhiteSpace(r.BaseUnit)&&r.ReorderLevel>=0,"Generic name, strength, form, base unit and nonnegative reorder level required.");
 var m=new Medicine{GenericName=r.GenericName,BrandName=r.BrandName,Strength=r.Strength,DosageForm=r.DosageForm,Route=r.Route,Manufacturer=r.Manufacturer,ImportedBy=MedicineTradeText(r.ImportedBy),DistributedBy=MedicineTradeText(r.DistributedBy),Category=r.Category,BaseUnit=r.BaseUnit,ReorderLevel=r.ReorderLevel};db.Medicines.Add(m);return Task.FromResult<object>(m);
})).RequireAuthorization("Catalog");
api.MapGet("/barcodes",async(PharmacyDb db)=>await db.MedicineBarcodes.AsNoTracking().ToListAsync());
api.MapPost("/barcodes",async(BarcodeInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.RegisterBarcode(r,a))).RequireAuthorization("Catalog");
api.MapPost("/scan",async(ScanInput r,HttpContext c,PharmacyDb db,InventoryService service)=>await Mutate(r,c,service,async actor=>{
 Safety.Require(Guid.TryParse(actor.Workstation,out var wid)&&await db.Workstations.AnyAsync(x=>x.Id==wid),"Register this workstation first.");
 var resolved=await new BarcodeLookup(db).Resolve(r.Barcode);
 db.ScannerEvents.Add(new ScannerEvent{WorkstationId=wid,Mode="ManualBarcode / HID unverified",Symbology=resolved.Data.Symbology,Success=resolved.Result.Status=="Matched"});
 return (object)new {data=resolved.Data,medicine=resolved.Medicine,stock=resolved.InternalStock==null?null:new{resolved.InternalStock.Id,resolved.InternalStock.Batch.Lot,resolved.InternalStock.Batch.Expiration,resolved.InternalStock.Status,resolved.InternalStock.Quantity},verification="Unverified input. Scan alone never commits dispensing.",result=resolved.Result};
}));
api.MapGet("/inventory",async(PharmacyDb db)=>await db.InventoryBalances.AsNoTracking().OrderBy(x=>x.Batch.Expiration).Select(x=>new{x.Id,x.BatchId,x.LocationId,x.Batch.MedicineId,Medicine=x.Batch.Medicine.GenericName,x.Batch.Medicine.Strength,x.Batch.Medicine.DosageForm,x.Batch.Lot,x.Batch.Expiration,Location=x.Location.Name,x.Status,x.Quantity,x.Batch.UnitCost,Value=x.Quantity*x.Batch.UnitCost,Recall=db.RecallAffectedBatchs.Any(r=>r.BatchId==x.BatchId),DaysToExpiry=x.Batch.Expiration.DayNumber-Safety.Today.DayNumber}).Take(20000).ToListAsync());
api.MapGet("/fefo/{medicineId:guid}",async(Guid medicineId,PharmacyDb db)=>await db.InventoryBalances.Where(x=>x.Batch.MedicineId==medicineId&&x.Status==StockStatus.Available&&x.Quantity>0&&x.Batch.Expiration>=Safety.Today&&!db.RecallAffectedBatchs.Any(r=>r.BatchId==x.BatchId)).OrderBy(x=>x.Batch.Expiration).Select(x=>new{x.Id,x.BatchId,x.Batch.Lot,x.Batch.Expiration,x.Quantity,x.LocationId}).ToListAsync());
api.MapGet("/receiving",async(PharmacyDb db)=>await db.GoodsReceipts.Select(x=>new{x.Id,x.Reference,Supplier=x.Supplier.Name,x.PurchaseOrderId,x.MovementId,x.CreatedAt}).OrderByDescending(x=>x.CreatedAt).Take(10000).ToListAsync()).RequireAuthorization("Inventory");
api.MapGet("/transfers",async(PharmacyDb db)=>await db.StockMovements.Where(x=>x.Kind=="TransferIn"||x.Kind=="TransferOut").Select(x=>new{x.Id,x.BalanceId,x.Delta,x.Kind,x.Reference,x.CreatedAt}).Take(10000).ToListAsync()).RequireAuthorization("Inventory");
api.MapGet("/counts",async(PharmacyDb db)=>await db.StockAdjustments.Select(x=>new{x.Id,x.MovementId,x.Counted,x.Reason,x.CreatedAt}).Take(10000).ToListAsync()).RequireAuthorization("Catalog");
api.MapPost("/receiving",async(ReceiveInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Receive(r,a))).RequireAuthorization("Inventory");
api.MapPost("/orders",async(OrderInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.CreateOrder(r))).RequireAuthorization("Clinical");
api.MapGet("/orders",async(PharmacyDb db)=>await db.DispenseOrders.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(1000).ToListAsync()).RequireAuthorization("Clinical");
api.MapPost("/dispensing",async(DispenseInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Dispense(r,a))).RequireAuthorization("Clinical");
api.MapPost("/reservations",async(ReserveInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Reserve(r))).RequireAuthorization("Clinical");
api.MapPost("/transfers",async(MoveInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Transfer(r,a))).RequireAuthorization("Inventory");
api.MapPost("/counts",async(CountInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Count(r,a))).RequireAuthorization("Catalog");
api.MapPost("/returns",async(ReturnInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Return(r,a))).RequireAuthorization("Clinical");
api.MapPost("/recalls",async(RecallInput r,HttpContext c,InventoryService s)=>await Mutate(r,c,s,a=>s.Recall(r))).RequireAuthorization("Catalog");
api.MapGet("/recalls",async(PharmacyDb db)=>await db.RecallAffectedBatchs.Select(x=>new{x.Id,x.BatchId,x.RecallId,x.Recall.Reference,x.Recall.Reason,x.Recall.Closed,x.Batch.Lot,x.Batch.Medicine.GenericName}).ToListAsync());
api.MapPost("/reconciliation",async(HttpContext c,InventoryService s)=>await Mutate(new NamedInput("Ledger reconciliation"),c,s,a=>s.Reconcile(a))).RequireAuthorization("Report");
api.MapGet("/reconciliation",async(PharmacyDb db)=>await db.ReconciliationDiscrepancys.AsNoTracking().ToListAsync()).RequireAuthorization("Report");
api.MapGet("/suppliers",async(PharmacyDb db)=>await db.Suppliers.AsNoTracking().ToListAsync());
api.MapGet("/locations",async(PharmacyDb db)=>await db.InventoryLocations.AsNoTracking().ToListAsync());
api.MapPost("/suppliers",async(NamedInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{Safety.Require(!string.IsNullOrWhiteSpace(r.Name),"Name required.");var e=new Supplier{Name=r.Name};db.Suppliers.Add(e);return Task.FromResult<object>(e);})).RequireAuthorization("Inventory");
api.MapPost("/locations",async(NamedInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{Safety.Require(!string.IsNullOrWhiteSpace(r.Name),"Name required.");var e=new InventoryLocation{Name=r.Name};db.InventoryLocations.Add(e);return Task.FromResult<object>(e);})).RequireAuthorization("Catalog");
api.MapPost("/purchase-orders",async(PurchaseInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{Safety.Positive(r.OrderedQuantity);Safety.Require(!string.IsNullOrWhiteSpace(r.Reference),"Reference required.");var e=new PurchaseOrder{SupplierId=r.SupplierId,Reference=r.Reference,OrderedQuantity=r.OrderedQuantity};db.PurchaseOrders.Add(e);return Task.FromResult<object>(e);})).RequireAuthorization("Inventory");
api.MapGet("/purchase-orders",async(PharmacyDb db)=>await db.PurchaseOrders.AsNoTracking().ToListAsync());
api.MapPost("/workstations",async(WorkstationInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,async a=>{Safety.Require(!string.IsNullOrWhiteSpace(r.Name)&&r.Name.Length<=128,"Valid workstation name required.");var w=await db.Workstations.SingleOrDefaultAsync(x=>x.Name==r.Name);if(w==null){w=new Workstation{Name=r.Name};db.Workstations.Add(w);}w.LastSeen=DateTime.UtcNow;return w;}));
api.MapGet("/workstations",async(PharmacyDb db)=>await db.Workstations.AsNoTracking().ToListAsync());
api.MapGet("/scanners",async(PharmacyDb db)=>await db.ScannerConfigurations.AsNoTracking().ToListAsync());
api.MapPost("/scanners",async(ScannerInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{Safety.Require(r.Type is "HID" or "COM" or "Bluetooth HID" or "Vendor SDK","Unknown scanner type.");Safety.Require(!string.IsNullOrWhiteSpace(r.Name),"Name required.");Safety.Require(r.WorkstationId.ToString()==a.Workstation||Roles.Catalog.Contains(a.Role),"Configure only your workstation.");var scanner=new ScannerConfiguration{WorkstationId=r.WorkstationId,Name=r.Name,Enabled=r.Enabled,Type=r.Type,Port=r.Port,Suffix=r.Suffix};db.ScannerConfigurations.Add(scanner);return Task.FromResult<object>(scanner);}));
api.MapGet("/dashboard",async(PharmacyDb db)=>new{medicines=await db.Medicines.CountAsync(),stockUnits=await db.InventoryBalances.SumAsync(x=>x.Quantity),nearExpiryBatches=await db.InventoryBatchs.CountAsync(x=>x.Expiration<=Safety.Today.AddDays(90)&&x.Expiration>=Safety.Today),expiredBatches=await db.InventoryBatchs.CountAsync(x=>x.Expiration<Safety.Today),openRecalls=await db.RecallNotices.CountAsync(x=>!x.Closed),workstations=await db.Workstations.CountAsync(),integration="Mock only • Live Cerebro disabled"});
UserAdministration.Map(api);

api.MapGet("/settings",async(PharmacyDb db)=>await db.SystemSettings.AsNoTracking().ToListAsync()).RequireAuthorization("Catalog");
api.MapPost("/settings",async(SettingInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,async a=>{
 Safety.Require(a.Role==Roles.Chief,"Chief Pharmacist approval required.");Safety.Require(!string.IsNullOrWhiteSpace(r.ApprovalReference),"Written hospital approval reference required.");
 Safety.Require(r.Name=="ManualDispensingApproved"&&r.Value is "true" or "false","Unsupported setting.");var setting=await db.SystemSettings.SingleOrDefaultAsync(x=>x.Name==r.Name);if(setting==null){setting=new SystemSetting{Name=r.Name};db.SystemSettings.Add(setting);}setting.Value=r.Value;return setting;
})).RequireAuthorization("Clinical");
api.MapGet("/integration",async(PharmacyDb db)=>await db.IntegrationOutboxs.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(5000).ToListAsync()).RequireAuthorization("Report");
api.MapPost("/integration/test",async(ICerebroIntegration integration)=>await integration.TestConnectionAsync(default)).RequireAuthorization("Report");
api.MapPost("/integration/{id:guid}/acknowledge",async(Guid id,AcknowledgeInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(new{id,r},c,s,async a=>{Safety.Require(r.Confirmed&&!string.IsNullOrWhiteSpace(r.Reference),"Explicit vendor/manual posting reference required.");var o=await db.IntegrationOutboxs.SingleAsync(x=>x.Id==id);Safety.Require(o.State!=PostingState.Acknowledged,"Already acknowledged.");o.State=PostingState.Acknowledged;o.Detail="Manual acknowledgment: "+r.Reference;db.IntegrationAcknowledgments.Add(new IntegrationAcknowledgment{Outbox=o,Reference=r.Reference,Actor=a.UserId});return o;})).RequireAuthorization("Catalog");
api.MapGet("/audit",async(PharmacyDb db)=>await db.AuditEvents.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(10000).ToListAsync()).RequireAuthorization("Report");
api.MapGet("/audit/verify",async(PharmacyDb db)=>{var rows=await db.AuditEvents.AsNoTracking().OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id).ToListAsync();var hash="GENESIS";foreach(var row in rows){if(row.PreviousHash!=hash||row.Hash!=InventoryService.AuditHash(row))return Results.Json(new{valid=false,record=row.Id});hash=row.Hash;}return Results.Json(new{valid=true,count=rows.Count});}).RequireAuthorization("Report");
api.MapGet("/reports/{name}",async(string name,PharmacyDb db)=>Results.Json(await Reports.Rows(name,db))).RequireAuthorization("Report");
api.MapGet("/reports/{name}/csv",async(string name,PharmacyDb db)=>{
 var element=JsonSerializer.SerializeToElement(await Reports.Rows(name,db));var rows=element.EnumerateArray().ToArray();var csv=rows.Length==0?"No records\r\n":CsvAdapter.Export(new[]{rows[0].EnumerateObject().Select(x=>x.Name).ToArray()}.Concat(rows.Select(r=>r.EnumerateObject().Select(x=>x.Value.ToString()).ToArray())));return Results.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray(),"text/csv",$"DDHI-{name}-{Safety.Today}.csv");
}).RequireAuthorization("Report");

api.MapPost("/imports/medicines",async(CsvInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,a=>{
 Safety.Require(r.Confirmed&&!string.IsNullOrWhiteSpace(r.ApprovalReference),"Approved import reference and confirmation required.");
 using var parser=new Microsoft.VisualBasic.FileIO.TextFieldParser(new StringReader(r.Csv));parser.SetDelimiters(",");parser.HasFieldsEnclosedInQuotes=true;
 var header=parser.ReadFields()??throw new InvalidOperationException("Empty CSV.");
 var expected=new[]{"CerebroMedicineId","GenericName","BrandName","Strength","DosageForm","Route","Manufacturer","Category","BaseUnit","ReorderLevel"};
 var extended=expected.Concat(new[]{"ImportedBy","DistributedBy"}).ToArray();
 Safety.Require(header.SequenceEqual(expected)||header.SequenceEqual(extended),"CSV header must match samples/medicines.csv or samples/medicines-with-trade.csv exactly.");
 var count=0;while(!parser.EndOfData){var row=parser.ReadFields()!;Safety.Require(row.Length==header.Length&&row[0].Length>0&&row[1].Length>0&&row[3].Length>0&&row[4].Length>0&&row[8].Length>0,"Invalid catalog row.");
 Safety.Require(decimal.TryParse(row[9],System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out var level)&&level>=0,"Invalid reorder level.");
 var med=new Medicine{CerebroMedicineId=row[0],GenericName=row[1],BrandName=row[2],Strength=row[3],DosageForm=row[4],Route=row[5],Manufacturer=row[6],Category=row[7],BaseUnit=row[8],ReorderLevel=level,ImportedBy=header.Length==12?MedicineTradeText(row[10]):"",DistributedBy=header.Length==12?MedicineTradeText(row[11]):""};db.Medicines.Add(med);db.CerebroMedicineMappings.Add(new CerebroMedicineMapping{Medicine=med,ExternalId=row[0],Approved=true});count++;Safety.Require(count<=10000,"Maximum 10000 rows per import.");
 }return Task.FromResult<object>(new{imported=count,reference=r.ApprovalReference});
})).RequireAuthorization("Catalog");
api.MapPost("/imports/stock-snapshot",async(SnapshotInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(r,c,s,async a=>{
 Safety.Require(r.Confirmed&&!string.IsNullOrWhiteSpace(r.Reference)&&r.Lines.Length<=20000,"Approved snapshot reference required.");
 var run=new ReconciliationRun{Reference=r.Reference,Actor=a.UserId};db.ReconciliationRuns.Add(run);
 Safety.Require(r.Lines.Select(x=>x.BalanceId).Distinct().Count()==r.Lines.Length,"Duplicate balance in snapshot.");
 foreach(var line in r.Lines){Safety.Require(line.Quantity>=0,"Snapshot quantity cannot be negative.");var balance=await db.InventoryBalances.SingleAsync(x=>x.Id==line.BalanceId);if(balance.Quantity!=line.Quantity)db.ReconciliationDiscrepancys.Add(new ReconciliationDiscrepancy{Run=run,Balance=balance,Expected=line.Quantity,Actual=balance.Quantity});}
 return new{run.Id,message="Comparison only. Central balances and Cerebro unchanged."};
})).RequireAuthorization("Report");
api.MapPut("/scanners/{id:guid}",async(Guid id,ScannerInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(new{id,r},c,s,async a=>{
 var scanner=await db.ScannerConfigurations.SingleAsync(x=>x.Id==id);Safety.Require(scanner.WorkstationId.ToString()==a.Workstation||Roles.Catalog.Contains(a.Role),"Configure only your registered workstation.");Safety.Require(r.Type is "HID" or "COM" or "Bluetooth HID" or "Vendor SDK","Unknown scanner type.");scanner.Enabled=r.Enabled;scanner.Type=r.Type;scanner.Port=r.Port;scanner.Suffix=r.Suffix;scanner.Name=r.Name;return scanner;
}));

api.MapPost("/medicines/{id:guid}/status",async(Guid id,MedicineStatusInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(new{id,r},c,s,async a=>{Safety.Require(r.Confirmed&&!string.IsNullOrWhiteSpace(r.Reason),"Confirmed authorized reason required.");var med=await db.Medicines.SingleAsync(x=>x.Id==id);med.ActiveStatus=r.ActiveStatus;med.UpdatedAt=DateTime.UtcNow;return new{med.Id,med.ActiveStatus};})).RequireAuthorization("Catalog");


api.MapPost("/stock-status",async(StockStateInput r,HttpContext c,InventoryService s,LifecycleService lifecycle)=>await Mutate(r,c,s,a=>lifecycle.ChangeStatus(r,a))).RequireAuthorization("Catalog");
api.MapPost("/disposals",async(DisposeInput r,HttpContext c,InventoryService s,LifecycleService lifecycle)=>await Mutate(r,c,s,a=>lifecycle.Dispose(r,a))).RequireAuthorization("Catalog");
api.MapPost("/recalls/{id:guid}/close",async(Guid id,ReasonInput r,HttpContext c,InventoryService s,LifecycleService lifecycle)=>await Mutate(new{id,r},c,s,a=>lifecycle.CloseRecall(id,r))).RequireAuthorization("Catalog");
api.MapGet("/reservations",async(PharmacyDb db)=>await db.StockReservations.Select(x=>new{x.Id,x.OrderId,x.BalanceId,x.Quantity,x.ExpiresAt,x.Released}).Take(10000).ToListAsync()).RequireAuthorization("Clinical");
api.MapPost("/reservations/{id:guid}/release",async(Guid id,ReasonInput r,HttpContext c,InventoryService s,LifecycleService lifecycle)=>await Mutate(new{id,r},c,s,a=>lifecycle.ReleaseReservation(id,r))).RequireAuthorization("Clinical");
api.MapPost("/movements/{id:guid}/reverse",async(Guid id,ReasonInput r,HttpContext c,InventoryService s,LifecycleService lifecycle)=>await Mutate(new{id,r},c,s,a=>lifecycle.Reverse(id,r,a))).RequireAuthorization("Catalog");
api.MapGet("/movements",async(PharmacyDb db)=>await db.StockMovements.Select(x=>new{x.Id,x.BalanceId,x.Delta,x.Kind,x.Reference,x.Reason,x.ReversesMovementId,x.CreatedAt}).OrderByDescending(x=>x.CreatedAt).Take(20000).ToListAsync()).RequireAuthorization("Catalog");
api.MapPost("/scanners/{id:guid}/configure",async(Guid id,ScannerInput r,HttpContext c,InventoryService s,PharmacyDb db)=>await Mutate(new{id,r},c,s,async a=>{var scanner=await db.ScannerConfigurations.SingleAsync(x=>x.Id==id);Safety.Require(scanner.WorkstationId.ToString()==a.Workstation||Roles.Catalog.Contains(a.Role),"Configure only your workstation.");Safety.Require(r.WorkstationId==scanner.WorkstationId,"Scanner workstation cannot be reassigned.");Safety.Require(r.Type is "HID" or "COM" or "Bluetooth HID" or "Vendor SDK","Unknown scanner type.");scanner.Enabled=r.Enabled;scanner.Type=r.Type;scanner.Port=r.Port;scanner.Suffix=r.Suffix;scanner.Name=r.Name;return new{scanner.Id,scanner.Enabled};}));
await app.RunAsync();

public record MedicineStatusInput(bool ActiveStatus,string Reason,bool Confirmed);
public record UserLockInput(bool Locked,string Reason,bool Confirmed);

public record CsvInput(string Csv,string ApprovalReference,bool Confirmed);
public record SnapshotLine(Guid BalanceId,decimal Quantity);
public record SnapshotInput(string Reference,SnapshotLine[] Lines,bool Confirmed);

public record LoginInput(string Email,string Password);
public record ScanInput(string Barcode);
public record MutationEnvelope<T>(string Path,T Request);
public partial class Program;
