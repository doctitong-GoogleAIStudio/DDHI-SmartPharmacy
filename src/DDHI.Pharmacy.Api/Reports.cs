using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
public static class Reports {
 public static async Task<object> Rows(string name,PharmacyDb db){
  if(name=="inventory"||name=="near-expiry"||name=="expired"||name=="valuation"){
   var q=db.InventoryBalances.AsNoTracking().AsQueryable();if(name=="near-expiry")q=q.Where(x=>x.Batch.Expiration>=Safety.Today&&x.Batch.Expiration<=Safety.Today.AddDays(180));if(name=="expired")q=q.Where(x=>x.Batch.Expiration<Safety.Today);
   return await q.OrderBy(x=>x.Batch.Expiration).Select(x=>new{x.Id,x.BatchId,Medicine=x.Batch.Medicine.GenericName,x.Batch.Medicine.Strength,x.Batch.Medicine.DosageForm,x.Batch.Lot,x.Batch.Expiration,Location=x.Location.Name,x.Quantity,x.Status,x.Batch.UnitCost,Value=x.Quantity*x.Batch.UnitCost,Recalled=db.RecallAffectedBatchs.Any(r=>r.BatchId==x.BatchId)}).Take(20000).ToListAsync();
  }
  if(name=="low-stock")return await db.Medicines.Select(m=>new{m.Id,m.GenericName,m.Strength,m.ReorderLevel,Quantity=db.InventoryBalances.Where(b=>b.Batch.MedicineId==m.Id&&b.Status==StockStatus.Available&&b.Batch.Expiration>=Safety.Today&&!db.RecallAffectedBatchs.Any(r=>r.BatchId==b.BatchId)).Sum(b=>b.Quantity)}).Where(x=>x.Quantity<=x.ReorderLevel).ToListAsync();
  if(name=="recalls")return await db.RecallAffectedBatchs.Select(x=>new{x.Recall.Reference,x.Recall.Reason,x.BatchId,x.Batch.Lot,x.Recall.Closed}).ToListAsync();
  if(name=="integration"||name=="failed-transactions")return await db.IntegrationOutboxs.Where(x=>name=="integration"||x.State==PostingState.NeedsReconciliation||x.State==PostingState.Rejected).Select(x=>new{x.Id,x.TransactionId,x.State,x.Detail,x.CreatedAt}).Take(20000).ToListAsync();
  if(name=="reconciliation")return await db.ReconciliationDiscrepancys.Select(x=>new{x.RunId,x.BalanceId,x.Expected,x.Actual,x.CreatedAt}).ToListAsync();
  if(name=="audit"||name=="user-activity")return await db.AuditEvents.Select(x=>new{x.Id,x.CreatedAt,x.Actor,x.Role,x.Workstation,x.Action,x.RecordId,x.Hash}).Take(20000).ToListAsync();
  if(name=="dispensing")return await db.DispenseTransactionLines.Select(x=>new{x.Id,x.TransactionId,x.Transaction.Order.Reference,x.Transaction.Order.EncounterReference,x.BalanceId,x.Quantity,x.Transaction.Pharmacist,x.Transaction.Verification,x.CreatedAt}).Take(20000).ToListAsync();
  var kinds=name switch{"receiving"=>new[]{"Receiving"},"adjustments"=>new[]{"CountAdjustment"},"returns"=>new[]{"ReturnQuarantine"},"transfers"=>new[]{"TransferIn","TransferOut"},"movements" or "traceability"=>Array.Empty<string>(),_=>throw new InvalidOperationException("Unknown report.")};
  return await db.StockMovements.Where(x=>kinds.Length==0||kinds.Contains(x.Kind)).OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.BalanceId,x.Balance.BatchId,Medicine=x.Balance.Batch.Medicine.GenericName,x.Balance.Batch.Lot,x.Delta,x.Kind,x.Reference,x.Actor,x.Reason,x.CreatedAt}).Take(20000).ToListAsync();
 }
}
