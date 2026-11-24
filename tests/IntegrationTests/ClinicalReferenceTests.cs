using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public partial class WorkflowTests
{
    static PackageImport Import(ClinicalPackage package){var json=JsonSerializer.Serialize(package);return new(json,InventoryService.Hash(json),true,"Synthetic engineering permission only");}
    async Task<Guid> PublishFixture(ClinicalPackage package)
    {
        var version=await Post(chief,"clinical/packages",Import(package));var id=version.GetProperty("Id").GetGuid();
        await Post(chief,"clinical/versions/"+id+"/decision",new ContentDecision("Submit","Synthetic test submission",true));using var reviewer=await User(Roles.Chief);
        foreach(var action in new[]{"Review","Approve","Publish"})await Post(reviewer,"clinical/versions/"+id+"/decision",new ContentDecision(action,"Fictional engineering fixture; not clinical approval",true));return id;
    }
    [Fact] public async Task ClinicalImportPreviewAndApprovalSeparationAreEnforced()
    {
        var package=ClinicalFixtures.Package();var input=Import(package);
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"clinical/packages",input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"clinical/packages",input with{Sha256="wrong"})).StatusCode);
        var preview=await Post(chief,"clinical/packages/preview",input);Assert.Equal(package.Version,preview.GetProperty("version").GetString());
        using(var scope=factory.Services.CreateScope())Assert.False(await scope.ServiceProvider.GetRequiredService<PharmacyDb>().ClinicalContentVersions.AnyAsync(x=>x.Version==package.Version));
        var staged=await Post(chief,"clinical/packages",input);var id=staged.GetProperty("Id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"clinical/versions/"+id+"/decision",new ContentDecision("Publish","Attempt shortcut",true))).StatusCode);
        await Post(chief,"clinical/versions/"+id+"/decision",new ContentDecision("Submit","Synthetic review",true));
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"clinical/versions/"+id+"/decision",new ContentDecision("Review","Self review refused",true))).StatusCode);
        using var reviewer=await User(Roles.Chief);foreach(var action in new[]{"Review","Approve","Publish"})await Post(reviewer,"clinical/versions/"+id+"/decision",new ContentDecision(action,"Synthetic fixture verification only",true));
        using var finalScope=factory.Services.CreateScope();var db=finalScope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Equal("Published",(await db.ClinicalContentVersions.SingleAsync(x=>x.Id==id)).State);
        await Assert.ThrowsAnyAsync<Exception>(()=>db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"ClinicalContentVersions\" SET \"PayloadJson\" = '{{}}' WHERE \"Id\" = {id}"));
        await Assert.ThrowsAnyAsync<Exception>(()=>db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"ClinicalRuleRecords\" WHERE \"VersionId\" = {id}"));
    }
    [Fact] public async Task ClinicalUnknownCoverageCannotSilentlyApproveDispensing()
    {
        await Policy();var stock=await Stock(10);var order=await Order(stock.Medicine,5);var request=new DispenseInput(order,stock.Balance,1,"","ManualSelection",true,true,"QA verification");
        Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,request)).StatusCode);
        var screen=await Post(chief,"clinical/screen",new ClinicalScreenInput([new(stock.Medicine)],"test-encounter",OrderId:order,BalanceId:stock.Balance,IssueQuantity:1));var id=screen.GetProperty("Id").GetGuid();Assert.False(screen.GetProperty("Result").GetProperty("Complete").GetBoolean());
        Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,request with{SafetyAssessmentId=id})).StatusCode);
        await Post(chief,"clinical/policy",new ClinicalSafetyPolicy(false,true,"Synthetic hold policy"));
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"clinical/interventions",new ClinicalInterventionInput(id,"Proceed with documented review","QA","","QA",true,true))).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Equal(10,(await db.InventoryBalances.SingleAsync(x=>x.Id==stock.Balance)).Quantity);
    }
    [Fact] public async Task ClinicalAssessmentBindsOrderBrandBatchQuantityUserAndPolicy()
    {
        await Policy();var stock=await Stock(10);var order=await Order(stock.Medicine,5);var original=new DispenseInput(order,stock.Balance,1,"","ManualSelection",true,true,"QA verification");var prepared=await ReviewFixtureDispense(chief,original);
        Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,prepared with{Quantity=2})).StatusCode);
        using var other=await User(Roles.Pharmacist);Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(other,prepared)).StatusCode);
        await Post(chief,"clinical/policy",new ClinicalSafetyPolicy(false,true,"Revoke synthetic manual-review policy"));Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,prepared)).StatusCode);
        using var scope=factory.Services.CreateScope();Assert.Equal(10,(await scope.ServiceProvider.GetRequiredService<PharmacyDb>().InventoryBalances.SingleAsync(x=>x.Id==stock.Balance)).Quantity);
    }
    [Fact] public async Task ClinicalReviewedIssueCommitsOneAssessmentAndImmutableHistory()
    {
        await Policy();var stock=await Stock(10);var order=await Order(stock.Medicine,5);var prepared=await ReviewFixtureDispense(chief,new(order,stock.Balance,1,"","ManualSelection",true,true,"QA verification"));var key=Guid.NewGuid().ToString();
        using var first=await DirectDispense(chief,prepared,key);Assert.True(first.IsSuccessStatusCode,await first.Content.ReadAsStringAsync());var body=await first.Content.ReadAsStringAsync();using var replay=await DirectDispense(chief,prepared,key);Assert.True(replay.IsSuccessStatusCode);Assert.Equal(body,await replay.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,prepared)).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Equal(9,(await db.InventoryBalances.SingleAsync(x=>x.Id==stock.Balance)).Quantity);Assert.Single(await db.DispenseTransactions.Where(x=>x.SafetyAssessmentId==prepared.SafetyAssessmentId).ToListAsync());
        await Assert.ThrowsAnyAsync<Exception>(()=>db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"ClinicalScreenings\" SET \"ResultJson\" = '{{}}' WHERE \"Id\" = {prepared.SafetyAssessmentId}"));
        Assert.True((await chief.GetFromJsonAsync<JsonElement>("/api/audit/verify")).GetProperty("valid").GetBoolean());
    }
    [Fact] public async Task ClinicalMappingDoesNotDuplicateInventoryAndRejectsWrongStrength()
    {
        var package=ClinicalFixtures.Package();var version=await PublishFixture(package);var stock=await Stock(10);
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var med=await db.Medicines.SingleAsync(x=>x.Id==stock.Medicine);med.Strength="20 mg";await db.SaveChangesAsync();}
        var input=new ClinicalMappingInput(stock.Medicine,"fixture-product","QA exact identity verification",true);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"clinical/mappings",input)).StatusCode);
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var med=await db.Medicines.SingleAsync(x=>x.Id==stock.Medicine);med.Strength="10 mg";med.GenericName=package.Drugs[0].Name;await db.SaveChangesAsync();}
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(admin,"clinical/mappings",input)).StatusCode);await Post(chief,"clinical/mappings",input);
        var monograph=await reader.GetFromJsonAsync<JsonElement>("/api/clinical/monograph/"+stock.Medicine);Assert.Equal("Verified current inventory mapping",monograph.GetProperty("mapping").GetString());Assert.Equal(package.Version,monograph.GetProperty("databaseVersion").GetString());Assert.Equal("Published",monograph.GetProperty("approvalStatus").GetString());
        using var final=factory.Services.CreateScope();var finalDb=final.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Single(await finalDb.DrugClinicalMappings.Where(x=>x.MedicineId==stock.Medicine).ToListAsync());Assert.Equal(10,(await finalDb.InventoryBalances.SingleAsync(x=>x.Id==stock.Balance)).Quantity);
    }
    [Fact] public async Task ClinicalPublicationRollbackPreservesHistoryAndInvalidatesOldScreen()
    {
        var first=ClinicalFixtures.Package();var a=await PublishFixture(first);var stock=await Stock(10);await Policy();var order=await Order(stock.Medicine,5);var prepared=await ReviewFixtureDispense(chief,new(order,stock.Balance,1,"","ManualSelection",true,true,"QA"));
        var b=await PublishFixture(ClinicalFixtures.Package());Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,prepared)).StatusCode);
        await Post(chief,"clinical/versions/"+a+"/decision",new ContentDecision("Restore publication","Synthetic rollback check",true));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.True(await db.ClinicalScreenings.AnyAsync(x=>x.Id==prepared.SafetyAssessmentId));Assert.Equal(a,(await db.ClinicalPublications.OrderByDescending(x=>x.Revision).FirstAsync()).VersionId);Assert.True(await db.ClinicalContentVersions.AnyAsync(x=>x.Id==b));
        Assert.Equal(HttpStatusCode.Conflict,(await DirectDispense(chief,prepared)).StatusCode);
    }
    [Fact] public async Task ClinicalIndependentWorkstationsGetConsistentLocalResultsWithoutInferringHistory()
    {
        var a=await Stock();var b=await Stock();var request=new ClinicalScreenInput([new(a.Medicine),new(b.Medicine)],"QA-minimal-encounter");
        var results=await Task.WhenAll(Post(chief,"clinical/screen",request),Post(reader,"clinical/screen",request));Assert.Equal(results[0].GetProperty("Result").ToString(),results[1].GetProperty("Result").ToString());Assert.Equal(2,results[0].GetProperty("Result").GetProperty("TotalMedications").GetInt32());
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"clinical/interventions",new ClinicalInterventionInput(results[1].GetProperty("Id").GetGuid(),"Proceed with documented review","QA","","QA",true,true))).StatusCode);
    }
    [Fact] public async Task ClinicalBarcodeUsesApprovedCatalogAndUnknownRegistrationRemainsUnknown()
    {
        var stock=await Stock();var code=Code();await Post(chief,"barcodes",new BarcodeInput(stock.Medicine,code,"box",10,true));var found=await Post(reader,"clinical/barcode",new{barcode=code});Assert.Equal(stock.Medicine,found.GetProperty("medicineId").GetGuid());
        var mono=await reader.GetFromJsonAsync<JsonElement>("/api/clinical/monograph/"+stock.Medicine);Assert.Null(mono.GetProperty("monograph").Deserialize<object>());Assert.Contains("Unmapped",mono.GetProperty("mapping").GetString());
        var package=ClinicalFixtures.Package();var packageCode=Code();package=package with{Packages=[new("fixture-package","fixture-product",packageCode,"box",10)]};await PublishFixture(package);
        var reference=await Post(reader,"clinical/barcode",new{barcode=packageCode});Assert.Equal("Reference product only",reference.GetProperty("status").GetString());
        var referenceMonograph=await reader.GetFromJsonAsync<JsonElement>("/api/clinical/monograph/"+reference.GetProperty("medicineId").GetGuid());Assert.Equal(JsonValueKind.Null,referenceMonograph.GetProperty("inventory").ValueKind);
        var referenceSearch=await reader.GetFromJsonAsync<JsonElement>("/api/clinical/search?q="+packageCode);Assert.Contains(referenceSearch.GetProperty("rows").EnumerateArray(),x=>x.GetProperty("id").GetGuid()==reference.GetProperty("medicineId").GetGuid());

        var search=await reader.GetFromJsonAsync<JsonElement>("/api/clinical/search?q="+code);Assert.Contains(search.GetProperty("rows").EnumerateArray(),x=>x.GetProperty("id").GetGuid()==stock.Medicine);
    }
}

