using System.ServiceProcess;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController
{
    public async Task ApplyDatabaseUpdates()
    {
        if(!profile.Prepared)throw new InvalidOperationException("Open the existing prepared server instance first.");
        if(profile.ServicesConfigured)RequireAdministrator();
        // Stop only this instance's owned services/processes before changing its schema.
        await Stop();
        if(profile.ServicesConfigured){await Run(ScExe,["start",PgService],allowFailure:true);await WaitForService(PgService,ServiceControllerStatus.Running);await WaitForDatabase();}
        else await StartPostgres();
        var secrets=ServerSecrets.Load(profile.DataRoot);
        await Run(ApiExe,["--migrate"],EnvironmentFor(Connection("ddhi_migration",secrets.MigrationPassword),secrets));
        await Start();
        log("Database updates applied; existing records retained and server restarted.");
    }
}
