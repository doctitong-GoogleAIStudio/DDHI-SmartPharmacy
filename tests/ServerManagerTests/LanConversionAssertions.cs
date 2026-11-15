using DDHI.Pharmacy.ServerManager;
using Npgsql;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Xunit;

internal static class LanConversionAssertions
{
    sealed class UnhealthyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
    }
    internal static async Task Exercise(ServerController controller, ServerProfile profile, string projectRoot)
    {
        Assert.True(await controller.HasApplicationUsers());
        var configPath = Path.Combine(profile.DataRoot, "server.json");
        var originalConfig = await File.ReadAllBytesAsync(configPath);
        var credentialsPath = Path.Combine(profile.DataRoot, "credentials.dpapi");
        var originalCredentials = await File.ReadAllBytesAsync(credentialsPath);
        var secrets = ServerSecrets.Load(profile.DataRoot);
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        var pfx = Path.Combine(profile.DataRoot, "qa-input-only.pfx");
        await File.WriteAllBytesAsync(pfx, leaf.Export(X509ContentType.Pfx, "Test-only!Pfx123"));
        var options = new LanConnectionOptions(LanTestCertificates.Host, pfx, "ISOLATED TEST ONLY");
        controller.TestCertificateChainPolicy = LanTestCertificates.Policy(root);
        bool dnsChecked = false;
        controller.TestDnsValidation = host => { Assert.Equal(LanTestCertificates.Host, host); dnsChecked = true; return Task.CompletedTask; };
        bool forceUnhealthy = false;
        controller.TestHealthHandlerFactory = () =>
        {
            if (profile.Mode == "Local evaluation")
                return new HttpClientHandler { ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => request.RequestUri!.IsLoopback && cert?.Thumbprint == profile.CertificateThumbprint };
            if (forceUnhealthy) return new UnhealthyHandler();
            return new SocketsHttpHandler
            {
                UseProxy = false,
                ConnectCallback = async (context, token) =>
                {
                    Assert.Equal(LanTestCertificates.Host, context.DnsEndPoint.Host);
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try { await socket.ConnectAsync(IPAddress.Loopback, context.DnsEndPoint.Port, token); return new NetworkStream(socket, true); }
                    catch { socket.Dispose(); throw; }
                },
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (_, cert, _, _) =>
                    {
                        if (cert == null) return false;
                        using var actual = X509CertificateLoader.LoadCertificate(cert.GetRawCertData());
                        using var chain = new X509Chain { ChainPolicy = LanTestCertificates.Policy(root) };
                        return actual.Thumbprint == leaf.Thumbprint && actual.MatchesHostname(LanTestCertificates.Host, true, false) && chain.Build(actual);
                    }
                }
            };
        };

        await using var connection = new NpgsqlConnection(controller.Connection("ddhi_runtime", secrets.RuntimePassword));
        await connection.OpenAsync();
        var medicineId = Guid.NewGuid();
        await using (var seed = new NpgsqlCommand("""
            INSERT INTO "Medicines" ("Id","GenericName","BrandName","Strength","DosageForm","Route","Manufacturer","Category","BaseUnit","PackagingUnit","ConversionFactor","ReorderLevel","ActiveStatus","UpdatedAt","CreatedAt")
            VALUES (@id,'LAN preservation test','','TEST','TEST','','','TEST ONLY','unit','unit',1,7,true,now(),now())
            """, connection)) { seed.Parameters.AddWithValue("id", medicineId); await seed.ExecuteNonQueryAsync(); }
        await using (var stock = new NpgsqlCommand("""
            INSERT INTO "InventoryLocations" ("Id","Name","CreatedAt") VALUES (@location,'Isolated LAN test location',now());
            INSERT INTO "InventoryBatches" ("Id","MedicineId","Lot","Expiration","UnitCost","CreatedAt") VALUES (@batch,@medicine,'TEST-ONLY',CURRENT_DATE+365,3,now());
            INSERT INTO "InventoryBalances" ("Id","BatchId","LocationId","Status","Quantity","CreatedAt") VALUES (@balance,@batch,@location,0,23,now());
            """, connection))
        {
            stock.Parameters.AddWithValue("location", Guid.NewGuid()); stock.Parameters.AddWithValue("batch", Guid.NewGuid());
            stock.Parameters.AddWithValue("medicine", medicineId); stock.Parameters.AddWithValue("balance", Guid.NewGuid());
            await stock.ExecuteNonQueryAsync();
        }
        async Task<string> Fingerprint() => (string)(await new NpgsqlCommand("""
            SELECT (SELECT string_agg("Id" || "Email" || "PasswordHash", ';' ORDER BY "Id") FROM "AspNetUsers")
            || '|' || (SELECT string_agg(row_to_json(t)::text, ';' ORDER BY "Id") FROM "Medicines" t)
            || '|' || (SELECT string_agg(row_to_json(t)::text, ';' ORDER BY "Id") FROM "InventoryBatches" t)
            || '|' || (SELECT string_agg(row_to_json(t)::text, ';' ORDER BY "Id") FROM "InventoryBalances" t)
            """, connection).ExecuteScalarAsync())!;
        var fingerprint = await Fingerprint();

        // Invalid inputs and concurrent changes must not interrupt the old connection.
        await Assert.ThrowsAnyAsync<System.Security.Cryptography.CryptographicException>(() => controller.ConfigureLanConnection(options, "Wrong test password"));
        Assert.Equal(originalConfig, await File.ReadAllBytesAsync(configPath));
        using (var heldLock = new FileStream(Path.Combine(profile.DataRoot, "connection-change.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ConfigureLanConnection(options, "Test-only!Pfx123"));
        controller.TestDnsValidation = _ => throw new InvalidOperationException("Simulated DNS validation failure.");
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ConfigureLanConnection(options, "Test-only!Pfx123"));
        Assert.Equal(originalConfig, await File.ReadAllBytesAsync(configPath));
        Assert.Contains("database ready", await controller.Health());
        controller.TestDnsValidation = host => { dnsChecked = true; return Task.CompletedTask; };

        await controller.ConfigureLanConnection(options, "Test-only!Pfx123");
        Assert.True(dnsChecked);
        Assert.Equal("Hospital LAN", profile.Mode);
        Assert.Equal("Applied", controller.ConnectionChangeState);
        Assert.Contains($"https://0.0.0.0:{profile.ApiPort}", await File.ReadAllTextAsync(configPath));
        Assert.Contains("database ready", await controller.Health());
        using (var client = new HttpClient(controller.TestHealthHandlerFactory()) { BaseAddress = new Uri(profile.Endpoint) })
        {
            using var login = await client.PostAsJsonAsync("/auth/login", new { email = "manager-test@example.invalid", password = "Test-only!Password123" });
            Assert.True(login.IsSuccessStatusCode, await login.Content.ReadAsStringAsync());
        }
        Assert.Equal(fingerprint, await Fingerprint());
        Assert.Equal(originalCredentials, await File.ReadAllBytesAsync(credentialsPath));
        Assert.DoesNotContain(secrets.CertificatePassword, await File.ReadAllTextAsync(configPath));

        // A corrupt restore snapshot is rejected before stopping the working LAN API.
        var journalPath = Path.Combine(profile.DataRoot, "connection-change.json");
        var journal = JsonDocument.Parse(await File.ReadAllTextAsync(journalPath));
        var backup = Path.Combine(profile.DataRoot, "connection-backups", journal.RootElement.GetProperty("DirectoryName").GetString()!);
        var backupConfig = Path.Combine(backup, "server.json");
        var saved = await File.ReadAllBytesAsync(backupConfig);
        await File.AppendAllTextAsync(backupConfig, "CORRUPTED TEST SNAPSHOT");
        await Assert.ThrowsAsync<InvalidDataException>(controller.RestorePreviousConnection);
        Assert.Contains("database ready", await controller.Health());
        await File.WriteAllBytesAsync(backupConfig, saved);

        await controller.RestorePreviousConnection();
        Assert.Equal("Restored", controller.ConnectionChangeState);
        Assert.Equal("Local evaluation", profile.Mode);
        Assert.Equal(originalConfig, await File.ReadAllBytesAsync(configPath));
        Assert.Equal(fingerprint, await Fingerprint());

        // Activate a real LAN listener but fail its health verification; restore localhost.
        forceUnhealthy = true;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ConfigureLanConnection(options, "Test-only!Pfx123"));
        Assert.Contains("previous connection settings were restored", error.Message);
        Assert.Equal("RolledBack", controller.ConnectionChangeState);
        Assert.Equal("Local evaluation", profile.Mode);
        Assert.Equal(originalConfig, await File.ReadAllBytesAsync(configPath));
        Assert.Equal(originalCredentials, await File.ReadAllBytesAsync(credentialsPath));
        Assert.Equal(fingerprint, await Fingerprint());
        Assert.Contains("database ready", await controller.Health());

        // Simulate reopening the manager after an interrupted change; recovery is persisted.
        var journalText = await File.ReadAllTextAsync(journalPath);
        await File.WriteAllTextAsync(journalPath, journalText.Replace("RolledBack", "Applying"));
        var reopened = new ServerController(profile, _ => { });
        Assert.True(reopened.CanRestoreConnection);
        await reopened.RestorePreviousConnection();
        Assert.Equal("Restored", reopened.ConnectionChangeState);
        Assert.Equal(fingerprint, await Fingerprint());
        Assert.Equal(originalCredentials, await File.ReadAllBytesAsync(credentialsPath));
        Assert.Contains("database ready", await reopened.Health());
        await File.WriteAllTextAsync(Path.Combine(projectRoot, "release", "Tests", "LanUpgradeSmoke.json"), JsonSerializer.Serialize(new
        {
            version = "0.3.0", managedProcessConversion = true, tlsHostnameVerified = true,
            existingAdministratorLogin = true, medicineAndUsersPreserved = true, stockAndLotsPreserved = true, credentialsUnchanged = true,
            dnsFailureRejectedBeforeChange = true, wrongPfxPasswordRejected = true, concurrentChangeRejected = true,
            corruptBackupRejected = true, manualRestore = true, failedActivationRolledBack = true,
            interruptedChangeRecovery = true, windowsTrustStoresModified = false, elevatedServicesTested = false,
            physicalWorkstationAcceptance = false, note = "Isolated test database and CA; no hospital DNS, firewall or trust modifications."
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
