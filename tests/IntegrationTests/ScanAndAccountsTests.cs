using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Barcode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public partial class WorkflowTests
{
    static string Code(string indicator="0") {var core=indicator+string.Concat(Guid.NewGuid().ToByteArray().Take(12).Select(b=>(b%10).ToString()));return Enumerable.Range(0,10).Select(n=>core+n).Single(BarcodeParser.ValidGtin);}
    async Task<HttpClient> LoginAccount(string email){var client=factory.CreateClient();using var response=await client.PostAsJsonAsync("/auth/login",new{email,password="Test-Only!Password123"});Assert.True(response.IsSuccessStatusCode,await response.Content.ReadAsStringAsync());var token=await response.Content.ReadFromJsonAsync<JsonElement>();client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token.GetProperty("accessToken").GetString());client.DefaultRequestHeaders.Add("X-Workstation",Guid.NewGuid().ToString());return client;}
    async Task<(string Id,string Email,HttpClient Client)> Account(string role=Roles.Reader){var email=Guid.NewGuid()+"@test.invalid";var created=await Post(admin,"users",new UserInput(email,"Test-Only!Password123",role));return(created.GetProperty("Id").GetString()!,email,await LoginAccount(email));}
    async Task<JsonElement> Profile(string id)=>await admin.GetFromJsonAsync<JsonElement>("/api/users/"+id);
    static UserEditInput Edit(JsonElement profile,string[] roles,string? email=null)=>new(email??profile.GetProperty("email").GetString()!,"QA staff","+63 900 123 4567",roles,"QA account update",true,profile.GetProperty("version").GetString()!);
    [Fact] public async Task AdministratorCanEditProfileAndRolesAndOldSessionIsRejected()
    {
        var account=await Account();using var session=account.Client;var profile=await Profile(account.Id);var email=Guid.NewGuid()+"@test.invalid";
        await Post(admin,"users/"+account.Id+"/edit",Edit(profile,[Roles.Pharmacist,Roles.Clerk],email));
        Assert.Equal(HttpStatusCode.Unauthorized,(await session.GetAsync("/api/dashboard")).StatusCode);
        using var oldLogin=await factory.CreateClient().PostAsJsonAsync("/auth/login",new{email=account.Email,password="Test-Only!Password123"});Assert.Equal(HttpStatusCode.Unauthorized,oldLogin.StatusCode);
        using var fresh=await LoginAccount(email);Assert.Equal(HttpStatusCode.OK,(await fresh.GetAsync("/api/orders")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await fresh.GetAsync("/api/users")).StatusCode);
        var updated=await Profile(account.Id);Assert.Equal("QA staff",updated.GetProperty("fullName").GetString());Assert.Equal("+63 900 123 4567",updated.GetProperty("phoneNumber").GetString());
        await Post(admin,"users/"+account.Id+"/edit",Edit(updated,[Roles.Pharmacist,Roles.Clerk],email) with{FullName="",PhoneNumber=""});
        Assert.Equal(HttpStatusCode.Unauthorized,(await fresh.GetAsync("/api/dashboard")).StatusCode);
        var cleared=await Profile(account.Id);Assert.Equal("",cleared.GetProperty("fullName").GetString());Assert.Equal("",cleared.GetProperty("phoneNumber").GetString());
    }
    [Fact] public async Task AccountEditsRejectStaleVersionsDuplicateEmailAndReaderAccess()
    {
        var a=await Account();using var session=a.Client;var b=await Account();using var second=b.Client;var original=await Profile(a.Id);
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"users/"+a.Id+"/edit",Edit(original,[Roles.SystemAdmin]))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(admin,"users/"+a.Id+"/edit",Edit(original,[Roles.Reader],b.Email))).StatusCode);
        await Post(admin,"users/"+a.Id+"/edit",Edit(original,[Roles.Reader]));
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(admin,"users/"+a.Id+"/edit",Edit(original,[Roles.Pharmacist]))).StatusCode);
        var current=await Profile(a.Id);Assert.Equal(a.Email,current.GetProperty("email").GetString());Assert.Contains(Roles.Reader,current.GetProperty("roles").EnumerateArray().Select(x=>x.GetString()));
    }
    [Fact] public async Task AccountDeletionRemovesLoginButPreservesAuditHistoryAndProtectsSelf()
    {
        var account=await Account();using var target=account.Client;var profile=await Profile(account.Id);
        var input=new UserDeleteInput(account.Email,"QA remove synthetic account",true,profile.GetProperty("version").GetString()!);
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"users/"+account.Id+"/delete",input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(admin,"users/"+account.Id+"/delete",input with{ConfirmationEmail="wrong@test.invalid"})).StatusCode);
        await Post(admin,"users/"+account.Id+"/delete",input);
        Assert.Equal(HttpStatusCode.Unauthorized,(await target.GetAsync("/api/dashboard")).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.False(await db.Users.AnyAsync(u=>u.Id==account.Id));Assert.True(await db.AuditEvents.AnyAsync(e=>e.Actor==account.Id));
        var me=await admin.GetFromJsonAsync<JsonElement>("/api/me");var own=await Profile(me.GetProperty("id").GetString()!);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(admin,"users/"+own.GetProperty("id").GetString()+"/delete",new UserDeleteInput(own.GetProperty("email").GetString()!,"QA self delete blocked",true,own.GetProperty("version").GetString()!))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(admin,"users/"+own.GetProperty("id").GetString()+"/edit",Edit(own,[Roles.Reader]))).StatusCode);
    }
    [Fact] public async Task MixedTechnicalAndClinicalRoleUsesTheClinicalRoleForDispensing()
    {
        await Policy();var account=await Account();using var old=account.Client;await Post(admin,"users/"+account.Id+"/edit",Edit(await Profile(account.Id),[Roles.SystemAdmin,Roles.Pharmacist]));
        using var mixed=await LoginAccount(account.Email);var stock=await Stock();var order=await Order(stock.Medicine);
        await Post(mixed,"dispensing",new DispenseInput(order,stock.Balance,1,"","ManualSelection",true,true,"QA mixed clinical role"));
    }
    [Fact] public async Task ScansMatchApprovedLegacyZeroPaddedGtinAndReturnReadableFields()
    {
        var stock=await Stock(10);var gtin=Code();using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();db.MedicineBarcodes.Add(new MedicineBarcode{MedicineId=stock.Medicine,Barcode=gtin[1..],Packaging="box",ConversionFactor=10,ApprovedBy="QA approved legacy"});await db.SaveChangesAsync();}
        var workstation=await Post(reader,"workstations",new WorkstationInput("QA-scan-"+Guid.NewGuid()));reader.DefaultRequestHeaders.Remove("X-Workstation");reader.DefaultRequestHeaders.Add("X-Workstation",workstation.GetProperty("Id").GetGuid().ToString());
        var response=await Post(reader,"scan",new{barcode=gtin});var result=response.GetProperty("result");
        Assert.Equal("Matched",result.GetProperty("Status").GetString());Assert.Equal(stock.Medicine,result.GetProperty("Medicine").GetProperty("Id").GetGuid());Assert.Equal(10,result.GetProperty("Package").GetProperty("BaseUnitsPerPackage").GetDecimal());Assert.Equal(JsonValueKind.Null,result.GetProperty("EncodedExpiration").ValueKind);Assert.Single(result.GetProperty("Stock").EnumerateArray());
        var denied=await Raw(chief,"barcodes",new BarcodeInput(stock.Medicine,gtin,"box",10,true));Assert.Equal(HttpStatusCode.Conflict,denied.StatusCode);
        using var scope2=factory.Services.CreateScope();var db2=scope2.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Equal(10,(await db2.InventoryBalances.SingleAsync(x=>x.Id==stock.Balance)).Quantity);
    }
    [Fact] public async Task ScansDoNotGuessPackagingIndicatorsAndRejectAmbiguousAliases()
    {
        var one=await Stock();var two=await Stock();var code=Code();await Post(chief,"barcodes",new BarcodeInput(one.Medicine,code,"unit",1,true));
        var w=await Post(chief,"workstations",new WorkstationInput("QA-scan-"+Guid.NewGuid()));chief.DefaultRequestHeaders.Remove("X-Workstation");chief.DefaultRequestHeaders.Add("X-Workstation",w.GetProperty("Id").GetGuid().ToString());
        var packageCore="1"+code.Substring(1,12);var package=Enumerable.Range(0,10).Select(n=>packageCore+n).Single(BarcodeParser.ValidGtin);
        var unknown=(await Post(chief,"scan",new{barcode=package})).GetProperty("result");Assert.Equal("NotRegistered",unknown.GetProperty("Status").GetString());Assert.Equal(JsonValueKind.Null,unknown.GetProperty("Medicine").ValueKind);
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();db.MedicineBarcodes.Add(new MedicineBarcode{MedicineId=two.Medicine,Barcode=code[1..],Packaging="unit",ConversionFactor=1,ApprovedBy="QA conflicting legacy"});await db.SaveChangesAsync();}
        var conflict=(await Post(chief,"scan",new{barcode=code})).GetProperty("result");Assert.Equal("Ambiguous",conflict.GetProperty("Status").GetString());Assert.Equal(JsonValueKind.Null,conflict.GetProperty("Medicine").ValueKind);
        await Policy();var order=await Order(one.Medicine);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",new DispenseInput(order,one.Balance,1,code,"ManualBarcode",true,true,"QA ambiguous blocked"))).StatusCode);
    }
    [Fact] public async Task Gs1AndRegisteredCode128LookupsPreserveBatchExpiryAndCase()
    {
        var stock=await Stock();var code=Code();await Post(chief,"barcodes",new BarcodeInput(stock.Medicine,code,"unit",1,true));
        var w=await Post(chief,"workstations",new WorkstationInput("QA-scan-"+Guid.NewGuid()));chief.DefaultRequestHeaders.Remove("X-Workstation");chief.DefaultRequestHeaders.Add("X-Workstation",w.GetProperty("Id").GetGuid().ToString());
        var gs1="(01)"+code+"(10)LOT(17)"+Safety.Today.AddDays(365).ToString("yyMMdd")+"(21)QAserial";
        var result=(await Post(chief,"scan",new{barcode=gs1})).GetProperty("result");Assert.Equal("LOT",result.GetProperty("EncodedLot").GetString());Assert.Equal("QAserial",result.GetProperty("Serial").GetString());Assert.Single(result.GetProperty("Stock").EnumerateArray());
        var raw="QaPack-"+Guid.NewGuid().ToString("N");await Post(chief,"barcodes",new BarcodeInput(stock.Medicine,raw,"pack",1,true,"Code128"));
        Assert.Equal("Matched",(await Post(chief,"scan",new{barcode=raw+"\r\n"})).GetProperty("result").GetProperty("Status").GetString());
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"scan",new{barcode=raw.ToLowerInvariant()})).StatusCode);
    }
}
