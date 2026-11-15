using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.ServerManager;
public sealed class ServerProfile {
 public string DataRoot {get;set;}="";public string ServerRoot {get;set;}="";public string PostgresBin {get;set;}="";
 public int PostgresPort {get;set;}=55433;public int ApiPort {get;set;}=8443;
 public string Hostname {get;set;}="localhost";public string Mode {get;set;}="Local evaluation";
 public string CertificatePath {get;set;}="";public string CertificateThumbprint {get;set;}="";public string ApprovalReference {get;set;}="";
 public bool Prepared {get;set;} public bool ServicesConfigured {get;set;}
 public string PairingPublicKeySha256 {get;set;}=""; public string SimpleLanInterface {get;set;}=""; public int DiscoveryPort {get;set;}=8444;
 public string ProfilePath=>Path.Combine(DataRoot,"profile.json");
 public string Endpoint=>$"https://{(Mode=="Simple LAN"?LanAddresses.CurrentAddress(Hostname,SimpleLanInterface):Hostname)}:{ApiPort}";
 public static ServerProfile Defaults(){
  var dir=new DirectoryInfo(AppContext.BaseDirectory);var server=dir.Parent?.FullName??dir.FullName;var project=dir;
  while(project!=null&&!File.Exists(Path.Combine(project.FullName,"AGENTS.md")))project=project.Parent;
  var data=project!=null?Path.Combine(project.FullName,"runtime","server"):Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"DDHI-SmartPharmacy","DesktopServer");
  return new(){DataRoot=data,ServerRoot=server,PostgresBin=Path.Combine(server,"PostgreSQL","bin")};
 }
 public static ServerProfile LoadOrDefault(){var p=Defaults();return File.Exists(p.ProfilePath)?JsonSerializer.Deserialize<ServerProfile>(File.ReadAllText(p.ProfilePath))??p:p;}
 public void Save(){Directory.CreateDirectory(DataRoot);File.WriteAllText(ProfilePath,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}
 public void Validate(){
  DataRoot=Path.GetFullPath(DataRoot);ServerRoot=Path.GetFullPath(ServerRoot);PostgresBin=Path.GetFullPath(PostgresBin);
  if(string.Equals(DataRoot.TrimEnd(Path.DirectorySeparatorChar),Path.GetPathRoot(DataRoot)?.TrimEnd(Path.DirectorySeparatorChar),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The server data folder cannot be a drive root.");
  if(DataRoot.Equals(ServerRoot,StringComparison.OrdinalIgnoreCase)||DataRoot.StartsWith(ServerRoot.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Server data must be outside the distributable server binaries folder.");
  var parent=new DirectoryInfo(ServerRoot);while(parent!=null&&!File.Exists(Path.Combine(parent.FullName,"AGENTS.md")))parent=parent.Parent;
  if(parent!=null){var projectRelative=Path.GetRelativePath(parent.FullName,DataRoot);if(!projectRelative.StartsWith("..")&&!projectRelative.StartsWith("runtime"+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Development server data must be inside the project runtime folder, which is excluded from source/release archives.");}
  if(PostgresPort is <1024 or >65535||ApiPort is <1024 or >65535||PostgresPort==ApiPort)throw new InvalidOperationException("Choose distinct unprivileged TCP ports between 1024 and 65535.");
  if(Mode=="Local evaluation"&&Hostname!="localhost")throw new InvalidOperationException("Local evaluation must use localhost and loopback bindings.");
  if(Mode=="Simple LAN"){if(!System.Net.IPAddress.TryParse(Hostname,out var ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork||PairingPublicKeySha256.Length!=64)throw new InvalidOperationException("The simple LAN identity is incomplete. Enable LAN again.");}
  else if(Mode!="Local evaluation"&&(string.IsNullOrWhiteSpace(ApprovalReference)||Uri.CheckHostName(Hostname)!=UriHostNameType.Dns||Hostname.Equals("localhost",StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Hospital mode needs an IT-approved DNS hostname and approval reference.");
  foreach(var exe in new[]{Path.Combine(PostgresBin,"initdb.exe"),Path.Combine(PostgresBin,"pg_ctl.exe"),Path.Combine(ServerRoot,"Api","DDHI.Pharmacy.Api.exe"),Path.Combine(ServerRoot,"Worker","DDHI.Pharmacy.Worker.exe")})if(!File.Exists(exe))throw new FileNotFoundException("Required server component is missing.",exe);
 }
}
public sealed class ServerSecrets {
 public string OwnerPassword {get;set;}="";public string MigrationPassword {get;set;}="";public string RuntimePassword {get;set;}="";public string CertificatePassword {get;set;}="";
 public static string Random()=>"Aa1!"+Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
 public static ServerSecrets Create()=>new(){OwnerPassword=Random(),MigrationPassword=Random(),RuntimePassword=Random(),CertificatePassword=Random()};
 public void Save(string root){var plain=JsonSerializer.SerializeToUtf8Bytes(this);try{File.WriteAllBytes(Path.Combine(root,"credentials.dpapi"),ProtectedData.Protect(plain,Encoding.UTF8.GetBytes("DDHI.ServerControl.v1"),DataProtectionScope.CurrentUser));}finally{CryptographicOperations.ZeroMemory(plain);}}
 public static ServerSecrets Load(string root){var plain=ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(root,"credentials.dpapi")),Encoding.UTF8.GetBytes("DDHI.ServerControl.v1"),DataProtectionScope.CurrentUser);try{return JsonSerializer.Deserialize<ServerSecrets>(plain)??throw new InvalidDataException("Invalid protected credential store.");}finally{CryptographicOperations.ZeroMemory(plain);}}
 public static void SecureDirectory(string path){Directory.CreateDirectory(path);var security=new DirectorySecurity();security.SetAccessRuleProtection(true,false);foreach(var sid in new[]{WindowsIdentity.GetCurrent().User!,new SecurityIdentifier(WellKnownSidType.LocalSystemSid,null),new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid,null)})security.AddAccessRule(new FileSystemAccessRule(sid,FileSystemRights.FullControl,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(path),security);}
}
