using System.Security.Claims;
using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;

static class ClinicalReferenceEndpoints
{
    static Actor Actor(HttpContext c)=>new(c.User.FindFirstValue(ClaimTypes.NameIdentifier)??"",ActorRoles.ForOperation(c.User.FindAll(ClaimTypes.Role).Select(x=>x.Value),"/api/clinical"),AuditEndpoints.Workstation(c),c.Connection.RemoteIpAddress?.ToString()??"",AuditEndpoints.Session(c));
    static async Task<IResult> Change(HttpContext c,InventoryService s,object safeInput,Func<Actor,Task<object>> action){var actor=Actor(c);var json=await s.Execute(c.Request.Headers["Idempotency-Key"].ToString(),actor,new{Path=c.Request.Path.ToString(),Input=safeInput},()=>action(actor));return Results.Content(json,"application/json");}
    public static void Map(RouteGroupBuilder api)
    {
        var group=api.MapGroup("/clinical").RequireAuthorization("Reference");
        group.MapGet("/status",async(PharmacyDb db)=>await new ClinicalReferenceQueries(db).Status());
        group.MapGet("/policy",async(PharmacyDb db)=>await new ClinicalSafetyService(db).Policy());
        group.MapPost("/policy",async(ClinicalSafetyPolicy input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,input,a=>new ClinicalSafetyService(db).SetPolicy(input,a))).RequireAuthorization("Clinical");
        group.MapGet("/search",async(string? q,bool? stock,string? directory,PharmacyDb db)=>await new ClinicalReferenceQueries(db).Search(q,stock??false,directory));
        group.MapGet("/monograph/{id:guid}",async(Guid id,PharmacyDb db)=>await new ClinicalReferenceQueries(db).Monograph(id));
        group.MapPost("/barcode",async(BarcodeQuery input,PharmacyDb db)=>{
            var result=await new BarcodeLookup(db).Resolve(input.Barcode);
            if(result.Medicine!=null)return Results.Json(new{MedicineId=(Guid?)result.Medicine.Id,Status=result.Result.Status,Message="Exact registered inventory product selected. Its clinical mapping may still be incomplete."});
            if(result.Result.Status=="Ambiguous")return Results.Json(new{MedicineId=(Guid?)null,Status=result.Result.Status,Message=result.Result.Message});
            var publication=await new ClinicalContentService(db).Current();var codes=DDHI.Pharmacy.Barcode.BarcodeIdentity.EquivalentCodes(result.Data);var products=await db.ClinicalPackageRecords.AsNoTracking().Where(x=>codes.Contains(x.Barcode)&&x.Product.VersionId==(publication==null?Guid.Empty:publication.VersionId)).Select(x=>x.ProductId).Distinct().ToListAsync();
            return products.Count==1?Results.Json(new{MedicineId=(Guid?)products[0],Status="Reference product only",Message="Published reference package selected; hospital inventory identity/stock is not implied."}):Results.Json(new{MedicineId=(Guid?)null,Status=products.Count>1?"Ambiguous reference packages":result.Result.Status,Message=products.Count>1?"Resolve conflicting published package identities before selecting a medicine.":result.Result.Message});
        });
        group.MapGet("/ingredients",async(PharmacyDb db)=>{var current=await new ClinicalContentService(db).Current();return await db.DrugIngredients.AsNoTracking().Where(x=>x.VersionId==(current==null?Guid.Empty:current.VersionId)).Select(x=>new{x.Key,x.Name}).ToListAsync();});
        group.MapGet("/products",async(PharmacyDb db)=>{var current=await new ClinicalContentService(db).Current();return await db.ClinicalProducts.AsNoTracking().Include(x=>x.Drug).Where(x=>x.VersionId==(current==null?Guid.Empty:current.VersionId)).Select(x=>new{x.Key,x.Brand,Drug=x.Drug.Name,x.Drug.Strength,x.Drug.Form,x.Drug.Route}).ToListAsync();});
        group.MapPost("/packages/preview",(PackageImport input,PharmacyDb db)=>new ClinicalContentService(db).Preview(input)).RequireAuthorization("Clinical");
        group.MapPost("/packages",async(PackageImport input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,new{input.Sha256,input.RightsConfirmed,input.ApprovalReference},a=>new ClinicalContentService(db).Stage(input,a))).RequireAuthorization("Clinical");
        group.MapGet("/versions",async(PharmacyDb db)=>await db.ClinicalContentVersions.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Version,x.State,x.Digest,x.Author,x.Reviewer,x.Approver,x.ReviewedAt,x.ApprovedAt,x.CreatedAt}).Take(500).ToListAsync()).RequireAuthorization("Audit");
        group.MapGet("/versions/{id:guid}/package",async(Guid id,PharmacyDb db)=>{var row=await db.ClinicalContentVersions.AsNoTracking().SingleAsync(x=>x.Id==id);return Results.Content(row.PayloadJson,"application/json");}).RequireAuthorization("Clinical");
        group.MapPost("/versions/{id:guid}/decision",async(Guid id,ContentDecision input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,new{id,input},a=>new ClinicalContentService(db).Decide(id,input,a))).RequireAuthorization("Clinical");
        group.MapPost("/mappings",async(ClinicalMappingInput input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,input,a=>new ClinicalContentService(db).Map(input,a))).RequireAuthorization("Clinical");
        group.MapPost("/screen",async(ClinicalScreenInput input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,new{MedicationIds=input.Medications?.Select(x=>x.MedicineId),RequestHash=InventoryService.Hash(JsonSerializer.Serialize(input))},a=>new ClinicalSafetyService(db).Screen(input,a)));
        group.MapPost("/interventions",async(ClinicalInterventionInput input,HttpContext c,PharmacyDb db,InventoryService s)=>await Change(c,s,new{input.ScreenId,input.Decision},a=>new ClinicalSafetyService(db).Intervene(input,a))).RequireAuthorization("Clinical");
        group.MapGet("/screenings",async(PharmacyDb db)=>await db.ClinicalScreenings.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Actor,x.Workstation,x.EncounterReference,x.PublicationId,x.CreatedAt,x.Digest}).Take(1000).ToListAsync()).RequireAuthorization("Audit");
        group.MapGet("/screenings/{id:guid}",async(Guid id,HttpContext c,PharmacyDb db)=>{var row=await db.ClinicalScreenings.AsNoTracking().SingleAsync(x=>x.Id==id);Safety.Require(row.Actor==Actor(c).UserId||c.User.IsInRole(Roles.Auditor)||c.User.IsInRole(Roles.Chief),"This screening belongs to another staff member.");return new{row.Id,row.CreatedAt,row.Actor,row.Workstation,row.EncounterReference,Input=JsonSerializer.Deserialize<JsonElement>(row.InputJson),Resolution=JsonSerializer.Deserialize<JsonElement>(row.ResolutionJson),Result=JsonSerializer.Deserialize<JsonElement>(row.ResultJson)};});
        group.MapGet("/interventions",async(PharmacyDb db)=>await db.PharmacistInterventions.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(1000).ToListAsync()).RequireAuthorization("Audit");
        group.MapGet("/reviews",async(PharmacyDb db)=>await db.ClinicalReviewRecords.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(1000).ToListAsync()).RequireAuthorization("Audit");
        group.MapPost("/favorite",async(FavoriteInput input,HttpContext c,InventoryService s,PharmacyDb db)=>await Change(c,s,input,async a=>{await new ClinicalContentService(db).Resolve([input.MedicineId],await new ClinicalContentService(db).Current());var row=await db.ReferencePreferences.SingleOrDefaultAsync(x=>x.Actor==a.UserId&&x.MedicineId==input.MedicineId);if(row==null){row=new(){Actor=a.UserId,MedicineId=input.MedicineId};db.ReferencePreferences.Add(row);}row.Favorite=input.Favorite;row.CreatedAt=DateTime.UtcNow;return new{row.Id,row.MedicineId,row.Favorite};}));
        group.MapGet("/favorites",async(HttpContext c,PharmacyDb db)=>await db.ReferencePreferences.AsNoTracking().Where(x=>x.Actor==Actor(c).UserId&&x.Favorite).OrderByDescending(x=>x.CreatedAt).Select(x=>new{Id=x.MedicineId,x.CreatedAt}).Take(200).ToListAsync());
    }
    public sealed record BarcodeQuery(string Barcode);
    public sealed record FavoriteInput(Guid MedicineId,bool Favorite);
}
