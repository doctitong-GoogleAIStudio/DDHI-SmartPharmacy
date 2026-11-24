using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DDHI.Pharmacy.Infrastructure;

public sealed record AuditIdentity(string Email,string Name,string Roles,string Computer);

// Only business fields are captured. Identity credentials, stamps, tokens, private keys,
// raw imported files and operation payloads must never enter an audit record.
public sealed class AuditChangeCapture
{
    static readonly HashSet<string> IdentityFields=["Id","UserId","RoleId","Email","UserName","PhoneNumber","LockoutEnd","ClaimType","ClaimValue"];
    readonly Dictionary<string,object> changes=[];
    public void Note(string record,string id,string field,string? before,string? after)=>changes[$"{record}:{id}:{field}:explicit"]=new{Record=record,Id=id,Activity="Changed",Field=field,Before=before,After=after};
    public void Capture(ChangeTracker tracker)
    {
        tracker.DetectChanges();
        foreach(var entry in tracker.Entries().Where(e=>e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var type=entry.Metadata.ClrType.Name;
            if(type.StartsWith("Clinical")||type is "PharmacistIntervention" or "DrugIngredient" or "ReferencePreference")continue;
            if(type is "AuditEvent" or "OperationKey" or "UserSession" or "IntegrationOutbox" or "IntegrationInbox" or "IntegrationAcknowledgment")continue;
            if(type.StartsWith("IdentityUserToken")||type.StartsWith("IdentityUserLogin"))continue;
            var id=string.Join(",",entry.Properties.Where(p=>p.Metadata.IsPrimaryKey()).Select(p=>p.CurrentValue?.ToString()));
            foreach(var property in entry.Properties)
            {
                var field=property.Metadata.Name;
                if(field is "Id" or "CreatedAt" or "UpdatedAt")continue;
                if(entry.State==EntityState.Modified&&!property.IsModified)continue;
                if((type==nameof(PharmacyUser)||type.StartsWith("Identity"))&&!IdentityFields.Contains(field))continue;
                if(type=="SystemSetting"&&field=="Value")continue;
                if(field.Contains("Password",StringComparison.OrdinalIgnoreCase)||field.Contains("Token",StringComparison.OrdinalIgnoreCase)||field.Contains("Secret",StringComparison.OrdinalIgnoreCase)||field.Contains("Stamp",StringComparison.OrdinalIgnoreCase)||field.Contains("Payload",StringComparison.OrdinalIgnoreCase))continue;
                if(field=="ClaimValue"&&entry.Property("ClaimType").CurrentValue?.ToString()!="ddhi:full_name")continue;
                string? Value(object? value)=>value is null?null:value is DateTime time?time.ToUniversalTime().ToString("O"):value.ToString() is string text?text.Length>160?text[..160]+" [truncated]":text:null;
                var key=$"{type}:{id}:{field}:{entry.State}";
                changes[key]=new{Record=type,Id=id,Activity=entry.State.ToString(),Field=field,Before=entry.State==EntityState.Added?null:Value(property.OriginalValue),After=entry.State==EntityState.Deleted?null:Value(property.CurrentValue)};
            }
        }
    }
    public string Json()
    {
        var all=changes.Values.ToArray();var selected=new List<object>();
        foreach(var item in all){selected.Add(item);if(JsonSerializer.Serialize(new{Fields=selected,OmittedFields=all.Length-selected.Count}).Length>1700){selected.RemoveAt(selected.Count-1);break;}}
        return JsonSerializer.Serialize(new{Fields=selected,OmittedFields=all.Length-selected.Count,AllChangesSha256=InventoryService.Hash(JsonSerializer.Serialize(all))});
    }
    public static string SafeDetail(object request,string result,string requestHash)
    {
        using var parsed=JsonDocument.Parse(result);
        var id=parsed.RootElement.ValueKind==JsonValueKind.Object&&parsed.RootElement.TryGetProperty("Id",out var value)?value.ToString():"";
        var file=request.GetType().GetProperty("Action")?.GetValue(request)?.ToString()?.StartsWith("Client file activity: ")==true;
        return JsonSerializer.Serialize(new{Path=request.GetType().GetProperty("Path")?.GetValue(request)?.ToString(),ResultId=id,RequestSha256=requestHash,ResultSha256=InventoryService.Hash(result),FileActivity=file?parsed.RootElement:(JsonElement?)null});
    }
}
