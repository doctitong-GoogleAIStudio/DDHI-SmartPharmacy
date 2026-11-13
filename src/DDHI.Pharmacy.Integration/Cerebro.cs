namespace DDHI.Pharmacy.Integration;
public record IntegrationResult(bool Success,string Message);
public record MedicineSyncResult(bool Success,int Count,string Message);
public record OrderLookupResult(bool Success,string Message);
public record DispensingPayload(Guid TransactionId,string Reference);
public record PostingResult(string State,string Message);
public interface ICerebroIntegration {
 Task<IntegrationResult> TestConnectionAsync(CancellationToken cancellationToken);
 Task<MedicineSyncResult> SyncMedicinesAsync(CancellationToken cancellationToken);
 Task<OrderLookupResult> GetMedicationOrderAsync(string orderId,CancellationToken cancellationToken);
 Task<PostingResult> PostDispensingAsync(DispensingPayload payload,CancellationToken cancellationToken);
}
public sealed class MockCerebroIntegration : ICerebroIntegration {
 public Task<IntegrationResult> TestConnectionAsync(CancellationToken c)=>Task.FromResult(new IntegrationResult(true,"MOCK adapter only. No Cerebro connection."));
 public Task<MedicineSyncResult> SyncMedicinesAsync(CancellationToken c)=>Task.FromResult(new MedicineSyncResult(true,0,"Mock only; no medicine master changed."));
 public Task<OrderLookupResult> GetMedicationOrderAsync(string id,CancellationToken c)=>Task.FromResult(new OrderLookupResult(false,"No authorized vendor order interface configured."));
 public Task<PostingResult> PostDispensingAsync(DispensingPayload p,CancellationToken c)=>Task.FromResult(new PostingResult("NeedsReconciliation","Mock cannot acknowledge production transactions."));
}
public abstract class DisabledVendorAdapter : ICerebroIntegration {
 protected static Exception Blocked()=>new InvalidOperationException("Vendor contract and written hospital authorization are required. Live integration disabled.");
 public Task<IntegrationResult> TestConnectionAsync(CancellationToken c)=>throw Blocked();
 public Task<MedicineSyncResult> SyncMedicinesAsync(CancellationToken c)=>throw Blocked();
 public Task<OrderLookupResult> GetMedicationOrderAsync(string id,CancellationToken c)=>throw Blocked();
 public Task<PostingResult> PostDispensingAsync(DispensingPayload p,CancellationToken c)=>throw Blocked();
}
public sealed class ApprovedRestAdapter : DisabledVendorAdapter;
public sealed class ApprovedReadOnlyDatabaseAdapter : DisabledVendorAdapter;
public static class CsvAdapter {
 public static string Escape(string value){if(value.Length>0&&"=+-@\t\r".Contains(value[0]))value="'"+value;return "\""+value.Replace("\"","\"\"")+"\"";}
 public static string Export(IEnumerable<string[]> rows)=>string.Join("\r\n",rows.Select(r=>string.Join(",",r.Select(Escape))));
}
