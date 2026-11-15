using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DDHI.Pharmacy.Contracts;

namespace DDHI.Pharmacy.Desktop;
public sealed class ConnectionSettingsStore(string path)
{
    static readonly byte[] Entropy = Encoding.UTF8.GetBytes("DDHI.ServerPairing.v1");
    public static ConnectionSettingsStore Default => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DDHI", "paired-server.dpapi"));
    public ServerConnection? Load()
    {
        if (!File.Exists(path)) return null;
        byte[]? plain = null;
        try
        {
            plain = ProtectedData.Unprotect(File.ReadAllBytes(path), Entropy, DataProtectionScope.CurrentUser);
            return ServerConnection.Parse(Encoding.UTF8.GetString(plain));
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or InvalidDataException)
        { throw new InvalidOperationException("The saved server connection cannot be read. Pair again using the server's connection file.", ex); }
        finally { if (plain != null) CryptographicOperations.ZeroMemory(plain); }
    }
    public void Save(ServerConnection connection)
    {
        var validated = connection.Validate();
        var plain = JsonSerializer.SerializeToUtf8Bytes(validated);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            var encrypted = ProtectedData.Protect(plain, Entropy, DataProtectionScope.CurrentUser);
            var staged = path + ".new";
            File.WriteAllBytes(staged, encrypted);
            File.Move(staged, path, true);
        }
        finally { CryptographicOperations.ZeroMemory(plain); }
    }
}
