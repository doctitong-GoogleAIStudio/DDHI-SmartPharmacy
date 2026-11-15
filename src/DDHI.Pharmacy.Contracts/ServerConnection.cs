using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DDHI.Pharmacy.Contracts;

// Public pairing information only: never include database credentials or a private key.
public sealed record ServerConnection(int Version, string ServerName, string Address, string PublicKeySha256)
{
    public ServerConnection Validate()
    {
        if (Version != 1 || string.IsNullOrWhiteSpace(ServerName) || ServerName.Length > 80 || ServerName.Any(char.IsControl))
            throw new InvalidDataException("This is not a valid DDHI server connection file.");
        if (!Uri.TryCreate(Address, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidDataException("The connection must contain an HTTPS server base address.");
        if (string.IsNullOrEmpty(PublicKeySha256) || PublicKeySha256.Length != 64 || !PublicKeySha256.All(Uri.IsHexDigit))
            throw new InvalidDataException("The server identity is missing or damaged. Obtain a new connection file from the server.");
        return this with { Address = uri.GetLeftPart(UriPartial.Authority), PublicKeySha256 = PublicKeySha256.ToUpperInvariant() };
    }
    [JsonIgnore] public string PairingCode => string.Join("-", Enumerable.Range(0, 5).Select(i => PublicKeySha256.Substring(i * 4, 4).ToUpperInvariant()));
    [JsonIgnore] public string DisplayName => $"{ServerName} ({new Uri(Address).Host})";
    public bool MatchesAddress(string address)
    {
        return Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
            string.Equals(uri.GetLeftPart(UriPartial.Authority), new Uri(Address).GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase) &&
            uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo) && uri.AbsolutePath == "/" && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment);
    }
    public static string KeyPin(X509Certificate2 certificate) => Convert.ToHexString(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo()));
    public bool AcceptsCertificate(X509Certificate2? certificate)
    {
        if (certificate == null || certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow || certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow) return false;
        var eku = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        if (eku == null || !eku.EnhancedKeyUsages.Cast<Oid>().Any(o => o.Value == "1.3.6.1.5.5.7.3.1")) return false;
        if (certificate.Extensions.OfType<X509BasicConstraintsExtension>().Any(e => e.CertificateAuthority)) return false;
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(PublicKeySha256), Convert.FromHexString(KeyPin(certificate)));
    }
    public HttpClientHandler CreateHandler()
    {
        var trusted = Validate();
        return new HttpClientHandler
        {
            AllowAutoRedirect = false,
            // Pairing authenticates the exact public key, independently of IP/DNS/CA names.
            // TLS still proves possession of its private key. Other keys are always rejected.
            ServerCertificateCustomValidationCallback = (request, certificate, _, _) =>
                request.RequestUri != null && trusted.MatchesAddress(request.RequestUri.GetLeftPart(UriPartial.Authority)) && trusted.AcceptsCertificate(certificate)
        };
    }
    public static ServerConnection Parse(string json)
    {
        if (json.Length > 4096) throw new InvalidDataException("The connection file is too large.");
        return (JsonSerializer.Deserialize<ServerConnection>(json, new JsonSerializerOptions { MaxDepth = 8 })
            ?? throw new InvalidDataException("Empty connection file.")).Validate();
    }
}

public sealed record LanAddress(string Name, string Address, string Mask, bool HasGateway)
{
    public override string ToString() => $"{Name} - {Address}";
}

public static class LanAddresses
{
    public static IReadOnlyList<LanAddress> Available() => NetworkInterface.GetAllNetworkInterfaces()
        .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .SelectMany(n => n.GetIPProperties().UnicastAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a.Address))
            .Select(a => new LanAddress(n.Name, a.Address.ToString(), a.IPv4Mask.ToString(), n.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any)))))
        .OrderByDescending(a => a.HasGateway).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public static string CurrentAddress(string original, string interfaceName) =>
        Available().FirstOrDefault(a => a.Name == interfaceName && a.Address == original)?.Address ??
        Available().FirstOrDefault(a => a.Name == interfaceName)?.Address ?? original;

    public static bool SameSubnet(IPAddress left, IPAddress right, IPAddress mask)
    {
        var a = left.GetAddressBytes(); var b = right.GetAddressBytes(); var m = mask.GetAddressBytes();
        return a.Length == 4 && b.Length == 4 && m.Length == 4 && m.Any(v => v != 0) && Enumerable.Range(0, 4).All(i => (a[i] & m[i]) == (b[i] & m[i]));
    }
}

public static class LanDiscovery
{
    public const int DefaultPort = 8444;
    const string Protocol = "DDHI-PHARMACY-DISCOVERY-1";
    sealed record Query(string Protocol, string Nonce);
    sealed record Reply(string Protocol, string Nonce, ServerConnection Connection);

    public static async Task<IReadOnlyList<ServerConnection>> FindAsync(int port = DefaultPort, TimeSpan? timeout = null,
        IEnumerable<IPAddress>? targets = null, CancellationToken cancellationToken = default)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(timeout ?? TimeSpan.FromSeconds(3));
        using var udp = new UdpClient(AddressFamily.InterNetwork) { EnableBroadcast = true };
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var packet = JsonSerializer.SerializeToUtf8Bytes(new Query(Protocol, nonce));
        var destinations = targets?.ToArray() ?? LanAddresses.Available().Select(a =>
        {
            var ip = IPAddress.Parse(a.Address).GetAddressBytes(); var mask = IPAddress.Parse(a.Mask).GetAddressBytes();
            return new IPAddress(Enumerable.Range(0, 4).Select(i => (byte)(ip[i] | ~mask[i])).ToArray());
        }).Append(IPAddress.Broadcast).Distinct().Take(12).ToArray();
        foreach (var destination in destinations)
        {
            try { await udp.SendAsync(packet, new IPEndPoint(destination, port), budget.Token); }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException) { }
        }
        var found = new Dictionary<string, ServerConnection>(StringComparer.Ordinal);
        while (!budget.IsCancellationRequested && found.Count < 16)
        {
            try
            {
                var result = await udp.ReceiveAsync(budget.Token);
                if (result.Buffer.Length > 4096) continue;
                var reply = JsonSerializer.Deserialize<Reply>(result.Buffer, new JsonSerializerOptions { MaxDepth = 8 });
                if (reply?.Protocol != Protocol || reply.Nonce != nonce || reply.Connection == null) continue;
                var advertised = reply.Connection.Validate();
                var endpoint = new UriBuilder(advertised.Address) { Host = result.RemoteEndPoint.Address.ToString() }.Uri.GetLeftPart(UriPartial.Authority);
                var connection = (advertised with { Address = endpoint }).Validate();
                found[connection.PublicKeySha256 + connection.Address] = connection;
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) when (ex is SocketException or JsonException or InvalidDataException or ArgumentException) { }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return found.Values.ToArray(); // Discovery alone never establishes trust.
    }

    public static async Task ServeAsync(ServerConnection connection, int port, CancellationToken stop)
    {
        var advertised = connection.Validate();
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        var lastReply = new Dictionary<IPAddress, DateTime>();
        while (!stop.IsCancellationRequested)
        {
            try
            {
                var message = await udp.ReceiveAsync(stop);
                if (message.Buffer.Length > 1024) continue;
                var query = JsonSerializer.Deserialize<Query>(message.Buffer, new JsonSerializerOptions { MaxDepth = 4 });
                if (query?.Protocol != Protocol || string.IsNullOrEmpty(query.Nonce) || query.Nonce.Length != 32 || !query.Nonce.All(Uri.IsHexDigit)) continue;
                var peer = message.RemoteEndPoint.Address;
                var local = IPAddress.IsLoopback(peer) ? "127.0.0.1" : LanAddresses.Available()
                    .FirstOrDefault(a => LanAddresses.SameSubnet(peer, IPAddress.Parse(a.Address), IPAddress.Parse(a.Mask)))?.Address;
                if (local == null) continue;
                if (lastReply.TryGetValue(peer, out var last) && DateTime.UtcNow - last < TimeSpan.FromMilliseconds(250)) continue;
                if (lastReply.Count > 1024) lastReply.Clear();
                lastReply[peer] = DateTime.UtcNow;
                var endpoint = new UriBuilder(advertised.Address) { Host = local }.Uri.GetLeftPart(UriPartial.Authority);
                var reply = JsonSerializer.SerializeToUtf8Bytes(new Reply(Protocol, query.Nonce, advertised with { Address = endpoint }));
                await udp.SendAsync(reply, message.RemoteEndPoint, stop);
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { break; }
            catch (Exception ex) when (ex is JsonException or InvalidDataException or ArgumentException or SocketException) { }
        }
    }
}
