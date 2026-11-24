using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public partial class WorkflowTests
{
    async Task<Guid> AuditStation(HttpClient client)
    {
        var station=await Post(client,"workstations",new WorkstationInput("Audit-PC-"+Guid.NewGuid()));var id=station.GetProperty("Id").GetGuid();client.DefaultRequestHeaders.Remove("X-Workstation");client.DefaultRequestHeaders.Add("X-Workstation",id.ToString());return id;
    }
    [Fact] public async Task AuditSnapshotsSurviveProfileChangesAndDeletionAndNeverContainCredentials()
    {
        var account=await Account(Roles.Chief);using var user=account.Client;await AuditStation(user);var marker="Audit-supplier-"+Guid.NewGuid();
        await Post(user,"suppliers",new NamedInput(marker));var profile=await Profile(account.Id);
        await Post(admin,"users/"+account.Id+"/edit",Edit(profile,[Roles.Reader],Guid.NewGuid()+"@test.invalid"));
        var updated=await Profile(account.Id);await Post(admin,"users/"+account.Id+"/delete",new UserDeleteInput(updated.GetProperty("email").GetString()!,"QA audit deletion",true,updated.GetProperty("version").GetString()!));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var rows=await db.AuditEvents.Where(x=>x.Actor==account.Id).ToListAsync();
        var activity=Assert.Single(rows,x=>x.Changes.Contains(marker));Assert.Equal(account.Email,activity.StaffEmail);Assert.Contains(Roles.Chief,activity.StaffRoles);Assert.StartsWith("Audit-PC-",activity.Computer);Assert.Equal(2,activity.FormatVersion);
        var deletion=await db.AuditEvents.SingleAsync(x=>x.Action=="Delete /api/users/"+account.Id+"/delete"&&x.Outcome=="Succeeded");Assert.Contains("Deleted",deletion.Changes);
        foreach(var row in await db.AuditEvents.Where(x=>x.CreatedAt>=activity.CreatedAt).ToListAsync())foreach(var sensitive in new[]{"Test-Only!Password123","PasswordHash","SecurityStamp","ConcurrencyStamp","accessToken"})Assert.DoesNotContain(sensitive,row.Detail+row.Changes);
        Assert.True((await admin.GetFromJsonAsync<JsonElement>("/api/audit/verify")).GetProperty("valid").GetBoolean());
    }
    [Fact] public async Task AuditRecordsDeniedAndFailedChangesWithoutClaimingCommit()
    {
        var stock=await Stock();var path="medicines/"+stock.Medicine+"/trade";
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,path,new MedicineTradeInput("Blocked","","QA",true))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,new MedicineTradeInput("Blocked","","QA",false))).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var attempts=await db.AuditEvents.Where(e=>e.Action=="Attempt /api/"+path).ToListAsync();Assert.Contains(attempts,e=>e.Outcome=="Denied");Assert.Contains(attempts,e=>e.Outcome=="Failed");Assert.Equal("",(await db.Medicines.SingleAsync(m=>m.Id==stock.Medicine)).ImportedBy);
    }
    [Fact] public async Task AuditAccessFiltersDatesAndPagingMatchServerRecords()
    {
        await AuditStation(chief);var marker="audit-filter-"+Guid.NewGuid();await Post(chief,"suppliers",new NamedInput(marker));
        Assert.Equal(HttpStatusCode.Forbidden,(await reader.GetAsync("/api/audit/trail")).StatusCode);
        var day=DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd");var result=await admin.GetFromJsonAsync<JsonElement>("/api/audit/trail?from="+day+"&to="+day+"&computer=Audit-PC-&activity="+Uri.EscapeDataString("Add / execute /api/suppliers")+"&outcome=Succeeded&page=1");
        Assert.True(result.GetProperty("total").GetInt32()>0);var rows=result.GetProperty("rows").EnumerateArray().ToArray();Assert.All(rows,x=>{Assert.Equal(day,x.GetProperty("date").GetString());Assert.Equal("Asia/Manila",x.GetProperty("timeZone").GetString());Assert.Matches("^\\d{2}:\\d{2}:\\d{2}$",x.GetProperty("time").GetString()!);});
        Assert.Equal(HttpStatusCode.Conflict,(await admin.GetAsync("/api/audit/trail?from=bad-date")).StatusCode);
    }
    [Fact] public async Task TimedSessionsAreOwnedHeartbeatBoundedAndSignoutIsConfirmed()
    {
        await AuditStation(reader);var session=await Post(reader,"sessions/start",new{});var id=session.GetProperty("Id").GetGuid();reader.DefaultRequestHeaders.Add("X-Audit-Session",id.ToString());
        await Post(reader,"sessions/"+id+"/heartbeat",new{});
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"sessions/"+id+"/heartbeat",new{})).StatusCode);
        var result=await admin.GetFromJsonAsync<JsonElement>("/api/audit/sessions?computer=Audit-PC-");var row=result.GetProperty("rows").EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==id);Assert.Equal("00h 00m",row.GetProperty("observedDuration").GetString());Assert.Equal("",row.GetProperty("signedOut").GetString());Assert.Equal("Connected",row.GetProperty("status").GetString());
        await Post(reader,"sessions/"+id+"/end",new{});reader.DefaultRequestHeaders.Remove("X-Audit-Session");
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(reader,"sessions/"+id+"/heartbeat",new{})).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var saved=await db.UserSessions.SingleAsync(x=>x.Id==id);Assert.NotNull(saved.EndedAt);Assert.True(saved.ExpiresAt<=saved.CreatedAt.AddMinutes(20));
    }
    [Fact] public async Task SessionCorrelationCannotBeForgedAndExpiredSessionCannotHeartbeat()
    {
        await AuditStation(reader);var session=await Post(reader,"sessions/start",new{});var id=session.GetProperty("Id").GetGuid();chief.DefaultRequestHeaders.Add("X-Audit-Session",id.ToString());
        Assert.Equal(HttpStatusCode.Conflict,(await chief.GetAsync("/api/dashboard")).StatusCode);chief.DefaultRequestHeaders.Remove("X-Audit-Session");
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var saved=await db.UserSessions.SingleAsync(x=>x.Id==id);saved.ExpiresAt=DateTime.UtcNow.AddSeconds(-1);await db.SaveChangesAsync();}
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(reader,"sessions/"+id+"/heartbeat",new{})).StatusCode);
    }
    [Fact] public async Task FileActivityIsMinimalClientReportedAndIdempotent()
    {
        await AuditStation(reader);var key=Guid.NewGuid().ToString();var input=new{action="Export CSV",outcome="Succeeded",fileName="audit-safe.csv",rows=17,page="Inventory Management"};await Post(reader,"audit/file-activity",input,key);await Post(reader,"audit/file-activity",input,key);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(reader,"audit/file-activity",input with{fileName="C:\\private\\secret.csv"})).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var row=await db.AuditEvents.SingleAsync(x=>x.RecordId==key);Assert.Contains("audit-safe.csv",row.Detail);Assert.Contains("ClientReported",row.Detail);Assert.Equal("Succeeded",row.Outcome);
        await Assert.ThrowsAnyAsync<Exception>(()=>db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"AuditEvents\" SET \"StaffEmail\" = 'tampered' WHERE \"Id\" = {row.Id}"));
    }
    [Fact] public async Task DailyTotalsSplitAtManilaMidnightAndCountOverlapOnce()
    {
        var computer="Audit-totals-"+Guid.NewGuid().ToString("N");var station=await AuditStation(reader);var me=await reader.GetFromJsonAsync<JsonElement>("/api/me");var date=DateTime.UtcNow.Date.AddDays(-4);var start=date.AddHours(15); // 23:00 Manila
        using(var scope=factory.Services.CreateScope()){
            var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();
            foreach(var offset in new[]{0,30})db.UserSessions.Add(new DDHI.Pharmacy.Domain.UserSession{UserId=me.GetProperty("id").GetString()!,WorkstationId=station,CreatedAt=start.AddMinutes(offset),LastSeenAt=start.AddHours(2),ExpiresAt=start.AddHours(3),Email="totals@test.invalid",Computer=computer,Roles=Roles.Reader});
            await db.SaveChangesAsync();
        }
        var from=date.ToString("yyyy-MM-dd");var to=date.AddDays(1).ToString("yyyy-MM-dd");var result=await admin.GetFromJsonAsync<JsonElement>("/api/audit/session-totals?from="+from+"&to="+to+"&computer="+computer);
        var rows=result.GetProperty("rows").EnumerateArray().ToArray();Assert.Equal(2,rows.Length);Assert.All(rows,row=>Assert.Equal("01h 00m",row.GetProperty("observedTotal").GetString()));
        Assert.Equal(HttpStatusCode.Forbidden,(await reader.GetAsync("/api/audit/session-totals")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.GetAsync("/api/audit/session-totals?from=2020-01-01&to=2026-01-01")).StatusCode);
    }
}
