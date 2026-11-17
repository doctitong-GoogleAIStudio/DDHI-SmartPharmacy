using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using System.ServiceProcess;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController {
 const string PgService="DDHIPostgreSQL",ApiService="DDHIPharmacyApi",WorkerService="DDHIPharmacyWorker";
 string ScExe=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"sc.exe");
 public static bool IsAdministrator=>new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
 static void RequireAdministrator(){if(!IsAdministrator)throw new InvalidOperationException("Run Server Control as Administrator to configure or control Windows services.");}
 async Task<bool> ServiceExists(string name)=>await Run(ScExe,["query",name],allowFailure:true)==0;
 void MarkService(string name){using var key=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\"+name,true)??throw new InvalidOperationException("Service registry entry missing.");key.SetValue("DDHI_InstanceRoot",profile.DataRoot,RegistryValueKind.String);}
 bool OwnService(string name){using var key=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\"+name);return string.Equals(key?.GetValue("DDHI_InstanceRoot") as string,profile.DataRoot,StringComparison.OrdinalIgnoreCase);}
 static SecurityIdentifier ServiceSid(string name)=>(SecurityIdentifier)new NTAccount(@"NT SERVICE\"+name).Translate(typeof(SecurityIdentifier));
 static void GrantDirectory(string path,string service,FileSystemRights rights,bool inherit){Directory.CreateDirectory(path);var info=new DirectoryInfo(path);var acl=FileSystemAclExtensions.GetAccessControl(info);acl.AddAccessRule(new FileSystemAccessRule(ServiceSid(service),rights,inherit?InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit:InheritanceFlags.None,PropagationFlags.None,AccessControlType.Allow));FileSystemAclExtensions.SetAccessControl(info,acl);}
 static void GrantFile(string path,string service){var info=new FileInfo(path);var acl=FileSystemAclExtensions.GetAccessControl(info);acl.AddAccessRule(new FileSystemAccessRule(ServiceSid(service),FileSystemRights.Read,AccessControlType.Allow));FileSystemAclExtensions.SetAccessControl(info,acl);}
 public async Task InstallWindowsServices(){
  RequireAdministrator();if(!profile.Prepared)throw new InvalidOperationException("Prepare the database and certificate first.");
  foreach(var service in new[]{PgService,ApiService,WorkerService})if(await ServiceExists(service)&&!OwnService(service))throw new InvalidOperationException($"{service} already exists and belongs to another installation. No changes were made to that service.");
  if(!profile.ServicesConfigured)await Stop();else await StopWindowsServices();
  if(!await ServiceExists(PgService)){await Run(Path.Combine(profile.PostgresBin,"pg_ctl.exe"),["register","-N",PgService,"-D",PgData,"-S","auto","-o",$"-h 127.0.0.1 -p {profile.PostgresPort} -c shared_buffers=64MB -c max_connections=60 -c timezone=Asia/Manila"]);MarkService(PgService);}
  foreach(var item in new[]{(ApiService,ApiExe),(WorkerService,Path.Combine(profile.ServerRoot,"Worker","DDHI.Pharmacy.Worker.exe"))}){
   if(!await ServiceExists(item.Item1)){await Run(ScExe,["create",item.Item1,"binPath=","\""+item.Item2+"\"","start=","auto"]);MarkService(item.Item1);}
  }
  foreach(var service in new[]{PgService,ApiService,WorkerService}){
   await Run(ScExe,["config",service,"obj=",@"NT SERVICE\"+service,"password=",""]);
   await Run(ScExe,["sidtype",service,"unrestricted"]);
   await Run(ScExe,["failure",service,"reset=","86400","actions=","restart/10000/restart/30000/restart/60000"]);
   GrantDirectory(profile.DataRoot,service,FileSystemRights.ReadAndExecute,false);
  }
  GrantDirectory(PgData,PgService,FileSystemRights.Modify,true);GrantDirectory(Logs,PgService,FileSystemRights.Modify,true);
  await Run(ScExe,["config",ApiService,"depend=",PgService]);await Run(ScExe,["config",WorkerService,"depend=",ApiService]);
  var secrets=ServerSecrets.Load(profile.DataRoot);var values=new Dictionary<string,string>{["ConnectionStrings:Pharmacy"]=Connection("ddhi_runtime",secrets.RuntimePassword),["DDHI:CertificatePassword"]=secrets.CertificatePassword,["Kestrel:Endpoints:Https:Certificate:Password"]=secrets.CertificatePassword};
  var plain=JsonSerializer.SerializeToUtf8Bytes(values);var secretPath=Path.Combine(profile.DataRoot,"service-credentials.dpapi");try{await File.WriteAllBytesAsync(secretPath,ProtectedData.Protect(plain,Encoding.UTF8.GetBytes("DDHI.ServiceSecrets.v1"),DataProtectionScope.LocalMachine));}finally{CryptographicOperations.ZeroMemory(plain);}
  foreach(var service in new[]{ApiService,WorkerService}){
   GrantFile(Config,service);GrantFile(secretPath,service);GrantDirectory(Logs,service,FileSystemRights.Modify,true);
   using var key=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\"+service,true)!;
   key.SetValue("Environment",new[]{"DDHI_CONFIG_FILE="+Config,"DDHI_SECRETS_FILE="+secretPath,"ASPNETCORE_ENVIRONMENT=Production","DOTNET_ENVIRONMENT=Production"},RegistryValueKind.MultiString);
  }
  GrantFile(profile.CertificatePath,ApiService);GrantDirectory(Path.Combine(profile.DataRoot,"keys"),ApiService,FileSystemRights.Modify,true);
  profile.ServicesConfigured=true;profile.Save();await Run(ScExe,["start",PgService],allowFailure:true);await WaitForService(PgService,ServiceControllerStatus.Running);await WaitForDatabase();log("PostgreSQL, API and worker Windows services are configured. Create the first administrator, then click Start server.");
 }
 async Task WaitForDatabase(){for(var i=0;i<60;i++){if(await PgReady())return;await Task.Delay(500);}throw new InvalidOperationException("PostgreSQL service did not become ready. Review its log and service account permissions.");}
 async Task WaitForService(string name,ServiceControllerStatus status){await Task.Run(()=>{using var service=new ServiceController(name);service.WaitForStatus(status,TimeSpan.FromSeconds(60));});}
 public async Task StartWindowsServices(){RequireAdministrator();foreach(var service in new[]{PgService,ApiService,WorkerService}){if(!OwnService(service))throw new InvalidOperationException("Service ownership does not match this instance.");await Run(ScExe,["start",service],allowFailure:true);await WaitForService(service,ServiceControllerStatus.Running);if(service==PgService){await WaitForDatabase();await EnsureAccountManagementGrants();}}await WaitForDatabase();log("Start requested for the three Windows services.");}
 public async Task StopWindowsServices(){RequireAdministrator();foreach(var service in new[]{WorkerService,ApiService,PgService}){if(!OwnService(service))throw new InvalidOperationException("Service ownership does not match this instance.");await Run(ScExe,["stop",service],allowFailure:true);await WaitForService(service,ServiceControllerStatus.Stopped);}for(var i=0;i<60;i++){if(!await PgReady())return;await Task.Delay(500);}throw new InvalidOperationException("PostgreSQL did not stop; review active service dependencies before continuing.");}
 public bool LocalCertificateTrusted(){if(profile.Mode!="Local evaluation"||string.IsNullOrEmpty(profile.CertificateThumbprint))return false;using var store=new System.Security.Cryptography.X509Certificates.X509Store(System.Security.Cryptography.X509Certificates.StoreName.Root,System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser);store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly);return store.Certificates.Find(System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,profile.CertificateThumbprint,true).Count>0;}
 public void TrustLocalCertificateForCurrentUser(){if(profile.Mode!="Local evaluation"||!profile.Prepared)throw new InvalidOperationException("Only this generated localhost evaluation certificate can be trusted here.");var secrets=ServerSecrets.Load(profile.DataRoot);using var cert=System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(profile.CertificatePath,secrets.CertificatePassword);if(cert.Thumbprint!=profile.CertificateThumbprint||!cert.MatchesHostname("localhost"))throw new InvalidOperationException("Local certificate identity mismatch.");using var publicCert=System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(cert.RawData);using var store=new System.Security.Cryptography.X509Certificates.X509Store(System.Security.Cryptography.X509Certificates.StoreName.Root,System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser);store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadWrite);store.Add(publicCert);log("This localhost certificate is trusted by the current Windows account only. Hospital/workstation trust has not been installed.");}
}
