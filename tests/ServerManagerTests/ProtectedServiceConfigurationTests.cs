
using DDHI.Pharmacy.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;
[CollectionDefinition("ProtectedServiceSettings",DisableParallelization=true)]public class ProtectedServiceSettingsCollection;
[Collection("ProtectedServiceSettings")]
public class ProtectedServiceConfigurationTests {
 [Fact]public void LoadsMachineProtectedSettingsWithoutPlaintextConfig(){
  var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir!=null&&!File.Exists(Path.Combine(dir.FullName,"AGENTS.md")))dir=dir.Parent;Assert.NotNull(dir);var root=Path.Combine(dir!.FullName,"runtime","service-secret-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);var file=Path.Combine(root,"credentials.dpapi");
  var expected="Host=localhost;Database=ddhi_pharmacy;Username=ddhi_runtime;Password=TEST-ONLY-VALUE";var values=new Dictionary<string,string>{{"ConnectionStrings:Pharmacy",expected}};var plain=JsonSerializer.SerializeToUtf8Bytes(values);var protectedBytes=ProtectedData.Protect(plain,Encoding.UTF8.GetBytes("DDHI.ServiceSecrets.v1"),DataProtectionScope.LocalMachine);File.WriteAllBytes(file,protectedBytes);Assert.DoesNotContain("TEST-ONLY-VALUE",Encoding.UTF8.GetString(protectedBytes));
  var old=Environment.GetEnvironmentVariable("DDHI_SECRETS_FILE");try{Environment.SetEnvironmentVariable("DDHI_SECRETS_FILE",file);var config=new ConfigurationBuilder();ProtectedServiceConfiguration.Apply(config);Assert.Equal(expected,config.Build().GetConnectionString("Pharmacy"));}finally{Environment.SetEnvironmentVariable("DDHI_SECRETS_FILE",old);CryptographicOperations.ZeroMemory(plain);}
 }
}
