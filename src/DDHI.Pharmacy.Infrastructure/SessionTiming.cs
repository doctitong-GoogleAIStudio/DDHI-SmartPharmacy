using DDHI.Pharmacy.Domain;

namespace DDHI.Pharmacy.Infrastructure;

public static class SessionTiming
{
    public static DateTime ObservedEnd(UserSession session)=>new[]{session.EndedAt??session.LastSeenAt,session.ExpiresAt}.Min()<session.CreatedAt?session.CreatedAt:new[]{session.EndedAt??session.LastSeenAt,session.ExpiresAt}.Min();
    public static string HoursMinutes(TimeSpan duration)=>$"{(long)Math.Max(0,duration.TotalHours):00}h {Math.Max(0,duration.Minutes):00}m";
    public static TimeSpan ObservedDuration(UserSession session)=>ObservedEnd(session)-session.CreatedAt;
    // Union overlapping sessions before calculating a user's total for one computer/day.
    public static TimeSpan Total(IEnumerable<(DateTime Start,DateTime End)> intervals)
    {
        DateTime? start=null,end=null;var total=TimeSpan.Zero;
        foreach(var item in intervals.Where(x=>x.End>x.Start).OrderBy(x=>x.Start))
        {if(start==null){start=item.Start;end=item.End;}else if(item.Start<=end){if(item.End>end)end=item.End;}else{total+=end!.Value-start.Value;start=item.Start;end=item.End;}}
        return total+(start==null?TimeSpan.Zero:end!.Value-start.Value);
    }
}
