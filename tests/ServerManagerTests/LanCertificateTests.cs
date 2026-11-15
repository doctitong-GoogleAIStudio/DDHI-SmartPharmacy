using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DDHI.Pharmacy.ServerManager;
using Xunit;

internal static class LanTestCertificates
{
    internal const string Host = "qa-pharmacy.example.invalid";
    internal static X509Certificate2 Root()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=DDHI isolated test CA " + Guid.NewGuid().ToString("N"), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddYears(2));
    }
    internal static X509Certificate2 Leaf(X509Certificate2 root, string host = Host, int begins = -1, int ends = 30, bool serverAuth = true)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Isolated LAN test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName(host);
        request.CertificateExtensions.Add(san.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(serverAuth ? "1.3.6.1.5.5.7.3.1" : "1.3.6.1.5.5.7.3.2") }, true));
        using var issued = request.Create(root, DateTimeOffset.UtcNow.AddDays(begins), DateTimeOffset.UtcNow.AddDays(ends), RandomNumberGenerator.GetBytes(16));
        return issued.CopyWithPrivateKey(rsa);
    }
    internal static X509ChainPolicy Policy(X509Certificate2 root)
    {
        var policy = new X509ChainPolicy { TrustMode = X509ChainTrustMode.CustomRootTrust, RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
        policy.CustomTrustStore.Add(root);
        return policy;
    }
}

public class LanCertificateTests
{
    [Theory]
    [InlineData("localhost")][InlineData("pc.localhost")][InlineData("192.168.1.20")]
    [InlineData("https://server.example.org")][InlineData("server.example.org:8443")][InlineData("server")]
    public void RejectsNonLanHostnameBeforeLookingForPfx(string host) =>
        Assert.Throws<InvalidOperationException>(() => new LanConnectionOptions(host, "missing.pfx", "TEST ONLY").NormalizedHostname());

    [Fact] public void RequiresActualApprovalReference() => Assert.Throws<InvalidOperationException>(() =>
        new LanConnectionOptions(LanTestCertificates.Host, "missing.pfx", " ").NormalizedHostname());

    [Fact] public void AcceptsMatchingServerCertificateWithoutChangingWindowsTrust()
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        ServerController.ValidateLanCertificate(leaf, LanTestCertificates.Host, LanTestCertificates.Policy(root));
    }
    [Fact] public void RejectsDifferentSubjectAlternativeName()
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        Assert.Throws<InvalidOperationException>(() => ServerController.ValidateLanCertificate(leaf, "other.example.invalid", LanTestCertificates.Policy(root)));
    }
    [Fact] public void ReloadedPfxCanBeValidatedRepeatedlyWithoutMutatingPolicy()
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        using var reloaded = X509CertificateLoader.LoadPkcs12(leaf.Export(X509ContentType.Pfx, "Test-only!Pfx123"), "Test-only!Pfx123", X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        var policy = LanTestCertificates.Policy(root);
        ServerController.ValidateLanCertificate(reloaded, LanTestCertificates.Host, policy);
        ServerController.ValidateLanCertificate(reloaded, LanTestCertificates.Host, policy);
        using var protectedCopy = X509CertificateLoader.LoadPkcs12(reloaded.Export(X509ContentType.Pfx, "Other-test-password!123"), "Other-test-password!123", X509KeyStorageFlags.EphemeralKeySet);
        Assert.True(protectedCopy.HasPrivateKey);
        Assert.Equal(leaf.Thumbprint, protectedCopy.Thumbprint);
        Assert.Empty(policy.ApplicationPolicy.Cast<Oid>());
    }
    [Theory][InlineData(-3,-2)][InlineData(1,2)]
    public void RejectsExpiredOrNotYetValidCertificate(int begins, int ends)
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root, begins: begins, ends: ends);
        Assert.Throws<InvalidOperationException>(() => ServerController.ValidateLanCertificate(leaf, LanTestCertificates.Host, LanTestCertificates.Policy(root)));
    }
    [Fact] public void RejectsPublicCertificateWithoutPrivateKey()
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        using var publicOnly = X509CertificateLoader.LoadCertificate(leaf.Export(X509ContentType.Cert));
        Assert.Throws<InvalidOperationException>(() => ServerController.ValidateLanCertificate(publicOnly, LanTestCertificates.Host, LanTestCertificates.Policy(root)));
    }
    [Fact] public void RejectsClientAuthenticationOnlyCertificate()
    {
        using var root = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root, serverAuth: false);
        Assert.Throws<InvalidOperationException>(() => ServerController.ValidateLanCertificate(leaf, LanTestCertificates.Host, LanTestCertificates.Policy(root)));
    }
    [Fact] public void RejectsAnUntrustedIssuer()
    {
        using var root = LanTestCertificates.Root(); using var other = LanTestCertificates.Root(); using var leaf = LanTestCertificates.Leaf(root);
        Assert.Throws<InvalidOperationException>(() => ServerController.ValidateLanCertificate(leaf, LanTestCertificates.Host, LanTestCertificates.Policy(other)));
    }
    [Fact] public async Task ProductionDnsCheckRejectsLoopbackMapping() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => ServerController.ValidateLocalDns("localhost"));
}
