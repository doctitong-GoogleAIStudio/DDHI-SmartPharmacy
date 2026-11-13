
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Application;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;
// Call inside InventoryService.Execute so all balance, ledger and audit writes share the serialization lock.
public sealed class LifecycleService(PharmacyDb db) {
 static void Approve(bool confirm,string reason){Safety.Require(confirm&&!string.IsNullOrWhiteSpace(reason)&&reason.Length<=500,"Explicit confirmation and an authorized reason/reference are required.");}
 async Task<InventoryBalance> Balance(Guid id)=>await db.InventoryBalances.Include(x=>x.Batch).SingleAsync(x=>x.Id==id);
 async Task<decimal> Reserved(Guid id)=>await db.StockReservations.Where(x=>x.BalanceId==id&&!x.Released&&x.ExpiresAt>DateTime.UtcNow).SumAsync(x=>x.Quantity);
 void Move(InventoryBalance b,decimal delta,string kind,string reference,Actor a,string reason,Guid? original=null){b.Quantity+=delta;Safety.Require(b.Quantity>=0,"Insufficient stock for this movement.");db.StockMovements.Add(new StockMovement{Balance=b,Delta=delta,Kind=kind,Reference=reference,Actor=a.UserId,Reason=reason,ReversesMovementId=original});}
 public async Task<object> ChangeStatus(StockStateInput r,Actor a){
  Approve(r.Confirmed,r.ApprovalReference);Safety.Positive(r.Quantity);Safety.Require(!string.IsNullOrWhiteSpace(r.Reference),"Transaction reference required.");
  Safety.Require(Enum.TryParse<StockStatus>(r.TargetStatus,out var status)&&status is StockStatus.Available or StockStatus.Quarantined or StockStatus.Damaged or StockStatus.PendingVerification,"Unsupported stock status transition.");
  var source=await Balance(r.BalanceId);Safety.Require(source.Status!=status,"Stock already has this status.");Safety.Require(source.Quantity-r.Quantity>=await Reserved(source.Id),"Insufficient unreserved stock.");
  if(status==StockStatus.Available){Safety.Require(a.Role==Roles.Chief,"Chief Pharmacist approval is required to release held stock.");Safety.Require(source.Batch.Expiration>=Safety.Today,"Expired stock cannot be released.");Safety.Require(!await db.RecallAffectedBatchs.AnyAsync(x=>x.BatchId==source.BatchId),"Recalled stock cannot be released.");}
  var target=await db.InventoryBalances.SingleOrDefaultAsync(x=>x.BatchId==source.BatchId&&x.LocationId==source.LocationId&&x.Status==status);
  if(target==null){target=new InventoryBalance{BatchId=source.BatchId,LocationId=source.LocationId,Status=status};db.InventoryBalances.Add(target);}
  Move(source,-r.Quantity,"StatusOut",r.Reference,a,r.ApprovalReference);Move(target,r.Quantity,"StatusIn",r.Reference,a,r.ApprovalReference);return new{source.Id,TargetBalanceId=target.Id,TargetStatus=status.ToString()};
 }
 public async Task<object> Dispose(DisposeInput r,Actor a){Approve(r.Confirmed,r.Reason);Safety.Positive(r.Quantity);Safety.Require(!string.IsNullOrWhiteSpace(r.Reference),"Disposal reference required.");var b=await Balance(r.BalanceId);Safety.Require(b.Quantity-r.Quantity>=await Reserved(b.Id),"Insufficient unreserved stock.");Move(b,-r.Quantity,"Disposal",r.Reference,a,r.Reason);return new{b.Id,b.Quantity,Status="Physical disposal/return-to-supplier must follow the referenced hospital procedure."};}
 public async Task<object> CloseRecall(Guid id,ReasonInput r){Approve(r.Confirmed,r.Reason);var notice=await db.RecallNotices.SingleAsync(x=>x.Id==id);Safety.Require(!notice.Closed,"Recall is already closed.");var batches=await db.RecallAffectedBatchs.Where(x=>x.RecallId==id).Select(x=>x.BatchId).ToListAsync();Safety.Require(!await db.InventoryBalances.AnyAsync(x=>batches.Contains(x.BatchId)&&x.Quantity>0),"Recall cannot close while affected stock remains. Record authorized disposition first.");notice.Closed=true;return new{notice.Id,notice.Closed,message="Historical affected-lot blocks remain in force."};}
 public async Task<object> ReleaseReservation(Guid id,ReasonInput r){Approve(r.Confirmed,r.Reason);var item=await db.StockReservations.SingleAsync(x=>x.Id==id);Safety.Require(!item.Released,"Reservation already released.");item.Released=true;return new{item.Id,item.Released};}
 public async Task<object> Reverse(Guid id,ReasonInput r,Actor a){
  Approve(r.Confirmed,r.Reason);var original=await db.StockMovements.AsNoTracking().SingleAsync(x=>x.Id==id);Safety.Require(original.Kind is "Receiving" or "CountAdjustment" or "TransferIn" or "TransferOut","Use the quarantined-return workflow for dispensing. Status changes, disposal and reversals require their own approved correction workflow.");
  var originals=original.Kind.StartsWith("Transfer")?await db.StockMovements.AsNoTracking().Where(x=>x.Reference==original.Reference&&(x.Kind=="TransferIn"||x.Kind=="TransferOut")).ToListAsync():[original];
  Safety.Require(!original.Kind.StartsWith("Transfer")||originals.Count==2&&originals.Sum(x=>x.Delta)==0,"Ambiguous transfer reference requires reconciliation.");
  foreach(var item in originals){Safety.Require(!await db.StockMovements.AnyAsync(x=>x.ReversesMovementId==item.Id),"This movement is already reversed.");var balance=await Balance(item.BalanceId);Safety.Require(balance.Quantity-item.Delta>=await Reserved(balance.Id),"Reversal would consume unavailable or reserved stock.");Move(balance,-item.Delta,"Reversal","REV-"+item.Id,a,r.Reason,item.Id);}
  return new{reversed=originals.Select(x=>x.Id).ToArray()};
 }
}
