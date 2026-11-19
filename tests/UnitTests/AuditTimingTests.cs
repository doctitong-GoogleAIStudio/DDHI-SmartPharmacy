using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Xunit;

public class AuditTimingTests
{
    static DateTime At(int hour,int minute=0)=>new(2026,11,19,hour,minute,0,DateTimeKind.Utc);
    [Fact] public void LegacyAuditHashesRemainByteForByteCompatible()
    {
        var row=new AuditEvent{FormatVersion=0,Actor="old",Role="Auditor",Detail="untouched",CreatedAt=At(1)};
        var expected=InventoryService.Hash($"{row.PreviousHash}|{row.Id}|{row.CreatedAt:O}|{row.Actor}|{row.Role}|{row.Workstation}|{row.Action}|{row.RecordId}|{row.Detail}");
        Assert.Equal(expected,InventoryService.AuditHash(row));row.FormatVersion=1;Assert.Equal(expected,InventoryService.AuditHash(row));
        row.FormatVersion=2;row.StaffEmail="staff@test.invalid";var hash=InventoryService.AuditHash(row);row.Computer="changed";Assert.NotEqual(hash,InventoryService.AuditHash(row));
    }
    [Fact] public void DurationUsesLastContactUntilConfirmedSignoutAndCapsAtExpiry()
    {
        var row=new UserSession{CreatedAt=At(1),LastSeenAt=At(3,7),ExpiresAt=At(4)};
        Assert.Equal("02h 07m",SessionTiming.HoursMinutes(SessionTiming.ObservedDuration(row)));
        row.EndedAt=At(3,11);Assert.Equal("02h 11m",SessionTiming.HoursMinutes(SessionTiming.ObservedDuration(row)));
        row.EndedAt=At(5);Assert.Equal("03h 00m",SessionTiming.HoursMinutes(SessionTiming.ObservedDuration(row)));
        row.EndedAt=At(0);Assert.Equal(TimeSpan.Zero,SessionTiming.ObservedDuration(row));
    }
    [Fact] public void TotalsDoNotDoubleCountOverlappingSessions()
    {
        var result=SessionTiming.Total([(At(1),At(2)),(At(1,30),At(3)),(At(4),At(4,15))]);
        Assert.Equal("02h 15m",SessionTiming.HoursMinutes(result));
    }
    [Fact] public void SafeDetailsExcludePasswordsRawFileContentsAndPatientPayloads()
    {
        var detail=AuditChangeCapture.SafeDetail(new{Path="/api/imports/medicines",Password="NeverRecord",Csv="PrivateFile",Patient="PrivatePatient"},"{\"Id\":\"record\",\"Token\":\"SecretToken\"}","hash");
        Assert.Contains("record",detail);foreach(var secret in new[]{"NeverRecord","PrivateFile","PrivatePatient","SecretToken"})Assert.DoesNotContain(secret,detail);
    }
}
