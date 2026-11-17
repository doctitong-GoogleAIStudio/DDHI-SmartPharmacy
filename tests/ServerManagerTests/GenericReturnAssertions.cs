using DDHI.Pharmacy.ServerManager;
using Npgsql;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
internal static class GenericReturnAssertions
{
    internal static async Task Exercise(ServerController controller,ServerProfile profile,string root,HttpClient client,Func<string,object,Task<JsonElement>> post,Guid originalMedicine)
    {
        var brand=await post("/api/medicines/"+originalMedicine+"/brands",new{brandName="QA alternate",manufacturer="QA alternate maker",importedBy="",distributedBy="",reorderLevel=0,reason="QA catalog approval",confirmed=true});
        var brandId=brand.GetProperty("Id").GetGuid();var group=await client.GetFromJsonAsync<JsonElement>("/api/medicines/"+originalMedicine+"/brands");Assert.Equal(2,group.GetArrayLength());
        var location=await post("/api/locations",new{name="QA discharge return location"});var supplier=await post("/api/suppliers",new{name="QA brand supplier"});
        var receipt=await post("/api/receiving",new{medicineId=brandId,locationId=location.GetProperty("Id").GetGuid(),supplierId=supplier.GetProperty("Id").GetGuid(),reference=Guid.NewGuid().ToString(),lot="QA-ALT-LOT",expiration=DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1).ToString("yyyy-MM-dd"),quantity=3,conversionFactor=1,unitCost=0,confirmed=true});
        var encounter="QA-discharge-"+Guid.NewGuid().ToString("N");
        var order=await post("/api/orders",new{reference=Guid.NewGuid().ToString(),encounterReference=encounter,medicineId=originalMedicine,quantity=1,confirmed=true});
        await post("/api/settings",new{name="ManualDispensingApproved",value="true",approvalReference="QA isolated clinical workflow"});
        var dispense=await post("/api/dispensing",new{orderId=order.GetProperty("Id").GetGuid(),balanceId=receipt.GetProperty("BalanceId").GetGuid(),quantity=1,barcode="",verificationMode="ManualSelection",patientVerified=true,confirmed=true,overrideReason="QA pharmacist verification",approveBrandSubstitution=true,substitutionApprovalReference="QA order authorization",substitutionReason="Original brand unavailable"});Assert.True(dispense.GetProperty("BrandSubstituted").GetBoolean());
        var lines=await client.GetFromJsonAsync<JsonElement>("/api/returns/discharge-lines?encounter="+Uri.EscapeDataString(encounter));Assert.Single(lines.EnumerateArray());Assert.Equal("QA alternate",lines[0].GetProperty("brand").GetString());
        using(var request=new HttpRequestMessage(HttpMethod.Post,"/api/returns"){Content=JsonContent.Create(new{dispenseLineId=lines[0].GetProperty("id").GetGuid(),quantity=1,reference=Guid.NewGuid().ToString(),reason="QA missing proof",confirmed=true})}){request.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());using var denied=await client.SendAsync(request);Assert.Equal(System.Net.HttpStatusCode.Conflict,denied.StatusCode);}

        await post("/api/returns",new{dispenseLineId=lines[0].GetProperty("id").GetGuid(),quantity=1,reference=Guid.NewGuid().ToString(),reason="QA unused verified hospital item",confirmed=true,dischargeReturn=true,dischargeReference="QA discharge",packageCondition="Sealed/intact",unusedVerified=true,hospitalSourceVerified=true,hospitalEvidenceReference="QA hospital issue record"});
        var secret=ServerSecrets.Load(profile.DataRoot);await using var db=new NpgsqlConnection(controller.Connection("ddhi_runtime",secret.RuntimePassword));await db.OpenAsync();
        const string snapshot="SELECT (SELECT COUNT(*) FROM \"StockMovements\")::text||'|'||(SELECT SUM(\"Quantity\") FROM \"InventoryBalances\")::text||'|'||(SELECT COUNT(*) FROM \"IntegrationOutbox\")::text";
        var before=await new NpgsqlCommand(snapshot,db).ExecuteScalarAsync();
        var outside=await post("/api/returns/external-intakes",new{reference=Guid.NewGuid().ToString(),encounterReference=encounter,source="Uncertain/mixed",itemDescription="Same named generic/brand with uncertain source",quantity=2,unit="tablet",packageCondition="Unknown",lot="QA-ALT-LOT",expiration=(string?)null,barcode="18904083844874",receiptReference="",custodyDisposition="Held separately pending review",notes="QA separate custody; no stock or billing credit",noHospitalCreditAcknowledged=true,confirmed=true});Assert.Equal("None",outside.GetProperty("StockEffect").GetString());Assert.Equal("None",outside.GetProperty("BillingEffect").GetString());Assert.Equal(before,await new NpgsqlCommand(snapshot,db).ExecuteScalarAsync());
        Assert.False((bool)(await new NpgsqlCommand("SELECT has_table_privilege(current_user,'\"ExternalReturnIntakes\"','UPDATE')",db).ExecuteScalarAsync())!);
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","GenericReturnSmoke.json"),JsonSerializer.Serialize(new{genericMultiBrands=true,approvedSubstitution=true,hospitalReturnLinkedToActualBrand=true,externalNoStockOrBillingCredit=true,runtimeIntakeAppendOnly=true,physicalPatientWorkflowAccepted=false}));
    }
}
