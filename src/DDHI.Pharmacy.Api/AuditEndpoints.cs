using System.Globalization;
using System.Security.Claims;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;

static class AuditEndpoints
{
    public static string Workstation(HttpContext c)=>c.Request.Headers["X-Workstation"].ToString() is {Length:>0 and <=128} station?station:"Not supplied";
    public static string Session(HttpContext c)=>Guid.TryParse(c.Request.Headers["X-Audit-Session"],out var id)?id.ToString():"";
    static Actor Actor(HttpContext c)=>new(c.User.FindFirstValue(ClaimTypes.NameIdentifier)??"",string.Join(", ",c.User.FindAll(ClaimTypes.Role).Select(x=>x.Value)),Workstation(c),c.Connection.RemoteIpAddress?.ToString()??"",Session(c));
    static string Local(DateTime? value,string format="yyyy-MM-dd HH:mm:ss")=>value?.ToUniversalTime().AddHours(8).ToString(format,CultureInfo.InvariantCulture)??"";
    static DateTime? Day(string? value){if(string.IsNullOrWhiteSpace(value))return null;Safety.Require(DateOnly.TryParseExact(value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var day),"Use yyyy-MM-dd for dates.");return DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue).AddHours(-8),DateTimeKind.Utc);}
    static async Task<UserSession> Owned(PharmacyDb db,HttpContext c,Guid id)
    {
        var session=await db.UserSessions.SingleOrDefaultAsync(s=>s.Id==id&&s.UserId==c.User.FindFirstValue(ClaimTypes.NameIdentifier));
        Safety.Require(session!=null&&session.WorkstationId.ToString()==Workstation(c),"Session does not belong to this account and workstation.");return session!;
    }
    public static async Task ObserveRequests(HttpContext c,RequestDelegate next)
    {
        var path=c.Request.Path.ToString();var failed=false;
        try {if(Session(c) is {Length:>0} supplied&&c.User.Identity?.IsAuthenticated==true){var db=c.RequestServices.GetRequiredService<PharmacyDb>();var session=await Owned(db,c,Guid.Parse(supplied));Safety.Require(path==$"/api/sessions/{supplied}/end"||(session.EndedAt==null&&session.ExpiresAt>DateTime.UtcNow),"Session ended. Sign in again.");}await next(c);}catch{failed=true;throw;}
        finally
        {
            // Successful writes are audited atomically by InventoryService.Execute.
            // Read/denied events use a fresh scope after an aborted mutation rolls back.
            if(c.User.Identity?.IsAuthenticated==true&&path.StartsWith("/api/")&&!path.StartsWith("/api/sessions")&&!path.StartsWith("/api/audit/file-activity")&&
                (c.Request.Method=="GET"||failed||c.Response.StatusCode>=400))
            {
                try{await using var scope=c.RequestServices.CreateAsyncScope();var service=scope.ServiceProvider.GetRequiredService<InventoryService>();
                    await service.Record(Actor(c),(c.Request.Method=="GET"?"View ":"Attempt ")+path,"",$"HTTP {(failed?409:c.Response.StatusCode)}; no request body recorded.",failed?"Failed":c.Response.StatusCode>=400?"Denied":"Succeeded");}
                catch(Exception ex){c.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Audit").LogError(ex,"Audit recording unavailable for {Method} {Path}",c.Request.Method,path);}
            }
        }
    }
    public static void Map(RouteGroupBuilder api)
    {
        api.MapGet("/audit/trail",async(PharmacyDb db,string? from,string? to,string? staff,string? computer,string? activity,string? outcome,int? page)=>{
            var start=Day(from);var end=Day(to)?.AddDays(1);Safety.Require(start==null||end==null||start<end,"End date must not precede start date.");
            var query=db.AuditEvents.AsNoTracking().AsQueryable();if(start!=null)query=query.Where(x=>x.CreatedAt>=start);if(end!=null)query=query.Where(x=>x.CreatedAt<end);
            if(!string.IsNullOrWhiteSpace(staff))query=query.Where(x=>x.StaffEmail.Contains(staff)||x.StaffName.Contains(staff)||x.Actor.Contains(staff));
            if(!string.IsNullOrWhiteSpace(computer))query=query.Where(x=>x.Computer.Contains(computer)||x.Workstation.Contains(computer));
            if(!string.IsNullOrWhiteSpace(activity))query=query.Where(x=>x.Action.Contains(activity));if(!string.IsNullOrWhiteSpace(outcome))query=query.Where(x=>x.Outcome==outcome);
            var number=Math.Clamp(page??1,1,100000);var count=await query.CountAsync();var rows=await query.OrderByDescending(x=>x.CreatedAt).ThenByDescending(x=>x.Id).Skip((number-1)*200).Take(200).ToListAsync();
            return new{page=number,total=count,pageSize=200,rows=rows.Select(x=>new{x.Id,Date=Local(x.CreatedAt,"yyyy-MM-dd"),Time=Local(x.CreatedAt,"HH:mm:ss"),TimeZone="Asia/Manila",Staff=x.FormatVersion<2?x.Actor:x.StaffEmail,Name=x.StaffName,Roles=x.FormatVersion<2?x.Role:x.StaffRoles,Computer=x.FormatVersion<2?x.Workstation:x.Computer,Activity=x.Action,Outcome=x.Outcome,Record=x.RecordId,IdentityEvidence=x.FormatVersion<2?"Legacy record; historical name not captured":"Snapshot at activity time; computer name is client-reported",x.IpAddress,x.SessionId,x.Changes,x.Detail,x.Hash})};
        }).RequireAuthorization("Audit");
        api.MapGet("/audit/session-totals",async(PharmacyDb db,string? from,string? to,string? staff,string? computer,int? page)=>{
            var end=Day(to)?.AddDays(1)??Day(DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd"))!.Value.AddDays(1);var start=Day(from)??end.AddDays(-30);
            Safety.Require(start<end&&(end-start).TotalDays<=366,"Select a date range of up to 366 days.");
            var query=db.UserSessions.AsNoTracking().Where(x=>x.CreatedAt<end&&x.LastSeenAt>=start);
            if(!string.IsNullOrWhiteSpace(staff))query=query.Where(x=>x.Email.Contains(staff));if(!string.IsNullOrWhiteSpace(computer))query=query.Where(x=>x.Computer.Contains(computer));
            Safety.Require(await query.CountAsync()<=20000,"Too many sessions. Narrow the date, staff or computer filters.");var sessions=await query.ToListAsync();
            var intervals=new List<(string User,string Email,string Roles,string Computer,DateTime Day,DateTime Start,DateTime End)>();
            foreach(var session in sessions){var begin=session.CreatedAt>start?session.CreatedAt:start;var stop=SessionTiming.ObservedEnd(session)<end?SessionTiming.ObservedEnd(session):end;
                while(begin<stop){var day=begin.AddHours(8).Date;var next=DateTime.SpecifyKind(day.AddDays(1).AddHours(-8),DateTimeKind.Utc);var finish=stop<next?stop:next;intervals.Add((session.UserId,session.Email,session.Roles,session.Computer,day,begin,finish));begin=finish;}}
            var groups=intervals.GroupBy(x=>new{x.User,x.Computer,x.Day}).OrderByDescending(x=>x.Key.Day).ThenBy(x=>x.Key.User).ToArray();var number=Math.Clamp(page??1,1,100000);
            return new{page=number,total=groups.Length,pageSize=200,rows=groups.Skip((number-1)*200).Take(200).Select(g=>new{Date=g.Key.Day.ToString("yyyy-MM-dd"),Staff=string.Join(", ",g.Select(x=>x.Email).Distinct()),Roles=string.Join("; ",g.Select(x=>x.Roles).Distinct()),g.Key.Computer,ObservedTotal=SessionTiming.HoursMinutes(SessionTiming.Total(g.Select(x=>(x.Start,x.End)))),TimeZone="Asia/Manila",Meaning="Observed session intervals, overlap counted once per user/computer/day; not active work or attendance"})};
        }).RequireAuthorization("Audit");
        api.MapPost("/sessions/start",async(HttpContext c,PharmacyDb db,InventoryService service)=>{
            var actor=Actor(c);Safety.Require(Guid.TryParse(actor.Workstation,out var id)&&await db.Workstations.AnyAsync(w=>w.Id==id),"Register this workstation first.");
            var result=await service.Execute(c.Request.Headers["Idempotency-Key"].ToString(),actor,new{Action="Session started"},async()=>{
                var identity=await service.Identity(actor);var now=DateTime.UtcNow;var session=new UserSession{UserId=actor.UserId,WorkstationId=id,CreatedAt=now,LastSeenAt=now,ExpiresAt=DateTime.TryParse(c.User.FindFirstValue("ddhi:expires"),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out var expires)?expires.ToUniversalTime():now.AddMinutes(20),Email=identity.Email,Roles=identity.Roles,Computer=identity.Computer};
                db.UserSessions.Add(session);return new{session.Id,session.CreatedAt,session.ExpiresAt};});return Results.Content(result,"application/json");
        });
        api.MapPost("/sessions/{id:guid}/heartbeat",async(Guid id,HttpContext c,PharmacyDb db)=>{
            await using var tx=await db.Database.BeginTransactionAsync();await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(772991001)");var session=await Owned(db,c,id);
            Safety.Require(session.EndedAt==null&&session.ExpiresAt>DateTime.UtcNow,"Session has ended. Sign in again.");session.LastSeenAt=DateTime.UtcNow;await db.SaveChangesAsync();await tx.CommitAsync();return new{recorded=true};
        });
        api.MapPost("/sessions/{id:guid}/end",async(Guid id,HttpContext c,PharmacyDb db,InventoryService service)=>{
            var result=await service.Execute(c.Request.Headers["Idempotency-Key"].ToString(),Actor(c),new{Action="Session signed out",Id=id},async()=>{var session=await Owned(db,c,id);if(session.EndedAt==null){session.LastSeenAt=new[]{DateTime.UtcNow,session.ExpiresAt}.Min();session.EndedAt=session.LastSeenAt;session.EndReason="Client reported sign-out / window closing";}return new{session.Id,session.EndedAt,session.EndReason};});return Results.Content(result,"application/json");
        });
        api.MapGet("/audit/sessions",async(PharmacyDb db,string? from,string? to,string? staff,string? computer,int? page)=>{
            var start=Day(from);var end=Day(to)?.AddDays(1);var query=db.UserSessions.AsNoTracking().Where(x=>x.LastSeenAt!=default);
            if(start!=null)query=query.Where(x=>x.CreatedAt>=start);if(end!=null)query=query.Where(x=>x.CreatedAt<end);
            if(!string.IsNullOrWhiteSpace(staff))query=query.Where(x=>x.Email.Contains(staff));if(!string.IsNullOrWhiteSpace(computer))query=query.Where(x=>x.Computer.Contains(computer));
            var number=Math.Clamp(page??1,1,100000);var count=await query.CountAsync();var sessions=await query.OrderByDescending(x=>x.CreatedAt).Skip((number-1)*200).Take(200).ToListAsync();
            return new{page=number,total=count,pageSize=200,rows=sessions.Select(s=>new{s.Id,Staff=s.Email,s.Roles,s.Computer,Started=Local(s.CreatedAt),LastContact=Local(s.LastSeenAt),SignedOut=Local(s.EndedAt),ObservedDuration=SessionTiming.HoursMinutes(SessionTiming.ObservedDuration(s)),TimeZone="Asia/Manila",Status=s.EndedAt!=null?"Signed out":s.ExpiresAt<=DateTime.UtcNow?"Expired; end unconfirmed":s.LastSeenAt<DateTime.UtcNow.AddMinutes(-2)?"No recent contact; end unconfirmed":"Connected",s.EndReason,Meaning="Observed app session, not active work or attendance"})};
        }).RequireAuthorization("Audit");
        api.MapPost("/audit/file-activity",async(FileActivity input,HttpContext c,InventoryService service)=>{
            Safety.Require(input.Action is "Export CSV" or "Import CSV" or "Print report" or "Print stock label" or "Open manual" or "Decode image" or "Export clinical report" or "Import clinical package","Unsupported file activity.");
            Safety.Require(input.Outcome is "Started" or "Succeeded" or "Failed" or "Canceled","Unsupported activity outcome.");
            Safety.Require(input.FileName.Length<=240&&!input.FileName.Any(char.IsControl)&&!input.FileName.Contains('/')&&!input.FileName.Contains('\\')&&input.Rows>=0&&input.Rows<=1000000&&input.Page.Length<=80,"Use a file base name, valid row count and page title.");
            var json=await service.Execute(c.Request.Headers["Idempotency-Key"].ToString(),Actor(c),new{Action="Client file activity: "+input.Action,Input=input},()=>Task.FromResult<object>(new{ClientReported=true,input.Outcome,input.FileName,input.Rows,input.Page,Note="App-handled file action only; server cannot verify local file/printer delivery."}));return Results.Content(json,"application/json");
        });
    }
    public sealed record FileActivity(string Action,string Outcome,string FileName,int Rows,string Page);
}
