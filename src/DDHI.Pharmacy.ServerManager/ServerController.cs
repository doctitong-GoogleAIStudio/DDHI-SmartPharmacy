using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Npgsql;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController(ServerProfile profile,Action<string> log) {
 public ServerProfile Profile=>profile;
 string PgData=>Path.Combine(profile.DataRoot,"postgres-data");
 string Logs=>Path.Combine(profile.DataRoot,"logs");
 string Config=>Path.Combine(profile.DataRoot,"server.json");
 public string Connection(string user,string password,string database="ddhi_pharmacy")=>new NpgsqlConnectionStringBuilder{Host="127.0.0.1",Port=profile.PostgresPort,Database=database,Username=user,Password=password,Timeout=10,Pooling=false,IncludeErrorDetail=false}.ConnectionString;
 public async Task<int> Run(string exe,IEnumerable<string> args,Dictionary<string,string>? env=null,bool allowFailure=false){
  var arguments=args.ToArray();var redirect=!(Path.GetFileName(exe).Equals("pg_ctl.exe",StringComparison.OrdinalIgnoreCase)&&arguments.Contains("start"));
  var info=new ProcessStartInfo(exe){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=redirect,RedirectStandardError=redirect};foreach(var arg in arguments)info.ArgumentList.Add(arg);if(env!=null)foreach(var kv in env)info.Environment[kv.Key]=kv.Value;
  using var process=Process.Start(info)??throw new InvalidOperationException("Could not start component.");var stdout=redirect?process.StandardOutput.ReadToEndAsync():Task.FromResult("");var stderr=redirect?process.StandardError.ReadToEndAsync():Task.FromResult("");await process.WaitForExitAsync();var text=(await stdout)+"\n"+(await stderr);
  Directory.CreateDirectory(Logs);await File.AppendAllTextAsync(Path.Combine(Logs,"setup.log"),$"{DateTimeOffset.UtcNow:O} {Path.GetFileName(exe)} exit={process.ExitCode}\n{text}\n");
  if(process.ExitCode!=0&&!allowFailure)throw new InvalidOperationException($"{Path.GetFileName(exe)} failed (code {process.ExitCode}). Review the protected setup log.");return process.ExitCode;
 }
 async Task<bool> PgReady()=>await Run(Path.Combine(profile.PostgresBin,"pg_isready.exe"),["-h","127.0.0.1","-p",profile.PostgresPort.ToString(),"-q"],allowFailure:true)==0;
 public async Task StartPostgres(){
  if(await PgReady()){var secrets=ServerSecrets.Load(profile.DataRoot);await using var con=new NpgsqlConnection(Connection("ddhi_owner",secrets.OwnerPassword,"postgres"));await con.OpenAsync();var actual=Convert.ToString(await new NpgsqlCommand("SHOW data_directory",con).ExecuteScalarAsync());if(!string.Equals(Path.GetFullPath(actual!),Path.GetFullPath(PgData),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The selected port belongs to a different PostgreSQL instance. Choose another port.");return;}
  if(!File.Exists(Path.Combine(PgData,"PG_VERSION")))throw new InvalidOperationException("Set up this server instance first.");
  await Run(Path.Combine(profile.PostgresBin,"pg_ctl.exe"),["-D",PgData,"-l",Path.Combine(Logs,"postgresql.log"),"-o",$"-h 127.0.0.1 -p {profile.PostgresPort} -c shared_buffers=64MB -c max_connections=60 -c timezone=Asia/Manila","-w","-t","60","start"]);log("PostgreSQL is running on loopback only.");
 }
 public async Task Prepare(string suppliedPfxPassword=""){
  profile.Validate();if(profile.Prepared)throw new InvalidOperationException("This instance is already prepared. Existing database contents will not be overwritten.");
  if(File.Exists(Path.Combine(PgData,"PG_VERSION"))&&!File.Exists(Path.Combine(profile.DataRoot,"credentials.dpapi")))throw new InvalidOperationException("Existing PostgreSQL data without this manager credential store detected. Initialization refused.");
  if(Directory.Exists(profile.DataRoot)&&!File.Exists(Path.Combine(profile.DataRoot,"credentials.dpapi"))&&Directory.EnumerateFileSystemEntries(profile.DataRoot).Any(p=>!Path.GetFileName(p).StartsWith("local-setup-request.json",StringComparison.Ordinal)))throw new InvalidOperationException("This nonempty directory is not an initialized Server Control instance. Choose a new dedicated runtime folder.");
  ServerSecrets.SecureDirectory(profile.DataRoot);Directory.CreateDirectory(Logs);
  var secretFile=Path.Combine(profile.DataRoot,"credentials.dpapi");var secrets=File.Exists(secretFile)?ServerSecrets.Load(profile.DataRoot):ServerSecrets.Create();secrets.Save(profile.DataRoot);
  if(!File.Exists(Path.Combine(PgData,"PG_VERSION"))){
   if(Directory.Exists(PgData)&&Directory.EnumerateFileSystemEntries(PgData).Any())throw new InvalidOperationException("Nonempty database directory detected; initialization refused.");
   log("Initializing an independent PostgreSQL cluster with SCRAM authentication and data checksums…");
   var passwordFile=Path.Combine(profile.DataRoot,"init-password.tmp");await File.WriteAllTextAsync(passwordFile,secrets.OwnerPassword,new UTF8Encoding(false));
   try{await Run(Path.Combine(profile.PostgresBin,"initdb.exe"),["-D",PgData,"-U","ddhi_owner","--pwfile",passwordFile,"--auth-local=scram-sha-256","--auth-host=scram-sha-256","--encoding=UTF8","--locale=C","--data-checksums"]);}finally{if(File.Exists(passwordFile))File.Delete(passwordFile);}
  }
  await StartPostgres();
  await using(var owner=new NpgsqlConnection(Connection("ddhi_owner",secrets.OwnerPassword,"postgres"))){
   await owner.OpenAsync();
   foreach(var item in new[]{("ddhi_migration",secrets.MigrationPassword),("ddhi_runtime",secrets.RuntimePassword)}){
    await using var exists=new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_roles WHERE rolname=@name)",owner);exists.Parameters.AddWithValue("name",item.Item1);
    if(!(bool)(await exists.ExecuteScalarAsync())!)await new NpgsqlCommand($"CREATE ROLE {item.Item1} LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT PASSWORD '{item.Item2}'",owner).ExecuteNonQueryAsync();
   }
   if(!(bool)(await new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname='ddhi_pharmacy')",owner).ExecuteScalarAsync())!)await new NpgsqlCommand("CREATE DATABASE ddhi_pharmacy OWNER ddhi_migration",owner).ExecuteNonQueryAsync();
  }
  log("Created separate ddhi_migration and ddhi_runtime database accounts.");
  if(profile.Mode=="Local evaluation"){
   profile.CertificatePath=Path.Combine(profile.DataRoot,"localhost-server.pfx");
   if(!File.Exists(profile.CertificatePath)){
    using var rsa=RSA.Create(3072);var request=new CertificateRequest("CN=DDHI localhost evaluation",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);var san=new SubjectAlternativeNameBuilder();san.AddDnsName("localhost");san.AddIpAddress(IPAddress.Loopback);request.CertificateExtensions.Add(san.Build());request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false,false,0,true));request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature|X509KeyUsageFlags.KeyEncipherment,true));var oids=new OidCollection{new Oid("1.3.6.1.5.5.7.3.1")};request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oids,false));using var certificate=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),DateTimeOffset.UtcNow.AddYears(1));await File.WriteAllBytesAsync(profile.CertificatePath,certificate.Export(X509ContentType.Pfx,secrets.CertificatePassword));await File.WriteAllBytesAsync(Path.Combine(profile.DataRoot,"localhost-server.cer"),certificate.Export(X509ContentType.Cert));
   }
   log("Created an encrypted localhost PFX. No Windows trust-store changes were made.");
  } else {
   if(!File.Exists(profile.CertificatePath))throw new FileNotFoundException("Select the hospital-issued PFX.");using var source=X509CertificateLoader.LoadPkcs12FromFile(profile.CertificatePath,suppliedPfxPassword,X509KeyStorageFlags.EphemeralKeySet);
   if(!source.HasPrivateKey||!source.MatchesHostname(profile.Hostname)||source.NotAfter.ToUniversalTime()<=DateTime.UtcNow)throw new InvalidOperationException("PFX must contain a private key, be valid, and match the approved hostname.");using var chain=new X509Chain();if(!chain.Build(source))throw new InvalidOperationException("The hospital certificate chain is not trusted on this computer. Hospital IT must establish trust before LAN configuration.");
   var destination=Path.Combine(profile.DataRoot,"hospital-server.pfx");await File.WriteAllBytesAsync(destination,source.Export(X509ContentType.Pfx,secrets.CertificatePassword));profile.CertificatePath=destination;
  }
  using(var certificate=X509CertificateLoader.LoadPkcs12FromFile(profile.CertificatePath,secrets.CertificatePassword))profile.CertificateThumbprint=certificate.Thumbprint;
  var bind=profile.Mode=="Local evaluation"?"127.0.0.1":"0.0.0.0";
  var config=new{DDHI=new{KeyDirectory=Path.Combine(profile.DataRoot,"keys"),LogDirectory=Logs,CertificatePath=profile.CertificatePath},Kestrel=new{Endpoints=new{Https=new{Url=$"https://{bind}:{profile.ApiPort}",Certificate=new{Path=profile.CertificatePath}}}}};
  await File.WriteAllTextAsync(Config,JsonSerializer.Serialize(config,new JsonSerializerOptions{WriteIndented=true}));profile.Save();
  log("Applying central database migrations…");await Run(ApiExe,["--migrate"],EnvironmentFor(Connection("ddhi_migration",secrets.MigrationPassword),secrets));
  await using(var owner=new NpgsqlConnection(Connection("ddhi_migration",secrets.MigrationPassword))){await owner.OpenAsync();await new NpgsqlCommand("GRANT CONNECT ON DATABASE ddhi_pharmacy TO ddhi_runtime; GRANT USAGE ON SCHEMA public TO ddhi_runtime; GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA public TO ddhi_runtime; GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO ddhi_runtime; REVOKE UPDATE, DELETE, TRUNCATE ON \"StockMovements\", \"AuditEvents\", \"OperationKeys\" FROM ddhi_runtime;",owner).ExecuteNonQueryAsync();}
  profile.Prepared=true;profile.Save();log("Database schema and least-privilege runtime grants are ready.");
 }
 string ApiExe=>Path.Combine(profile.ServerRoot,"Api","DDHI.Pharmacy.Api.exe");
 Dictionary<string,string> EnvironmentFor(string connection,ServerSecrets secrets)=>new(){["DDHI_CONFIG_FILE"]=Config,["ConnectionStrings__Pharmacy"]=connection,["DDHI__CertificatePassword"]=secrets.CertificatePassword,["Kestrel__Endpoints__Https__Certificate__Password"]=secrets.CertificatePassword,["DDHI_SECRETS_FILE"]="",["ASPNETCORE_ENVIRONMENT"]="Production",["DOTNET_ENVIRONMENT"]="Production"};
 sealed record ProcessRecord(int Id,long StartTicks,string Executable);
 string ProcessFile(string name)=>Path.Combine(profile.DataRoot,name+"-process.json");
 bool IsRunning(string name){try{var record=JsonSerializer.Deserialize<ProcessRecord>(File.ReadAllText(ProcessFile(name)))!;using var process=Process.GetProcessById(record.Id);return !process.HasExited&&process.StartTime.ToUniversalTime().Ticks==record.StartTicks&&string.Equals(process.MainModule?.FileName,record.Executable,StringComparison.OrdinalIgnoreCase);}catch{return false;}}
 void Launch(string name,string exe,Dictionary<string,string> env){
  if(IsRunning(name))return;var info=new ProcessStartInfo(exe){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(exe)!,RedirectStandardOutput=true,RedirectStandardError=true};foreach(var kv in env)info.Environment[kv.Key]=kv.Value;
  info.RedirectStandardOutput=false;info.RedirectStandardError=false;var process=Process.Start(info)??throw new InvalidOperationException("Could not start server component.");File.WriteAllText(ProcessFile(name),JsonSerializer.Serialize(new ProcessRecord(process.Id,process.StartTime.ToUniversalTime().Ticks,exe)));log(name+" started.");
 }
 public async Task Start(){if(profile.ServicesConfigured){await StartWindowsServices();return;}if(!profile.Prepared)throw new InvalidOperationException("Complete initial setup first.");await StartPostgres();var secrets=ServerSecrets.Load(profile.DataRoot);var env=EnvironmentFor(Connection("ddhi_runtime",secrets.RuntimePassword),secrets);Launch("api",ApiExe,env);Launch("worker",Path.Combine(profile.ServerRoot,"Worker","DDHI.Pharmacy.Worker.exe"),env);}
 public async Task<string> Health(){if(!profile.Prepared)return "Not configured";using var handler=new HttpClientHandler();if(profile.Mode=="Local evaluation")handler.ServerCertificateCustomValidationCallback=(request,certificate,chain,errors)=>request.RequestUri?.IsLoopback==true&&certificate!=null&&certificate.Thumbprint==profile.CertificateThumbprint;using var client=new HttpClient(handler){Timeout=TimeSpan.FromSeconds(5)};var response=await client.GetAsync(profile.Endpoint+"/health");response.EnsureSuccessStatusCode();var json=JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());return json.GetProperty("status").GetString()=="ready"?"Running • database ready":"Database unavailable";}
 public async Task Bootstrap(string email,string password){if(!profile.Prepared)throw new InvalidOperationException("Set up the database first.");var secrets=ServerSecrets.Load(profile.DataRoot);var env=EnvironmentFor(Connection("ddhi_runtime",secrets.RuntimePassword),secrets);env["DDHI_BOOTSTRAP_EMAIL"]=email;env["DDHI_BOOTSTRAP_PASSWORD"]=password;await Run(ApiExe,["--bootstrap"],env);log("First application administrator created. No password was written to the configuration.");}
 public async Task Stop(){if(profile.ServicesConfigured){await StopWindowsServices();return;}foreach(var name in new[]{"worker","api"}){if(!IsRunning(name))continue;var record=JsonSerializer.Deserialize<ProcessRecord>(File.ReadAllText(ProcessFile(name)))!;using var p=Process.GetProcessById(record.Id);p.Kill(true);await p.WaitForExitAsync();log(name+" stopped.");}if(File.Exists(Path.Combine(PgData,"PG_VERSION")))await Run(Path.Combine(profile.PostgresBin,"pg_ctl.exe"),["-D",PgData,"-m","fast","-w","-t","60","stop"],allowFailure:true);}
 public async Task<string> VerifyDatabase(){var secrets=ServerSecrets.Load(profile.DataRoot);await using var con=new NpgsqlConnection(Connection("ddhi_runtime",secrets.RuntimePassword));await con.OpenAsync();var count=Convert.ToInt32(await new NpgsqlCommand("SELECT count(*) FROM \"Medicines\"",con).ExecuteScalarAsync());return $"Connected to ddhi_pharmacy as ddhi_runtime • {count} medicines";}
}
