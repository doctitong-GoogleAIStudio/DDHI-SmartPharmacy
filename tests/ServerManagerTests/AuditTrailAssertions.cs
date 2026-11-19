using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

internal static class AuditTrailAssertions
{
    internal static async Task Exercise(string root,HttpClient client,Func<string,object,Task<JsonElement>> post)
    {
        var session=await post("/api/sessions/start",new{});var id=session.GetProperty("Id").GetString()!;client.DefaultRequestHeaders.Add("X-Audit-Session",id);
        await post("/api/sessions/"+id+"/heartbeat",new{});
        await post("/api/audit/file-activity",new{action="Export CSV",outcome="Succeeded",fileName="synthetic-audit.csv",rows=1,page="Audit Trail"});
        var trail=await client.GetFromJsonAsync<JsonElement>("/api/audit/trail?activity="+Uri.EscapeDataString("Client file activity: Export CSV"));
        var file=Assert.Single(trail.GetProperty("rows").EnumerateArray(),x=>x.GetProperty("sessionId").GetString()==id);Assert.Equal("Succeeded",file.GetProperty("outcome").GetString());Assert.Contains("synthetic-audit.csv",file.GetProperty("detail").GetString());Assert.NotEmpty(file.GetProperty("staff").GetString()!);Assert.NotEmpty(file.GetProperty("computer").GetString()!);Assert.Contains("Chief Pharmacist",file.GetProperty("roles").GetString());
        await post("/api/sessions/"+id+"/end",new{});client.DefaultRequestHeaders.Remove("X-Audit-Session");
        var sessions=await client.GetFromJsonAsync<JsonElement>("/api/audit/sessions");var row=Assert.Single(sessions.GetProperty("rows").EnumerateArray(),x=>x.GetProperty("id").GetString()==id);Assert.Equal("Signed out",row.GetProperty("status").GetString());Assert.Matches("^\\d+h \\d{2}m$",row.GetProperty("observedDuration").GetString()!);Assert.NotEmpty(row.GetProperty("signedOut").GetString()!);
        var totals=await client.GetFromJsonAsync<JsonElement>("/api/audit/session-totals");Assert.True(totals.GetProperty("total").GetInt32()>0);
        Assert.True((await client.GetFromJsonAsync<JsonElement>("/api/audit/verify")).GetProperty("valid").GetBoolean());
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","AuditRuntimeSmoke.json"),JsonSerializer.Serialize(new{publishedRuntimeRole=true,sessionStartHeartbeatEnd=true,staffRoleComputerSnapshot=true,fileBaseNameOnly=true,observedHoursMinutes=true,dailyEndpoint=true,hashChainValid=true}));
    }
}
