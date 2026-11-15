using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Desktop;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Xunit;

public class SimpleConnectionTests
{
    static string Root
    {
        get { var d = new DirectoryInfo(AppContext.BaseDirectory); while (d != null && !File.Exists(Path.Combine(d.FullName, "AGENTS.md"))) d = d.Parent; return d!.FullName; }
    }
    static X509Certificate2 Certificate(RSA key, int before = -1, int after = 30)
    {
        var r = new CertificateRequest("CN=Isolated app pairing test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        r.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        r.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true));
        return r.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(before), DateTimeOffset.UtcNow.AddDays(after));
    }
    [Fact] public void PairingTrustsOnlyTheConfirmedKeyAndSurvivesCertificateRenewal()
    {
        using var key = RSA.Create(2048); using var wrongKey = RSA.Create(2048);
        using var original = Certificate(key); using var renewed = Certificate(key, after: 60); using var impostor = Certificate(wrongKey);
        var connection = new ServerConnection(1, "QA server", "https://192.168.10.20:8443", ServerConnection.KeyPin(original)).Validate();
        Assert.True(connection.AcceptsCertificate(original)); Assert.True(connection.AcceptsCertificate(renewed)); Assert.False(connection.AcceptsCertificate(impostor));
        Assert.Equal(24, connection.PairingCode.Length);
        Assert.True(connection.MatchesAddress("https://192.168.10.20:8443/"));
        Assert.False(connection.MatchesAddress("https://192.168.10.21:8443"));
        Assert.False(connection.MatchesAddress("http://192.168.10.20:8443"));
    }
    [Theory][InlineData(-3,-2)][InlineData(1,2)] public void CorrectKeyDoesNotOverrideInvalidDates(int before, int after)
    {
        using var key = RSA.Create(2048); using var cert = Certificate(key, before, after);
        Assert.False(new ServerConnection(1, "QA", "https://127.0.0.1:8443", ServerConnection.KeyPin(cert)).AcceptsCertificate(cert));
    }
    [Theory][InlineData("http://192.168.1.20:8443")][InlineData("https://user:pass@192.168.1.20:8443")]
    [InlineData("https://192.168.1.20:8443/auth/login")][InlineData("https://192.168.1.20:8443/?redirect=bad")]
    public void RejectsDangerousConnectionAddresses(string address) => Assert.Throws<InvalidDataException>(() =>
        new ServerConnection(1, "QA", address, new string('A',64)).Validate());
    [Fact] public void RejectsMissingIdentityAndUnknownFileVersion()
    {
        Assert.Throws<InvalidDataException>(() => ServerConnection.Parse("{\"Version\":1,\"ServerName\":\"QA\",\"Address\":\"https://127.0.0.1\",\"PublicKeySha256\":null}"));
        Assert.Throws<InvalidDataException>(() => new ServerConnection(2, "QA", "https://127.0.0.1", new string('A',64)).Validate());
    }
    [Fact] public void StoredPairingIsProtectedAndTamperingFailsClosed()
    {
        var directory = Path.Combine(Root,"runtime","pairing-test-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var path=Path.Combine(directory,"paired.dpapi"); var store=new ConnectionSettingsStore(path);
        var connection=new ServerConnection(1,"QA private test","https://127.0.0.1:8443",new string('A',64));
        store.Save(connection); Assert.Equal(connection,store.Load());
        Assert.DoesNotContain("QA private test",System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(path)));
        var corrupted=File.ReadAllBytes(path); corrupted[^1]^=1; File.WriteAllBytes(path,corrupted);
        Assert.Throws<InvalidOperationException>(store.Load);
    }
    [Fact] public async Task DiscoveryFindsServerButDoesNotAutomaticallyTrustIt()
    {
        using var reservation=new UdpClient(new IPEndPoint(IPAddress.Loopback,0)); var port=((IPEndPoint)reservation.Client.LocalEndPoint!).Port; reservation.Close();
        var connection=new ServerConnection(1,"Isolated discovery server","https://127.0.0.1:58443",new string('A',64));
        using var stop=new CancellationTokenSource(); var responder=LanDiscovery.ServeAsync(connection,port,stop.Token);
        try
        {
            var found=await LanDiscovery.FindAsync(port,TimeSpan.FromMilliseconds(700),[IPAddress.Loopback]);
            Assert.Single(found); Assert.Equal(connection.PublicKeySha256,found[0].PublicKeySha256);
            Assert.Equal("127.0.0.1",new Uri(found[0].Address).Host);
        }
        finally {stop.Cancel();await responder;}
    }
}
