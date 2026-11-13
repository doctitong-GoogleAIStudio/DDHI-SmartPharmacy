using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace DDHI.Pharmacy.Infrastructure;
public static class ProtectedServiceConfiguration {
 public static void Apply(IConfigurationBuilder configuration) {
  var path=Environment.GetEnvironmentVariable("DDHI_SECRETS_FILE");
  if(string.IsNullOrWhiteSpace(path))return;
  if(!OperatingSystem.IsWindows())throw new InvalidOperationException("Windows protected service credentials cannot be used on this operating system.");
  var plain=ProtectedData.Unprotect(File.ReadAllBytes(path),Encoding.UTF8.GetBytes("DDHI.ServiceSecrets.v1"),DataProtectionScope.LocalMachine);
  try {
   var values=JsonSerializer.Deserialize<Dictionary<string,string?>>(plain)??throw new InvalidDataException("Invalid protected service configuration.");
   configuration.AddInMemoryCollection(values);
  } finally { CryptographicOperations.ZeroMemory(plain); }
 }
}
