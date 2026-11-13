
using DDHI.Pharmacy.ServerManager;
using System.IO;
using Xunit;
public class ServerProfileTests {
 static string Root {get{var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir!=null&&!File.Exists(Path.Combine(dir.FullName,"AGENTS.md")))dir=dir.Parent;return dir?.FullName??throw new InvalidOperationException("Project root not found.");}}
 static ServerProfile Valid()=>new(){DataRoot=Path.Combine(Root,"runtime","profile-test"),ServerRoot=Path.Combine(Root,"release","Server"),PostgresBin=Path.Combine(Root,"release","Server","PostgreSQL","bin")};
 [Fact] public void AcceptsIndependentLoopbackProfile(){Valid().Validate();}
 [Fact] public void CannotApplyDirectoryAclToDriveRoot(){var p=Valid();p.DataRoot=Path.GetPathRoot(Root)!;Assert.Throws<InvalidOperationException>(p.Validate);}
 [Fact] public void RejectsDataInsideDistributedPayload(){var p=Valid();p.DataRoot=Path.Combine(p.ServerRoot,"Data");Assert.Throws<InvalidOperationException>(p.Validate);}
 [Fact] public void RejectsClinicalDataInsideSourceArchiveTree(){var p=Valid();p.DataRoot=Path.Combine(Root,"database","live-data");Assert.Throws<InvalidOperationException>(p.Validate);}
 [Fact] public void LocalEvaluationCannotBindAnUnapprovedHostname(){var p=Valid();p.Hostname="pharmacy.example.invalid";Assert.Throws<InvalidOperationException>(p.Validate);}
 [Fact] public void LanModeNeedsApprovalReference(){var p=Valid();p.Mode="Hospital LAN";p.Hostname="pharmacy.example.invalid";Assert.Throws<InvalidOperationException>(p.Validate);}
 [Fact] public void DatabaseAndApiCannotSharePort(){var p=Valid();p.ApiPort=p.PostgresPort;Assert.Throws<InvalidOperationException>(p.Validate);}
 [Theory][InlineData(0)][InlineData(65536)]public void RejectsInvalidPorts(int port){var p=Valid();p.ApiPort=port;Assert.Throws<InvalidOperationException>(p.Validate);}

 [Fact]public async Task LocalSetupCreatesSeparateAccountsAndSurvivesRestart(){
  var p=Valid();p.DataRoot=Path.Combine(Root,"runtime","manager-qa-"+Guid.NewGuid().ToString("N"));
  int FreePort(){var listener=new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback,0);listener.Start();var port=((System.Net.IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();return port;}
  p.PostgresPort=FreePort();do{p.ApiPort=FreePort();}while(p.ApiPort==p.PostgresPort);
  var controller=new ServerController(p,_=>{});
  async Task Ready(){Exception? error=null;for(int i=0;i<40;i++){try{if((await controller.Health()).Contains("ready"))return;}catch(Exception ex){error=ex;}await Task.Delay(500);}throw new InvalidOperationException("Test server did not become ready.",error);}
  try{
   await controller.Prepare();await controller.Start();await Ready();Assert.Contains("ddhi_runtime",await controller.VerifyDatabase());
   var secrets=ServerSecrets.Load(p.DataRoot);var publicConfig=File.ReadAllText(Path.Combine(p.DataRoot,"server.json"));Assert.DoesNotContain(secrets.RuntimePassword,publicConfig);Assert.DoesNotContain(secrets.CertificatePassword,publicConfig);
   await using(var con=new Npgsql.NpgsqlConnection(controller.Connection("ddhi_runtime",secrets.RuntimePassword))){await con.OpenAsync();Assert.Equal("ddhi_runtime",await new Npgsql.NpgsqlCommand("SELECT current_user",con).ExecuteScalarAsync());Assert.False((bool)(await new Npgsql.NpgsqlCommand("SELECT rolsuper FROM pg_roles WHERE rolname=current_user",con).ExecuteScalarAsync())!);Assert.False((bool)(await new Npgsql.NpgsqlCommand("SELECT has_table_privilege(current_user,'\"AuditEvents\"','UPDATE')",con).ExecuteScalarAsync())!);}
   await controller.Bootstrap("manager-test@example.invalid","Test-only!Password123");await controller.Stop();await controller.Start();await Ready();
   await using(var con=new Npgsql.NpgsqlConnection(controller.Connection("ddhi_runtime",secrets.RuntimePassword))){await con.OpenAsync();Assert.Equal(1L,await new Npgsql.NpgsqlCommand("SELECT count(*) FROM \"AspNetUsers\"",con).ExecuteScalarAsync());}
   using var cert=System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(p.CertificatePath,secrets.CertificatePassword);Assert.True(cert.HasPrivateKey);Assert.True(cert.MatchesHostname("localhost"));
  }finally{await controller.Stop();}
 }
}
