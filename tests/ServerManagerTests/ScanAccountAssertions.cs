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
        async Task<JsonElement> Post(string path,object input){using var request=new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(input)};request.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());using var reply=await client.SendAsync(request);var text=await reply.Content.ReadAsStringAsync();Assert.True(reply.IsSuccessStatusCode,text);return JsonDocument.Parse(text).RootElement.Clone();}
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
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","ScanAccountSmoke.json"),JsonSerializer.Serialize(new{publishedRuntimeRole=true,profileEdit=true,roleReplacement=true,accountDeletion=true,clinicalDeleteDenied=true,structuredScan=true,physicalScannerAcceptance=false}));
    }
}
