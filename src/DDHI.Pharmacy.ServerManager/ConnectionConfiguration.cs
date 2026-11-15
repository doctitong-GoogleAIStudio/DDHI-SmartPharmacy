using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DDHI.Pharmacy.ServerManager;

public sealed record LanConnectionOptions(string Hostname, string PfxPath, string ApprovalReference)
{
    public string NormalizedHostname()
    {
        var hostname = Hostname.Trim().ToLowerInvariant();
        if (Uri.CheckHostName(hostname) != UriHostNameType.Dns ||
            hostname.EndsWith('.') || hostname == "localhost" ||
            hostname.EndsWith(".localhost", StringComparison.Ordinal) || !hostname.Contains('.'))
            throw new InvalidOperationException("Enter the approved full DNS hostname, without https://, a port, an IP address or a path.");
        if (string.IsNullOrWhiteSpace(ApprovalReference))
            throw new InvalidOperationException("Enter the actual hospital IT approval reference.");
        if (!File.Exists(PfxPath))
            throw new FileNotFoundException("Select the matching server PFX file.");
        return hostname;
    }
}

public sealed partial class ServerController
{
    // Test-only seams use an isolated CA and socket route, never a Windows trust-store change.
    internal X509ChainPolicy? TestCertificateChainPolicy { get; set; }
    internal Func<string, Task>? TestDnsValidation { get; set; }
    internal Func<HttpMessageHandler>? TestHealthHandlerFactory { get; set; }
    internal bool TestSkipDatabaseSync { get; set; }

    sealed record ConnectionJournal(string DirectoryName, string State);
    sealed record ConnectionManifest(string ProfileHash, string ConfigHash, DateTimeOffset Created);
    string ConnectionJournalPath => Path.Combine(profile.DataRoot, "connection-change.json");
    string ConnectionBackupsRoot => Path.Combine(profile.DataRoot, "connection-backups");

    public string ConnectionChangeState
    {
        get
        {
            try { return ReadJournal()?.State ?? "None"; }
            catch { return "Recovery information unreadable; review the protected logs."; }
        }
    }
    public bool CanRestoreConnection => ConnectionChangeState is "Applied" or "Applying" or "RecoveryRequired";

    FileStream AcquireConnectionLock()
    {
        try { return new FileStream(Path.Combine(profile.DataRoot, "connection-change.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException) { throw new InvalidOperationException("Another Server Control window is changing this instance's connection. Wait for it to finish."); }
    }

    static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    static async Task AtomicWrite(string destination, byte[] bytes)
    {
        var staged = destination + ".new-" + Guid.NewGuid().ToString("N");
        await File.WriteAllBytesAsync(staged, bytes);
        File.Move(staged, destination, true);
    }
    ConnectionJournal? ReadJournal() => File.Exists(ConnectionJournalPath)
        ? JsonSerializer.Deserialize<ConnectionJournal>(File.ReadAllText(ConnectionJournalPath))
        : null;
    Task WriteJournal(string directory, string state) => AtomicWrite(ConnectionJournalPath,
        JsonSerializer.SerializeToUtf8Bytes(new ConnectionJournal(Path.GetFileName(directory), state)));

    internal static void ValidateLanCertificate(X509Certificate2 certificate, string hostname, X509ChainPolicy? policy = null)
    {
        var now = DateTime.UtcNow;
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("The PFX must contain the server's private key.");
        if (certificate.NotBefore.ToUniversalTime() > now || certificate.NotAfter.ToUniversalTime() <= now)
            throw new InvalidOperationException("The server certificate is expired or not yet valid. Check its dates and the server clock.");
        if (!certificate.MatchesHostname(hostname, allowWildcards: true, allowCommonName: false))
            throw new InvalidOperationException("The certificate Subject Alternative Name does not match the approved hostname.");
        if (certificate.Extensions.OfType<X509BasicConstraintsExtension>().Any(e => e.CertificateAuthority))
            throw new InvalidOperationException("Select a server certificate, not a certificate authority's private key.");
        var eku = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        if (eku == null || !eku.EnhancedKeyUsages.Cast<Oid>().Any(o => o.Value == "1.3.6.1.5.5.7.3.1"))
            throw new InvalidOperationException("The certificate must explicitly permit Server Authentication.");
        using var chain = new X509Chain();
        if (policy != null) chain.ChainPolicy = policy.Clone();
        chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
        chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(5);
        if (!chain.Build(certificate))
            throw new InvalidOperationException("The certificate chain is not trusted or its revocation check failed (" + string.Join(", ", chain.ChainStatus.Select(s => s.Status)) + "). Hospital IT must establish server trust and certificate validity first.");
    }

    internal static async Task ValidateLocalDns(string hostname)
    {
        IPAddress[] resolved;
        try { resolved = await Dns.GetHostAddressesAsync(hostname).WaitAsync(TimeSpan.FromSeconds(10)); }
        catch { throw new InvalidOperationException("The approved hostname cannot be resolved on this server. Have IT configure DNS before applying LAN settings."); }
        var local = NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(n => n.GetIPProperties().UnicastAddresses).Select(a => a.Address).ToHashSet();
        if (resolved.Length == 0 || resolved.Any(a => IPAddress.IsLoopback(a) || !local.Contains(a)))
            throw new InvalidOperationException("Every DNS address for the approved hostname must identify this server's LAN interface. Correct stale, loopback or foreign A/AAAA records first.");
    }

    void CheckConnectionOperation()
    {
        if (!profile.Prepared) throw new InvalidOperationException("Open the existing prepared server instance first.");
        profile.Validate();
        if (profile.ServicesConfigured)
        {
            RequireAdministrator();
            foreach (var name in new[] { PgService, ApiService, WorkerService })
                if (!OwnService(name)) throw new InvalidOperationException("A Windows service belongs to a different instance. No connection settings were changed.");
        }
    }

    async Task EnsureDatabaseStarted()
    {
        if (profile.ServicesConfigured)
        {
            await Run(ScExe, ["start", PgService], allowFailure: true);
            await WaitForService(PgService, System.ServiceProcess.ServiceControllerStatus.Running);
            await WaitForDatabase();
        }
        else await StartPostgres();
        await VerifyDatabase();
    }

    async Task StopApplication()
    {
        // PostgreSQL remains up; this operation never initializes or migrates a database.
        if (profile.ServicesConfigured)
        {
            foreach (var name in new[] { WorkerService, ApiService })
            {
                await Run(ScExe, ["stop", name], allowFailure: true);
                await WaitForService(name, System.ServiceProcess.ServiceControllerStatus.Stopped);
            }
        }
        else
        {
            foreach (var name in new[] { "worker", "api" })
            {
                if (!IsRunning(name)) continue;
                var record = JsonSerializer.Deserialize<ProcessRecord>(File.ReadAllText(ProcessFile(name)))!;
                using var process = Process.GetProcessById(record.Id);
                process.Kill(true);
                await process.WaitForExitAsync();
            }
        }
    }

    async Task WaitForHealthyConnection()
    {
        var timer = Stopwatch.StartNew();
        Exception? error = null;
        while (timer.Elapsed < TimeSpan.FromSeconds(35))
        {
            try
            {
                if ((await Health()).Contains("database ready", StringComparison.Ordinal))
                {
                    await Task.Delay(700);
                    if (ApplicationIsRunning() && (await Health()).Contains("database ready", StringComparison.Ordinal)) return;
                }
            }
            catch (Exception ex) { error = ex; }
            await Task.Delay(400);
        }
        throw new InvalidOperationException("The HTTPS endpoint did not become ready. Check the API log, DNS, certificate and listener port.", error);
    }

    bool ApplicationIsRunning()
    {
        if (!profile.ServicesConfigured) return IsRunning("api") && IsRunning("worker");
        foreach (var name in new[] { ApiService, WorkerService })
        {
            using var service = new System.ServiceProcess.ServiceController(name);
            if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running) return false;
        }
        return true;
    }

    async Task<string> CreateConnectionBackup()
    {
        var directory = Path.Combine(ConnectionBackupsRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
        ServerSecrets.SecureDirectory(directory);
        var oldProfile = JsonSerializer.SerializeToUtf8Bytes(profile);
        var oldConfig = await File.ReadAllBytesAsync(Config);
        await File.WriteAllBytesAsync(Path.Combine(directory, "profile.json"), oldProfile);
        await File.WriteAllBytesAsync(Path.Combine(directory, "server.json"), oldConfig);
        await File.WriteAllBytesAsync(Path.Combine(directory, "manifest.json"), JsonSerializer.SerializeToUtf8Bytes(
            new ConnectionManifest(Hash(oldProfile), Hash(oldConfig), DateTimeOffset.UtcNow)));
        return directory;
    }

    async Task RestoreConnectionFiles(string directory, bool apply = true)
    {
        var canonical = Path.GetFullPath(directory);
        var parent = Path.GetDirectoryName(canonical);
        if (!string.Equals(parent, Path.GetFullPath(ConnectionBackupsRoot), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Connection backup is outside this server instance.");
        var manifest = JsonSerializer.Deserialize<ConnectionManifest>(await File.ReadAllTextAsync(Path.Combine(canonical, "manifest.json")))
            ?? throw new InvalidDataException("Connection backup manifest is missing.");
        var oldProfile = await File.ReadAllBytesAsync(Path.Combine(canonical, "profile.json"));
        var oldConfig = await File.ReadAllBytesAsync(Path.Combine(canonical, "server.json"));
        if (Hash(oldProfile) != manifest.ProfileHash || Hash(oldConfig) != manifest.ConfigHash)
            throw new InvalidDataException("Connection backup checksum mismatch. No backup configuration was applied.");
        var restored = JsonSerializer.Deserialize<ServerProfile>(oldProfile) ?? throw new InvalidDataException("Invalid backup profile.");
        if (!string.Equals(Path.GetFullPath(restored.DataRoot), Path.GetFullPath(profile.DataRoot), StringComparison.OrdinalIgnoreCase) ||
            restored.PostgresPort != profile.PostgresPort || restored.ApiPort != profile.ApiPort ||
            !string.Equals(restored.PostgresBin, profile.PostgresBin, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(restored.ServerRoot, profile.ServerRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The backup does not belong to this database instance and installation.");
        if (!File.Exists(restored.CertificatePath)) throw new FileNotFoundException("The previous server certificate is unavailable.");
        if (!apply) return;
        await AtomicWrite(Config, oldConfig);
        await AtomicWrite(profile.ProfilePath, oldProfile);
        ApplyConnectionProfile(restored);
        GrantConnectionServiceAccess();
    }

    void ApplyConnectionProfile(ServerProfile values)
    {
        profile.Mode = values.Mode;
        profile.Hostname = values.Hostname;
        profile.CertificatePath = values.CertificatePath;
        profile.CertificateThumbprint = values.CertificateThumbprint;
        profile.ApprovalReference = values.ApprovalReference;
    }

    void GrantConnectionServiceAccess()
    {
        if (!profile.ServicesConfigured) return;
        // Atomic replacements inherit the protected directory ACL, not the old file's service grants.
        foreach (var service in new[] { ApiService, WorkerService }) GrantFile(Config, service);
        GrantFile(profile.CertificatePath, ApiService);
    }

    public async Task ConfigureLanConnection(LanConnectionOptions options, string pfxPassword)
    {
        CheckConnectionOperation();
        using var connectionLock = AcquireConnectionLock();
        if (ConnectionChangeState is "Applying" or "RecoveryRequired")
            throw new InvalidOperationException("Restore the interrupted connection change before applying another one.");
        var hostname = options.NormalizedHostname();
        var secrets = ServerSecrets.Load(profile.DataRoot);
        using var certificate = X509CertificateLoader.LoadPkcs12FromFile(options.PfxPath, pfxPassword, X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        ValidateLanCertificate(certificate, hostname, TestCertificateChainPolicy);
        await (TestDnsValidation?.Invoke(hostname) ?? ValidateLocalDns(hostname));
        await EnsureDatabaseStarted();
        var directory = await CreateConnectionBackup();
        await WriteJournal(directory, "Applying");
        try
        {
            await StopApplication();
            var destination = Path.Combine(profile.DataRoot, "lan-server-" + Guid.NewGuid().ToString("N") + ".pfx");
            var encrypted = certificate.Export(X509ContentType.Pfx, secrets.CertificatePassword);
            try { await File.WriteAllBytesAsync(destination, encrypted); }
            finally { CryptographicOperations.ZeroMemory(encrypted); }
            if (profile.ServicesConfigured) GrantFile(destination, ApiService);

            var config = JsonNode.Parse(await File.ReadAllTextAsync(Config))?.AsObject()
                ?? throw new InvalidDataException("The existing public server configuration is invalid.");
            var ddhi = config["DDHI"] as JsonObject ?? new JsonObject();
            if (ddhi.Parent == null) config["DDHI"] = ddhi;
            ddhi["CertificatePath"] = destination;
            var kestrel = config["Kestrel"]?.AsObject() ?? throw new InvalidDataException("The HTTPS configuration is missing.");
            var https = kestrel["Endpoints"]?["Https"]?.AsObject() ?? throw new InvalidDataException("The HTTPS endpoint is missing.");
            https["Url"] = $"https://0.0.0.0:{profile.ApiPort}";
            https["Certificate"] = new JsonObject { ["Path"] = destination };
            await AtomicWrite(Config, JsonSerializer.SerializeToUtf8Bytes(config, new JsonSerializerOptions { WriteIndented = true }));
            profile.Mode = "Hospital LAN";
            profile.Hostname = hostname;
            profile.CertificatePath = destination;
            profile.CertificateThumbprint = certificate.Thumbprint;
            profile.ApprovalReference = options.ApprovalReference.Trim();
            await AtomicWrite(profile.ProfilePath, JsonSerializer.SerializeToUtf8Bytes(profile, new JsonSerializerOptions { WriteIndented = true }));
            GrantConnectionServiceAccess();
            await Start();
            await WaitForHealthyConnection();
            await WriteJournal(directory, "Applied");
            log($"LAN connection is ready: {profile.Endpoint}. Existing database, users and protected credentials were retained. Connection backup: {directory}");
        }
        catch (Exception error)
        {
            try
            {
                await StopApplication();
                await RestoreConnectionFiles(directory);
                await Start();
                await WaitForHealthyConnection();
                await WriteJournal(directory, "RolledBack");
            }
            catch (Exception recovery)
            {
                await WriteJournal(directory, "RecoveryRequired");
                throw new InvalidOperationException("LAN activation failed and automatic recovery could not complete. Use Restore previous connection and review the protected API/setup logs. The database was not reinitialized.", new AggregateException(error, recovery));
            }
            throw new InvalidOperationException("LAN activation failed. The previous connection settings were restored and the server restarted. " + error.Message, error);
        }
    }

    public async Task RestorePreviousConnection()
    {
        CheckConnectionOperation();
        using var connectionLock = AcquireConnectionLock();
        var journal = ReadJournal();
        if (journal == null || !CanRestoreConnection)
            throw new InvalidOperationException("There is no pending or applied connection backup to restore.");
        if (Path.GetFileName(journal.DirectoryName) != journal.DirectoryName)
            throw new InvalidDataException("Invalid connection backup reference.");
        var directory = Path.Combine(ConnectionBackupsRoot, journal.DirectoryName);
        // Reject a corrupt/foreign snapshot before interrupting a healthy server.
        await RestoreConnectionFiles(directory, apply: false);
        try
        {
            await StopApplication();
            await RestoreConnectionFiles(directory);
            await Start();
            await WaitForHealthyConnection();
            await WriteJournal(directory, "Restored");
            log("Previous connection settings restored. Server ready at " + profile.Endpoint);
        }
        catch
        {
            await WriteJournal(directory, "RecoveryRequired");
            throw;
        }
    }

    public async Task<bool> HasApplicationUsers()
    {
        var secrets = ServerSecrets.Load(profile.DataRoot);
        await using var connection = new Npgsql.NpgsqlConnection(Connection("ddhi_runtime", secrets.RuntimePassword));
        await connection.OpenAsync();
        return (bool)(await new Npgsql.NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM \"AspNetUsers\")", connection).ExecuteScalarAsync())!;
    }
}
