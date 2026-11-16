using DDHI.Pharmacy.ServerManager;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

public class DatabaseAccessTests
{
    sealed class Replies(params (HttpStatusCode Status, string Body)[] replies) : HttpMessageHandler
    {
        int index;
        public List<string> Requests { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken stop)
        {
            Requests.Add(request.Method + " " + request.RequestUri!.AbsolutePath);
            var next = replies[index++];
            return Task.FromResult(new HttpResponseMessage(next.Status) { Content = new StringContent(next.Body, Encoding.UTF8, "application/json") });
        }
    }
    const string Token = "{\"accessToken\":\"synthetic-test-token\",\"expiresIn\":1200}";
    const string Identity = "{\"roles\":[\"Read-Only User\"]}";
    [Theory][InlineData("http://localhost:8443")][InlineData("https://user:secret@localhost:8443")][InlineData("https://localhost:8443/other")]
    public void RejectsUntrustedBaseAddressShapes(string address) => Assert.Throws<InvalidOperationException>(() => new DatabaseAccessSession(address, new Replies()));
    [Fact] public async Task CannotReadWithoutApplicationSignIn()
    {
        var wire = new Replies(); using var session = new DatabaseAccessSession("https://localhost:8443", wire);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.Read(DatabaseDataset.All[0])); Assert.Empty(wire.Requests);
    }
    [Fact] public async Task ApplicationRolesRemainEnforcedAndSignOutClearsAccess()
    {
        var wire = new Replies((HttpStatusCode.OK,Token),(HttpStatusCode.OK,Identity),(HttpStatusCode.Forbidden,""),(HttpStatusCode.OK,"[{\"GenericName\":\"QA medicine\"}]"));
        using var session = new DatabaseAccessSession("https://localhost:8443",wire);
        await session.SignIn("qa@example.invalid","Synthetic-only!123");
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.Read(DatabaseDataset.All.Single(d=>d.Endpoint=="users")));
        Assert.True(session.IsAuthenticated);
        var rows = await session.Read(DatabaseDataset.All[0]); Assert.Single(rows.EnumerateArray());
        Assert.Equal("QA medicine",DatabaseRecordExport.ToView(rows)[0]["GenericName"]);
        session.SignOut(); Assert.False(session.IsAuthenticated); Assert.Empty(session.Identity);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>session.Read(DatabaseDataset.All[0]));
        Assert.Equal(new[]{"POST /auth/login","GET /api/me","GET /api/users","GET /api/medicines"},wire.Requests);
    }
    [Fact] public async Task ExpiredServerTicketAndFailedLoginCannotLeaveAnAuthenticatedSession()
    {
        var wire = new Replies((HttpStatusCode.OK,Token),(HttpStatusCode.OK,Identity),(HttpStatusCode.Unauthorized,""),(HttpStatusCode.Unauthorized,""));
        using var session = new DatabaseAccessSession("https://localhost:8443",wire);
        await session.SignIn("qa@example.invalid","Synthetic-only!123");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>session.Read(DatabaseDataset.All[0])); Assert.False(session.IsAuthenticated);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>session.SignIn("qa@example.invalid","Incorrect-test-password")); Assert.False(session.IsAuthenticated);
    }
    [Fact] public async Task CannotInjectAnArbitraryEndpoint()
    {
        var wire = new Replies((HttpStatusCode.OK,Token),(HttpStatusCode.OK,Identity));
        using var session = new DatabaseAccessSession("https://localhost:8443",wire); await session.SignIn("qa@example.invalid","Synthetic-only!123");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>session.Read(new("Injected","../auth/login"))); Assert.Equal(2,wire.Requests.Count);
    }
    [Theory][InlineData("=1+1")][InlineData("  +SUM(1,2)")][InlineData("-formula")][InlineData("@SUM(1,2)")][InlineData("\tcmd")]
    public void CsvProtectsSpreadsheetFormulaStrings(string value)
    {
        using var data = JsonDocument.Parse(JsonSerializer.Serialize(new[]{new{Value=value}}));
        Assert.Contains("\"'"+value.Replace("\"","\"\""),DatabaseRecordExport.Csv(data.RootElement));
    }
    [Fact] public void CsvPreservesNumbersQuotesUnicodeAndNewlines()
    {
        using var data=JsonDocument.Parse(JsonSerializer.Serialize(new[]{new{Name="Médicine, \"QA\"\nsecond line",Balance=-2,Empty=(string?)null}}));
        var csv=DatabaseRecordExport.Csv(data.RootElement);
        Assert.Contains("Médicine, \"\"QA\"\"\nsecond line",csv); Assert.Contains("\"-2\"",csv); Assert.DoesNotContain("'-2",csv);
        Assert.Empty(DatabaseRecordExport.Csv(JsonDocument.Parse("[]").RootElement));
    }
}
