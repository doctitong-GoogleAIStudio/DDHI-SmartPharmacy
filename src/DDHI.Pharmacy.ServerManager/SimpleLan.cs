using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DDHI.Pharmacy.Contracts;

namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController
{
    internal bool TestSkipFirewall { get; set; }
    internal bool TestAllowLoopbackLan { get; set; }
    string FirewallMarker => Path.Combine(profile.DataRoot, "easy-lan-firewall.json");

    public ServerConnection ClientConnection()
    {
        if (profile.Mode != "Simple LAN" || string.IsNullOrEmpty(profile.PairingPublicKeySha256))
            throw new InvalidOperationException("Click Enable LAN first.");
        return new ServerConnection(1, Environment.MachineName, profile.Endpoint, profile.PairingPublicKeySha256).Validate();
    }

    async Task SynchronizeSimpleFirewall()
    {
        if (TestSkipFirewall || (profile.Mode != "Simple LAN" && !File.Exists(FirewallMarker))) return;
        RequireAdministrator();
        var script = Path.Combine(profile.ServerRoot, "deployment", "Configure-EasyLanFirewall.ps1");
        if (!File.Exists(script)) throw new FileNotFoundException("Install the complete server update, including its network setup helper.");
        var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(profile.DataRoot).ToUpperInvariant())))[..16];
        var args = new List<string> { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", script,
            "-InstanceId", id, "-ApiExecutable", ApiExe, "-WorkerExecutable", Path.Combine(profile.ServerRoot, "Worker", "DDHI.Pharmacy.Worker.exe"),
            "-ApiPort", profile.ApiPort.ToString(), "-DiscoveryPort", profile.DiscoveryPort.ToString() };
        if (profile.Mode == "Simple LAN") { args.Add("-InterfaceName"); args.Add(profile.SimpleLanInterface); }
        else args.Add("-Disable");
        if (profile.Mode == "Simple LAN") await AtomicWrite(FirewallMarker, JsonSerializer.SerializeToUtf8Bytes(new { InstanceId = id }));
        await Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"), args);
        if (profile.Mode != "Simple LAN" && File.Exists(FirewallMarker)) File.Delete(FirewallMarker);
    }

    public async Task EnableSimpleLan(LanAddress selected)
    {
        CheckConnectionOperation();
        if (!TestSkipFirewall) RequireAdministrator();
        using var changeLock = AcquireConnectionLock();
        if (ConnectionChangeState is "Applying" or "RecoveryRequired") throw new InvalidOperationException("Restore the interrupted connection change first.");
        if (!TestAllowLoopbackLan && !LanAddresses.Available().Any(a => a.Address == selected.Address && a.Name == selected.Name))
            throw new InvalidOperationException("Select this server's active local-network address.");
        var ip = System.Net.IPAddress.Parse(selected.Address);
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) throw new InvalidOperationException("Select an IPv4 local-network address.");
        if (profile.DiscoveryPort is < 1024 or > 65535) throw new InvalidOperationException("The discovery port is invalid.");
        var secrets = ServerSecrets.Load(profile.DataRoot);
        X509Certificate2? previous = null;
        RSA? rsa = null;
        try
        {
            if (profile.Mode == "Simple LAN" && File.Exists(profile.CertificatePath))
            {
                previous = X509CertificateLoader.LoadPkcs12FromFile(profile.CertificatePath, secrets.CertificatePassword, X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
                rsa = previous.GetRSAPrivateKey();
            }
            rsa ??= RSA.Create(3072);
            var request = new CertificateRequest("CN=DDHI Pharmacy local connection", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var san = new SubjectAlternativeNameBuilder(); san.AddIpAddress(ip); san.AddIpAddress(System.Net.IPAddress.Loopback); san.AddDnsName("localhost");
            request.CertificateExtensions.Add(san.Build());
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true));
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(2));
            await EnsureDatabaseStarted();
            var backup = await CreateConnectionBackup();
            await WriteJournal(backup, "Applying");
            try
            {
                await StopApplication();
                var path = Path.Combine(profile.DataRoot, "easy-lan-" + Guid.NewGuid().ToString("N") + ".pfx");
                var encrypted = certificate.Export(X509ContentType.Pfx, secrets.CertificatePassword);
                try { await File.WriteAllBytesAsync(path, encrypted); } finally { CryptographicOperations.ZeroMemory(encrypted); }
                profile.Mode = "Simple LAN"; profile.Hostname = selected.Address; profile.SimpleLanInterface = selected.Name;
                profile.CertificatePath = path; profile.CertificateThumbprint = certificate.Thumbprint;
                profile.PairingPublicKeySha256 = ServerConnection.KeyPin(certificate); profile.ApprovalReference = "";
                var config = JsonNode.Parse(await File.ReadAllTextAsync(Config))!.AsObject();
                var ddhi = config["DDHI"]!.AsObject();
                ddhi["CertificatePath"] = path;
                ddhi["PairingConnection"] = JsonSerializer.Serialize(ClientConnection());
                ddhi["DiscoveryPort"] = profile.DiscoveryPort;
                var https = config["Kestrel"]!["Endpoints"]!["Https"]!.AsObject();
                https["Url"] = $"https://0.0.0.0:{profile.ApiPort}";
                https["Certificate"] = new JsonObject { ["Path"] = path };
                await AtomicWrite(Config, JsonSerializer.SerializeToUtf8Bytes(config, new JsonSerializerOptions { WriteIndented = true }));
                await AtomicWrite(profile.ProfilePath, JsonSerializer.SerializeToUtf8Bytes(profile, new JsonSerializerOptions { WriteIndented = true }));
                GrantConnectionServiceAccess();
                await Start();
                await WaitForHealthyConnection();
                await SynchronizeSimpleFirewall();
                await WriteJournal(backup, "Applied");
                log("LAN enabled. Open the client, click Find server, and confirm this code once: " + ClientConnection().PairingCode);
            }
            catch (Exception activation)
            {
                try
                {
                    await StopApplication(); await RestoreConnectionFiles(backup); await Start(); await WaitForHealthyConnection();
                    await WriteJournal(backup, "RolledBack");
                }
                catch (Exception recovery)
                {
                    await WriteJournal(backup, "RecoveryRequired");
                    throw new InvalidOperationException("Network setup failed and recovery needs attention. Use Restore previous connection. The database was not reinitialized.", new AggregateException(activation, recovery));
                }
                throw new InvalidOperationException("Network setup failed; the previous connection was restored. " + activation.Message, activation);
            }
        }
        finally { rsa?.Dispose(); previous?.Dispose(); }
    }
}
