using System.Net.Http;
using System.Net.Security;
namespace DDHI.Pharmacy.ServerManager;

public sealed partial class ServerController
{
    public DatabaseAccessSession OpenRecordSession()
    {
        if (!profile.Prepared) throw new InvalidOperationException("Open the prepared server instance first.");
        HttpMessageHandler handler;
        if (profile.Mode == "Simple LAN") handler = ClientConnection().CreateHandler();
        else {
            var standard = new HttpClientHandler { AllowAutoRedirect = false };
            standard.ServerCertificateCustomValidationCallback = (request, certificate, _, errors) =>
                certificate != null && certificate.Thumbprint == profile.CertificateThumbprint &&
                (profile.Mode == "Local evaluation" ? request.RequestUri?.IsLoopback == true : errors == SslPolicyErrors.None);
            handler = standard;
        }
        return new DatabaseAccessSession(profile.Endpoint, handler);
    }
    public string DatabaseConnectionDetails =>
        $"Database: ddhi_pharmacy\nPostgreSQL host: 127.0.0.1 (this server only)\nPostgreSQL port: {profile.PostgresPort}\n" +
        $"Runtime account: ddhi_runtime\nMigration account: ddhi_migration\nData folder: {profile.DataRoot}\n" +
        $"Pharmacy API: {profile.Endpoint}\nPasswords and private keys are hidden. Clients connect to HTTPS, never directly to PostgreSQL.";
}
