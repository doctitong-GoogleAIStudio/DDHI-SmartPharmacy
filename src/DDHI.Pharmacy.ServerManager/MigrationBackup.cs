using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController
{
    async Task BackupBeforeMigration(ServerSecrets secrets)
    {
        var folder=Path.Combine(profile.DataRoot,"migration-backups");ServerSecrets.SecureDirectory(folder);
        var file=Path.Combine(folder,"before-schema-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")+".ddhibak");
        var password=ServerSecrets.Random();var bytes=Encoding.UTF8.GetBytes(password);
        try{await File.WriteAllBytesAsync(file+".key.dpapi",ProtectedData.Protect(bytes,Encoding.UTF8.GetBytes("DDHI.MigrationBackup.v1"),DataProtectionScope.CurrentUser));}finally{CryptographicOperations.ZeroMemory(bytes);}
        var env=new Dictionary<string,string>{{"PGHOST","127.0.0.1"},{"PGPORT",profile.PostgresPort.ToString()},{"PGDATABASE","ddhi_pharmacy"},{"PGUSER","ddhi_migration"},{"PGPASSWORD",secrets.MigrationPassword},{"DDHI_BACKUP_PASSWORD",password}};
        await Run(Path.Combine(profile.ServerRoot,"Tools","DDHI.Pharmacy.Tools.exe"),["backup",Path.Combine(profile.PostgresBin,"pg_dump.exe"),file],env);
        log("Authenticated encrypted pre-migration backup saved. Retain this instance's protected migration-backups folder and original Windows setup account for authorized recovery.");
    }
}
