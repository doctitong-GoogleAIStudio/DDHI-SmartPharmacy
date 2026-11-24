using DDHI.Pharmacy.ServerManager;
using Npgsql;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

internal static class ScanAccountAssertions
{
    internal static async Task Exercise(ServerController controller,ServerProfile profile,string root)
    {
        var connection=controller.ClientConnection();
        using var client=new HttpClient(connection.CreateHandler()){BaseAddress=new Uri(connection.Address)};
        async Task<JsonElement> Post(string path,object input){
            if(path=="/api/dispensing"){
                var dispenseRequest=JsonSerializer.SerializeToNode(input)!.AsObject();var data=JsonSerializer.SerializeToElement(input);var orderId=data.GetProperty("orderId").GetGuid();var balanceId=data.GetProperty("balanceId").GetGuid();
                var orders=await client.GetFromJsonAsync<JsonElement>("/api/orders");var order=orders.EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==orderId);var inventory=await client.GetFromJsonAsync<JsonElement>("/api/inventory");var balance=inventory.EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==balanceId);
                await Post("/api/clinical/policy",new DDHI.Pharmacy.Contracts.ClinicalSafetyPolicy(true,true,"QA-only engineering manual-review policy"));
                var screening=await Post("/api/clinical/screen",new DDHI.Pharmacy.Contracts.ClinicalScreenInput([new(balance.GetProperty("medicineId").GetGuid())],order.GetProperty("encounterReference").ToString(),OrderId:orderId,BalanceId:balanceId,IssueQuantity:data.GetProperty("quantity").GetDecimal()));
                var assessment=screening.GetProperty("Id").GetGuid();await Post("/api/clinical/interventions",new DDHI.Pharmacy.Contracts.ClinicalInterventionInput(assessment,"Proceed with documented review","Synthetic runtime workflow review; no clinical validation.","","QA authorization",true,true));dispenseRequest["safetyAssessmentId"]=assessment;input=dispenseRequest;
            }
using var request=new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(input)};request.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());using var reply=await client.SendAsync(request);var text=await reply.Content.ReadAsStringAsync();Assert.True(reply.IsSuccessStatusCode,text);return JsonDocument.Parse(text).RootElement.Clone();}
        var token=await Post("/auth/login",new{email="manager-test@example.invalid",password="Test-only!Password123"});
        client.DefaultRequestHeaders.Authorization=new("Bearer",token.GetProperty("accessToken").GetString());client.DefaultRequestHeaders.Add("X-Workstation","scan-account-qa");
        var email="account-qa-"+Guid.NewGuid().ToString("N")+"@example.invalid";
        var created=await Post("/api/users",new{email,password="Synthetic-only!Password123",role="Read-Only User"});
        var id=created.GetProperty("Id").GetString();
        var row=await client.GetFromJsonAsync<JsonElement>("/api/users/"+id);
        await Post("/api/users/"+id+"/edit",new{email,fullName="Synthetic test staff",phoneNumber="12345",roles=new[]{"Pharmacist"},reason="Isolated published API check",confirmed=true,version=row.GetProperty("version").GetString()});
        row=await client.GetFromJsonAsync<JsonElement>("/api/users/"+id);Assert.Equal("Synthetic test staff",row.GetProperty("fullName").GetString());Assert.Equal("Pharmacist",row.GetProperty("roles")[0].GetString());
        await Post("/api/users/"+id+"/delete",new{confirmationEmail=email,reason="Remove synthetic QA account",confirmed=true,version=row.GetProperty("version").GetString()});
        var users=await client.GetFromJsonAsync<JsonElement>("/api/users");Assert.DoesNotContain(users.EnumerateArray(),u=>u.GetProperty("id").GetString()==id);
        var workstation=await Post("/api/workstations",new{name="scan-account-qa"});
        client.DefaultRequestHeaders.Remove("X-Workstation");client.DefaultRequestHeaders.Add("X-Workstation",workstation.GetProperty("Id").GetString());
        var scan=await Post("/api/scan",new{barcode="18904083844874"});
        Assert.True(scan.TryGetProperty("result",out var result));Assert.True(result.TryGetProperty("Status",out _));
        var secret=ServerSecrets.Load(profile.DataRoot);await using var db=new NpgsqlConnection(controller.Connection("ddhi_runtime",secret.RuntimePassword));await db.OpenAsync();
        foreach(var table in new[]{"AspNetUsers","AspNetUserClaims","AspNetUserRoles","AspNetUserLogins","AspNetUserTokens"})Assert.True((bool)(await new NpgsqlCommand($"SELECT has_table_privilege(current_user,'\"{table}\"','DELETE')",db).ExecuteScalarAsync())!);
        foreach(var table in new[]{"AuditEvents","InventoryBalances"})Assert.False((bool)(await new NpgsqlCommand($"SELECT has_table_privilege(current_user,'\"{table}\"','DELETE')",db).ExecuteScalarAsync())!);
        var chiefEmail="catalog-qa-"+Guid.NewGuid().ToString("N")+"@example.invalid";
        await Post("/api/users",new{email=chiefEmail,password="Synthetic-only!Password123",role="Chief Pharmacist"});
        var chiefToken=await Post("/auth/login",new{email=chiefEmail,password="Synthetic-only!Password123"});
        client.DefaultRequestHeaders.Authorization=new("Bearer",chiefToken.GetProperty("accessToken").GetString());
        var medicine=await Post("/api/medicines",new{genericName="QA trade medicine",brandName="",strength="5 mg",dosageForm="Tablet",route="Oral",manufacturer="QA maker",category="QA",baseUnit="tablet",reorderLevel=0,importedBy="QA importer",distributedBy="QA distributor"});
        await Post("/api/barcodes",new{medicineId=medicine.GetProperty("Id").GetGuid(),barcode="18904083844874",packaging="box",conversionFactor=10,confirmed=true});
        var tradeScan=await Post("/api/scan",new{barcode="18904083844874"});
        Assert.Equal("QA importer",tradeScan.GetProperty("result").GetProperty("Medicine").GetProperty("ImportedBy").GetString());
        Assert.Equal("QA distributor",tradeScan.GetProperty("result").GetProperty("Medicine").GetProperty("DistributedBy").GetString());
        await GenericReturnAssertions.Exercise(controller,profile,root,client,Post,medicine.GetProperty("Id").GetGuid());
        await AuditTrailAssertions.Exercise(root,client,Post);
        await ClinicalReferenceAssertions.Exercise(root,client,Post);
        const string countSql="SELECT (SELECT COUNT(*) FROM \"Medicines\")::text||'|'||(SELECT COUNT(*) FROM \"AspNetUsers\")::text||'|'||(SELECT COUNT(*) FROM \"StockMovements\")::text";
        var before=await new NpgsqlCommand(countSql,db).ExecuteScalarAsync();await db.CloseAsync();
        await controller.ApplyDatabaseUpdates();
        for(var attempt=0;attempt<40;attempt++){try{if((await controller.Health()).Contains("ready"))break;}catch{}await Task.Delay(500);}
        Assert.Contains("ready",await controller.Health());await db.OpenAsync();Assert.Equal(before,await new NpgsqlCommand(countSql,db).ExecuteScalarAsync());
        var backup=Directory.GetFiles(Path.Combine(profile.DataRoot,"migration-backups"),"*.ddhibak").Single();Assert.True(File.Exists(backup+".key.dpapi"));Assert.True(File.Exists(backup+".sha256"));using(var input=File.OpenRead(backup)){var magic=new byte[8];input.ReadExactly(magic);Assert.Equal("DDHIBAK1",System.Text.Encoding.ASCII.GetString(magic));}
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","ClinicalMigrationBackup.json"),JsonSerializer.Serialize(new{encryptedBeforeMigration=true,protectedKey=true,checksumPresent=true,isolatedServer=true}));
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","MedicineTradeSmoke.json"),JsonSerializer.Serialize(new{catalogPersisted=true,scanFields=true,schemaUpdatePreservedRecords=true,runtimeRole=true,physicalWorkstationsTested=false}));
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","ScanAccountSmoke.json"),JsonSerializer.Serialize(new{publishedRuntimeRole=true,profileEdit=true,roleReplacement=true,accountDeletion=true,clinicalDeleteDenied=true,structuredScan=true,physicalScannerAcceptance=false}));
    }
}
