using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.ServerManager;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Npgsql;
using Xunit;

internal static class SimpleLanAssertions
{
    internal static async Task Exercise(ServerController controller, ServerProfile profile, string root)
    {
        controller.TestHealthHandlerFactory=null;
        controller.TestSkipFirewall=true; controller.TestAllowLoopbackLan=true;
        using(var socket=new UdpClient(new IPEndPoint(IPAddress.Loopback,0)))profile.DiscoveryPort=((IPEndPoint)socket.Client.LocalEndPoint!).Port;
        var original=await File.ReadAllBytesAsync(Path.Combine(profile.DataRoot,"credentials.dpapi"));
        var secret=ServerSecrets.Load(profile.DataRoot);
        await using var database=new NpgsqlConnection(controller.Connection("ddhi_runtime",secret.RuntimePassword));await database.OpenAsync();
        async Task<string> Snapshot()=>(string)(await new NpgsqlCommand("""
            SELECT (SELECT string_agg("Id"||"Email"||"PasswordHash",';' ORDER BY "Id") FROM "AspNetUsers")
              ||'|'||(SELECT string_agg(row_to_json(t)::text,';' ORDER BY "Id") FROM "InventoryBalances" t)
              ||'|'||(SELECT string_agg(row_to_json(t)::text,';' ORDER BY "Id") FROM "InventoryBatches" t)
            """,database).ExecuteScalarAsync())!;
        var snapshot=await Snapshot();
        var selected=new LanAddress("Isolated QA loopback","127.0.0.1","255.0.0.0",false);
        await controller.EnableSimpleLan(selected);
        var connection=controller.ClientConnection();
        Assert.Equal("Simple LAN",profile.Mode); Assert.Empty(profile.ApprovalReference);
        using(var client=new HttpClient(connection.CreateHandler()))
        {
            var json=await client.GetStringAsync(connection.Address+"/health");Assert.Contains("ready",json);
            using var login=await client.PostAsJsonAsync(connection.Address+"/auth/login",new{email="manager-test@example.invalid",password="Test-only!Password123"});
            Assert.True(login.IsSuccessStatusCode,await login.Content.ReadAsStringAsync());
        }
        using(var impostor=new HttpClient((connection with{PublicKeySha256=new string('A',64)}).CreateHandler()))
            await Assert.ThrowsAsync<HttpRequestException>(()=>impostor.GetAsync(connection.Address+"/health"));
        var found=await LanDiscovery.FindAsync(profile.DiscoveryPort,TimeSpan.FromSeconds(1),[IPAddress.Loopback]);
        Assert.Contains(found,c=>c.PublicKeySha256==connection.PublicKeySha256);
        var file=JsonSerializer.Serialize(connection); Assert.DoesNotContain(secret.RuntimePassword,file); Assert.DoesNotContain(secret.CertificatePassword,file);
        Assert.Equal(connection,ServerConnection.Parse(file));
        Assert.Equal(snapshot,await Snapshot());Assert.Equal(original,await File.ReadAllBytesAsync(Path.Combine(profile.DataRoot,"credentials.dpapi")));
        await controller.EnableSimpleLan(selected);Assert.Equal(connection.PublicKeySha256,controller.ClientConnection().PublicKeySha256);
        await controller.RestorePreviousConnection();Assert.Equal("Simple LAN",profile.Mode);
        await database.CloseAsync();await controller.Stop();await controller.Start();
        for(var i=0;i<30;i++){try{if((await controller.Health()).Contains("ready"))break;}catch{}await Task.Delay(300);}
        await database.OpenAsync();Assert.Contains("ready",await controller.Health());Assert.Equal(snapshot,await Snapshot());
        using(var records=controller.OpenRecordSession()) {
            await records.SignIn("manager-test@example.invalid","Test-only!Password123");
            Assert.True(records.IsAuthenticated);
            var medicines=await records.Read(DatabaseDataset.All.Single(d=>d.Endpoint=="medicines"));
            Assert.Equal(System.Text.Json.JsonValueKind.Array,medicines.ValueKind);
            Assert.Equal(medicines.GetArrayLength(),DatabaseRecordExport.ToView(medicines).Count);
            var users=await records.Read(DatabaseDataset.All.Single(d=>d.Endpoint=="users"));
            Assert.DoesNotContain("PasswordHash",users.GetRawText());
            await Assert.ThrowsAsync<InvalidOperationException>(()=>records.Read(DatabaseDataset.All.Single(d=>d.Endpoint=="receiving")));
            Assert.DoesNotContain(secret.RuntimePassword,controller.DatabaseConnectionDetails);
            records.SignOut(); await Assert.ThrowsAsync<InvalidOperationException>(()=>records.Read(DatabaseDataset.All[0]));
        }
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","DatabaseAccessSmoke.json"),JsonSerializer.Serialize(new {
            version="0.5.0",pinnedHttps=true,staffAuthentication=true,roleDenied=true,readOnlyRecords=true,
            viewConversion=true,credentialsHidden=true,signOutClearsAccess=true,externalToolsLaunched=false
        },new JsonSerializerOptions{WriteIndented=true}));
        await ItDatabaseLoginAssertions.Exercise(controller,profile,root);
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","SimpleConnectionSmoke.json"),JsonSerializer.Serialize(new{
            version="0.4.0",automaticDiscovery=true,pinnedHttps=true,wrongKeyRejected=true,existingAdministratorLogin=true,
            databaseAndStockPreserved=true,credentialsUnchanged=true,publicConnectionFile=true,identitySurvivesReenable=true,
            restoreAndRestart=true,windowsTrustStoresChanged=false,firewallChangesTested=false,physicalWorkstationsTested=false
        },new JsonSerializerOptions{WriteIndented=true}));
    }
}
