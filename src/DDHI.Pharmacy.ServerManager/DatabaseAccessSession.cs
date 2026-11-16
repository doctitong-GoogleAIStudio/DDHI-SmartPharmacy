using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DDHI.Pharmacy.ServerManager;

public sealed record DatabaseDataset(string Name, string Endpoint)
{
    public static IReadOnlyList<DatabaseDataset> All { get; } = new[] {
        new DatabaseDataset("Medicines", "medicines"), new("Inventory", "inventory"), new("Barcodes", "barcodes"),
        new("Suppliers", "suppliers"), new("Locations", "locations"), new("Receiving", "receiving"),
        new("Transfers", "transfers"), new("Stock movements", "movements"), new("Recalls", "recalls"),
        new("Orders (clinical role)", "orders"), new("Workstations", "workstations"),
        new("Staff accounts (System Administrator)", "users"), new("Audit events (report role)", "audit")
    };
}

// Uses application authentication and existing server policies; never reads raw database tables.
public sealed class DatabaseAccessSession : IDisposable
{
    readonly HttpClient client;
    DateTimeOffset expires;
    public bool IsAuthenticated => client.DefaultRequestHeaders.Authorization != null && DateTimeOffset.UtcNow < expires;
    public string Identity { get; private set; } = "";
    public DatabaseAccessSession(string address, HttpMessageHandler handler)
    {
        var uri = new Uri(address.TrimEnd('/') + "/");
        if (uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath != "/" || uri.Query != "" || uri.Fragment != "")
            throw new InvalidOperationException("Database record access requires the configured HTTPS base address.");
        client = new HttpClient(handler) { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(30), MaxResponseContentBufferSize = 32 * 1024 * 1024 };
        client.DefaultRequestHeaders.Add("X-Workstation", "ServerControl-" + Environment.MachineName);
    }
    public async Task SignIn(string email, string password)
    {
        SignOut();
        using var response = await client.PostAsJsonAsync("auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Staff sign-in failed or the account is locked. Check your application email and password.");
        using var token = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var value = token.RootElement.GetProperty("accessToken").GetString();
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("The server did not return a staff session.");
        var duration = token.RootElement.TryGetProperty("expiresIn", out var seconds) ? seconds.GetInt32() : 1200;
        expires = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(duration, 1, 1200));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
        try {
            using var me = await client.GetAsync("api/me"); await Check(me);
            using var identity = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
            Identity = email + " — " + string.Join(", ", identity.RootElement.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        } catch { SignOut(); throw; }
    }
    async Task Check(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized) { SignOut(); throw new InvalidOperationException("Your application session expired. Sign in again."); }
        if (response.StatusCode == HttpStatusCode.Forbidden) throw new InvalidOperationException("Your application role does not allow this dataset. Select another dataset or use the appropriate staff account.");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"The server could not read records (HTTP {(int)response.StatusCode}). Check server health.");
    }
    public async Task<JsonElement> Read(DatabaseDataset dataset)
    {
        if (!IsAuthenticated) { SignOut(); throw new InvalidOperationException("Sign in with your pharmacy application account first."); }
        if (!DatabaseDataset.All.Contains(dataset)) throw new InvalidOperationException("Select a supported dataset.");
        using var response = await client.GetAsync("api/" + dataset.Endpoint); await Check(response);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (json.RootElement.ValueKind != JsonValueKind.Array) throw new InvalidDataException("The server returned an unexpected dataset.");
        return json.RootElement.Clone();
    }
    public void SignOut() { client.DefaultRequestHeaders.Authorization = null; Identity = ""; expires = default; }
    public void Dispose() { SignOut(); client.Dispose(); }
}

public static class DatabaseRecordExport
{
    static string Display(JsonElement value) => value.ValueKind is JsonValueKind.Null ? "" : value.ToString();
    public static DataView ToView(JsonElement rows)
    {
        var table = new DataTable();
        foreach (var row in rows.EnumerateArray()) foreach (var field in row.EnumerateObject())
            if (!table.Columns.Contains(field.Name)) table.Columns.Add(field.Name);
        foreach (var row in rows.EnumerateArray()) {
            var data = table.NewRow(); foreach (var field in row.EnumerateObject()) data[field.Name] = Display(field.Value); table.Rows.Add(data);
        }
        return table.DefaultView;
    }
    static string Cell(string value, bool protectFormula)
    {
        var start = value.TrimStart();
        if (protectFormula && ((start.Length > 0 && "=+-@".Contains(start[0])) || value.StartsWith('\t') || value.StartsWith('\r') || value.StartsWith('\n'))) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
    public static string Csv(JsonElement rows)
    {
        var fields = rows.EnumerateArray().SelectMany(r => r.EnumerateObject().Select(p => p.Name)).Distinct().ToArray();
        if (fields.Length == 0) return "";
        var result = new StringBuilder().AppendLine(string.Join(",", fields.Select(f => Cell(f, true))));
        foreach (var row in rows.EnumerateArray()) result.AppendLine(string.Join(",", fields.Select(f => row.TryGetProperty(f, out var v) ? Cell(Display(v), v.ValueKind == JsonValueKind.String) : "\"\"")));
        return result.ToString();
    }
}
