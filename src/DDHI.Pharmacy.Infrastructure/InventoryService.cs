using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Barcode;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;
public sealed class InventoryService(PharmacyDb db) {
 public static string Hash(string v)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
 public static string AuditHash(AuditEvent e) {
  var legacy=$"{e.PreviousHash}|{e.Id}|{e.CreatedAt:O}|{e.Actor}|{e.Role}|{e.Workstation}|{e.Action}|{e.RecordId}|{e.Detail}";
  return Hash(e.FormatVersion<=1?legacy:JsonSerializer.Serialize(new{legacy,e.FormatVersion,e.StaffEmail,e.StaffName,e.StaffRoles,e.Computer,e.IpAddress,e.Outcome,e.Changes,e.SessionId}));
 }
 public async Task<AuditIdentity> Identity(Actor actor) {
  var user=await db.Users.AsNoTracking().SingleOrDefaultAsync(u=>u.Id==actor.UserId);
  var roles=await (from ur in db.UserRoles join role in db.Roles on ur.RoleId equals role.Id where ur.UserId==actor.UserId orderby role.Name select role.Name).ToListAsync();
  var name=await db.UserClaims.Where(c=>c.UserId==actor.UserId&&c.ClaimType=="ddhi:full_name").Select(c=>c.ClaimValue).FirstOrDefaultAsync();
  var computer=Guid.TryParse(actor.Workstation,out var station)?await db.Workstations.Where(w=>w.Id==station).Select(w=>w.Name).FirstOrDefaultAsync():actor.Workstation;
  return new(user?.Email??actor.UserId,name??"",string.Join(", ",roles),computer??"Unregistered workstation ("+actor.Workstation+")");
 }
 public async Task Audit(Actor actor,string action,string record,string detail,string outcome="Succeeded",string changes="",AuditIdentity? identity=null) {
  identity??=await Identity(actor);
  var previous=await db.AuditEvents.OrderByDescending(e=>e.CreatedAt).ThenByDescending(e=>e.Id).Select(e=>e.Hash).FirstOrDefaultAsync()??"GENESIS";
  var lastTime=await db.AuditEvents.MaxAsync(e=>(DateTime?)e.CreatedAt);
  var e=new AuditEvent{FormatVersion=2,Actor=actor.UserId,Role=actor.Role,Workstation=actor.Workstation,Action=action,RecordId=record,Detail=detail,PreviousHash=previous,CreatedAt=DateTime.UtcNow,
   StaffEmail=identity.Email,StaffName=identity.Name,StaffRoles=identity.Roles,Computer=identity.Computer,IpAddress=actor.IpAddress,SessionId=actor.SessionId,Outcome=outcome,Changes=changes};
  e.CreatedAt=new DateTime(e.CreatedAt.Ticks-e.CreatedAt.Ticks%10,DateTimeKind.Utc);if(lastTime>=e.CreatedAt)e.CreatedAt=lastTime.Value.AddTicks(10);
  e.Hash=AuditHash(e);db.AuditEvents.Add(e);
 }
 public async Task Record(Actor actor,string action,string record,string detail,string outcome="Succeeded") {
  await using var tx=await db.Database.BeginTransactionAsync();await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(772991001)");
  await Audit(actor,action,record,detail,outcome);await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task<string> Execute(string key,Actor actor,object request,Func<Task<object>> action) {
  Safety.Require(Guid.TryParse(key,out _),"A UUID Idempotency-Key header is required.");
  Safety.Require(!string.IsNullOrWhiteSpace(actor.Workstation)&&actor.Workstation.Length<=128,"Workstation identity required.");
  var fingerprint=Hash(JsonSerializer.Serialize(request));
  await using var tx=await db.Database.BeginTransactionAsync();
  await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(772991001)");
  var old=await db.OperationKeys.SingleOrDefaultAsync(x=>x.Key==key);
  if(old!=null){Safety.Require(old.PayloadHash==fingerprint&&old.Actor==actor.UserId,"Idempotency key reused for a different request.");return old.Result;}
  var identity=await Identity(actor);var capture=new AuditChangeCapture();db.AuditCapture=capture;
  string result;
  try { result=JsonSerializer.Serialize(await action());capture.Capture(db.ChangeTracker); }
  finally { db.AuditCapture=null; }
  var path=request.GetType().GetProperty("Path")?.GetValue(request)?.ToString()??"";
  var label=request.GetType().GetProperty("Action")?.GetValue(request)?.ToString()??(path.EndsWith("/delete")?"Delete "+path:path.EndsWith("/edit")||path.EndsWith("/trade")||path.EndsWith("/status")||path.EndsWith("/configure")?"Edit "+path:path.EndsWith("/lock")?"Change account access "+path:"Add / execute "+path);
  var detail=AuditChangeCapture.SafeDetail(request,result,fingerprint);
  var outcome="Succeeded";if(label.StartsWith("Client file activity: ")){using var fileResult=JsonDocument.Parse(result);outcome=fileResult.RootElement.GetProperty("Outcome").GetString()??outcome;}
  await Audit(actor,label,key,detail,outcome,changes:capture.Json(),identity:identity);
  db.OperationKeys.Add(new OperationKey{Key=key,Actor=actor.UserId,PayloadHash=fingerprint,Result=result});
  await db.SaveChangesAsync();await tx.CommitAsync();return result;
 }
 static void Confirm(bool c)=>Safety.Require(c,"Explicit confirmation is required.");
 static string Text(string value,string name){Safety.Require(!string.IsNullOrWhiteSpace(value)&&value.Length<=200,$"{name} is required (max 200 characters).");return value.Trim();}
 async Task<InventoryBalance> Balance(Guid id)=>await db.InventoryBalances.Include(x=>x.Batch).Include(x=>x.Location).SingleAsync(x=>x.Id==id);
 async Task<InventoryBalance> Target(InventoryBalance source,Guid location,StockStatus status){
  Safety.Require(await db.InventoryLocations.AnyAsync(x=>x.Id==location),"Unknown location.");
  var b=await db.InventoryBalances.SingleOrDefaultAsync(x=>x.BatchId==source.BatchId&&x.LocationId==location&&x.Status==status);
  if(b==null){b=new InventoryBalance{BatchId=source.BatchId,LocationId=location,Status=status};db.InventoryBalances.Add(b);}return b;
 }
 void Movement(InventoryBalance b,decimal delta,string kind,string reference,string actor,string reason="") {b.Quantity+=delta;Safety.Require(b.Quantity>=0,"Negative stock blocked.");db.StockMovements.Add(new StockMovement{Balance=b,Delta=delta,Kind=kind,Reference=reference,Actor=actor,Reason=reason});}
 async Task<decimal> Reserved(Guid balance,Guid? except=null)=>await db.StockReservations.Where(x=>x.BalanceId==balance&&!x.Released&&x.ExpiresAt>DateTime.UtcNow&&(except==null||x.OrderId!=except)).SumAsync(x=>x.Quantity);
 public async Task<object> Receive(ReceiveInput r,Actor a) {
  Confirm(r.Confirmed);Safety.Positive(r.Quantity);Safety.Positive(r.ConversionFactor);Safety.Require(r.UnitCost>=0,"Cost cannot be negative.");Text(r.Reference,"Delivery reference");Text(r.Lot,"Lot");
  var med=await db.Medicines.SingleAsync(x=>x.Id==r.MedicineId);Safety.Require(med.ActiveStatus,"Inactive medicine.");
  Safety.Require(await db.InventoryLocations.AnyAsync(x=>x.Id==r.LocationId)&&await db.Suppliers.AnyAsync(x=>x.Id==r.SupplierId),"Select a valid supplier and location.");
  Safety.Require(!await db.GoodsReceipts.AnyAsync(x=>x.SupplierId==r.SupplierId&&x.Reference==r.Reference),"Duplicate delivery reference. Use a unique delivery-line reference for partial receipts.");
  if(r.PurchaseOrderId!=null){var po=await db.PurchaseOrders.SingleAsync(x=>x.Id==r.PurchaseOrderId);Safety.Require(po.SupplierId==r.SupplierId,"Purchase-order supplier mismatch.");}
  Safety.Require(r.Expiration>=Safety.Today,"Expired receiving requires a separately approved quarantine workflow.");
  var batch=await db.InventoryBatchs.SingleOrDefaultAsync(x=>x.MedicineId==r.MedicineId&&x.Lot==r.Lot&&x.Expiration==r.Expiration);
  if(batch==null){batch=new InventoryBatch{MedicineId=r.MedicineId,Lot=r.Lot,Expiration=r.Expiration,UnitCost=r.UnitCost};db.InventoryBatchs.Add(batch);}
  Safety.Require(!await db.RecallAffectedBatchs.AnyAsync(x=>x.BatchId==batch.Id),"Recalled batch cannot be received routinely.");
  var b=await db.InventoryBalances.SingleOrDefaultAsync(x=>x.BatchId==batch.Id&&x.LocationId==r.LocationId&&x.Status==StockStatus.Available);
  if(b==null){b=new InventoryBalance{Batch=batch,LocationId=r.LocationId,Status=StockStatus.Available};db.InventoryBalances.Add(b);}
  var quantity=r.Quantity*r.ConversionFactor;Safety.Positive(quantity);Movement(b,quantity,"Receiving",r.Reference,a.UserId);
  var movement=db.ChangeTracker.Entries<StockMovement>().Last().Entity;
  db.GoodsReceipts.Add(new GoodsReceipt{Reference=r.Reference,SupplierId=r.SupplierId,PurchaseOrderId=r.PurchaseOrderId,Movement=movement});return new{batch.Id,BalanceId=b.Id,Quantity=b.Quantity};
 }
 public async Task<object> RegisterBarcode(BarcodeInput r,Actor a){Confirm(r.Confirmed);var data=BarcodeIdentity.Registration(r.Barcode,r.Format);Safety.Positive(r.ConversionFactor);Safety.Require(await db.Medicines.AnyAsync(x=>x.Id==r.MedicineId&&x.ActiveStatus),"Medicine not active.");var code=data.Gtin??data.Raw;
  var equivalents=BarcodeIdentity.EquivalentCodes(data);Safety.Require(!await db.MedicineBarcodes.AnyAsync(x=>equivalents.Contains(x.Barcode)),"Barcode already mapped. Ambiguous mapping blocked.");var b=new MedicineBarcode{MedicineId=r.MedicineId,Barcode=code,Packaging=Text(r.Packaging,"Packaging"),ConversionFactor=r.ConversionFactor,ApprovedBy=a.UserId};db.MedicineBarcodes.Add(b);return b;
 }
 public async Task<object> CreateOrder(OrderInput r){Confirm(r.Confirmed);Safety.Positive(r.Quantity);Text(r.Reference,"Order reference");Text(r.EncounterReference,"Encounter reference");Safety.Require(await db.Medicines.AnyAsync(x=>x.Id==r.MedicineId&&x.ActiveStatus),"Medicine inactive.");var o=new DispenseOrder{Reference=r.Reference,EncounterReference=r.EncounterReference,MedicineId=r.MedicineId,Quantity=r.Quantity};db.DispenseOrders.Add(o);return o;}
 public async Task<object> Dispense(DispenseInput r,Actor a){
  Confirm(r.Confirmed);Safety.Require(Roles.Clinical.Contains(a.Role),"Pharmacist authorization required.");Safety.Require(r.PatientVerified,"Patient identity confirmation required.");Safety.Positive(r.Quantity);
  Safety.Require(r.VerificationMode is "ManualSelection" or "ManualBarcode" or "HidUnverified","Physical-device provenance is not available in this build; cannot attest a physical scan.");
  Safety.Require(await db.SystemSettings.AnyAsync(x=>x.Name=="ManualDispensingApproved"&&x.Value=="true"),"Hospital-approved manual dispensing policy is disabled.");Text(r.OverrideReason,"Manual verification reason / approval reference");
  var order=await db.DispenseOrders.SingleAsync(x=>x.Id==r.OrderId);Safety.Require(order.Active&&order.Dispensed+r.Quantity<=order.Quantity,"Order inactive or quantity exceeds remaining order.");
  var b=await Balance(r.BalanceId);Safety.CanDispense(b,r.Quantity,Safety.Today);
  var requestedMedicine=await db.Medicines.SingleAsync(m=>m.Id==order.MedicineId);var actualMedicine=await db.Medicines.SingleAsync(m=>m.Id==b.Batch.MedicineId);
  var substituted=b.Batch.MedicineId!=order.MedicineId;
  if(substituted){
   Safety.Require(r.ApproveBrandSubstitution,"A different brand requires explicit pharmacist substitution approval.");
   Safety.Require(MedicineGrouping.Definition(requestedMedicine)==MedicineGrouping.Definition(actualMedicine),"Substitution must match the recorded generic name, strength, dosage form, route and base unit. Dose conversion is not supported.");
   Text(r.SubstitutionApprovalReference,"Substitution authorization reference");Text(r.SubstitutionReason,"Substitution reason");
  }
  Safety.Require(actualMedicine.ActiveStatus,"Selected brand is inactive.");
  Safety.Require(await db.Medicines.AnyAsync(x=>x.Id==order.MedicineId&&x.ActiveStatus),"Medicine inactive.");
  Safety.Require(!await db.RecallAffectedBatchs.AnyAsync(x=>x.BatchId==b.BatchId),"Recalled batch blocked.");Safety.Require(b.Quantity-r.Quantity>=await Reserved(b.Id,order.Id),"Stock reserved for another order.");
  if(r.VerificationMode!="ManualSelection") {var resolved=await new BarcodeLookup(db).Resolve(r.Barcode);var data=resolved.Data;Safety.Require(resolved.Result.Status=="Matched"&&resolved.Medicine?.Id==b.Batch.MedicineId,"Barcode medicine/package mismatch or ambiguous mapping.");if(resolved.InternalStock!=null)Safety.Require(resolved.InternalStock.Id==b.Id,"Stock label belongs to a different stock balance.");if(data.Lot!=null)Safety.Require(data.Lot==b.Batch.Lot,"Barcode lot mismatch.");if(data.Expiration!=null)Safety.Require(data.Expiration==b.Batch.Expiration,"Barcode expiry mismatch.");}
  await new ClinicalSafetyService(db).Enforce(r,a,order,b.Batch.MedicineId);
  var transaction=new DispenseTransaction{SafetyAssessmentId=r.SafetyAssessmentId,Order=order,Reference=Guid.NewGuid().ToString(),Pharmacist=a.UserId,Verification=Enum.Parse<VerificationMode>(r.VerificationMode),OverrideReason=r.OverrideReason,BrandSubstituted=substituted,RequestedMedicineId=order.MedicineId,DispensedMedicineId=b.Batch.MedicineId,RequestedBrandName=requestedMedicine.BrandName,DispensedBrandName=actualMedicine.BrandName,SubstitutionApprovalReference=substituted?r.SubstitutionApprovalReference.Trim():"",SubstitutionReason=substituted?r.SubstitutionReason.Trim():""};
  db.DispenseTransactions.Add(transaction);db.DispenseTransactionLines.Add(new DispenseTransactionLine{Transaction=transaction,Balance=b,Quantity=r.Quantity});
  Movement(b,-r.Quantity,"Dispensing",transaction.Reference,a.UserId,substituted?$"Brand substitution: {transaction.SubstitutionApprovalReference}; {transaction.SubstitutionReason}. Verification: {r.OverrideReason}":r.OverrideReason);order.Dispensed+=r.Quantity;
  foreach(var reserve in await db.StockReservations.Where(x=>x.OrderId==order.Id&&(substituted||x.BalanceId==b.Id)&&!x.Released).ToListAsync())reserve.Released=true;
  db.IntegrationOutboxs.Add(new IntegrationOutbox{Transaction=transaction});return new{transaction.Id,transaction.Reference,Remaining=order.Quantity-order.Dispensed,transaction.BrandSubstituted,transaction.RequestedMedicineId,transaction.DispensedMedicineId};
 }
 public async Task<object> Transfer(MoveInput r,Actor a){Confirm(r.Confirmed);Text(r.Reference,"Reference");Text(r.Reason,"Reason");Safety.Positive(r.Quantity);Safety.Require(!await db.StockMovements.AnyAsync(x=>x.Reference==r.Reference&&(x.Kind=="TransferIn"||x.Kind=="TransferOut")),"Duplicate transfer reference.");var source=await Balance(r.BalanceId);Safety.Require(source.LocationId!=r.DestinationId,"Choose a different destination.");Safety.Require(source.Quantity-r.Quantity>=await Reserved(source.Id),"Insufficient unreserved stock.");var target=await Target(source,r.DestinationId,source.Status);Movement(source,-r.Quantity,"TransferOut",r.Reference,a.UserId,r.Reason);Movement(target,r.Quantity,"TransferIn",r.Reference,a.UserId,r.Reason);return new{source.Id,DestinationBalance=target.Id};}
 public async Task<object> Count(CountInput r,Actor a){Confirm(r.Confirmed);Text(r.Reference,"Reference");Text(r.Reason,"Adjustment reason");Safety.Require(r.CountedQuantity>=0&&r.CountedQuantity<=100000000&&decimal.Round(r.CountedQuantity,4)==r.CountedQuantity,"Invalid counted quantity.");var b=await Balance(r.BalanceId);Safety.Require(r.CountedQuantity>=await Reserved(b.Id),"Cannot adjust below reserved stock.");var delta=r.CountedQuantity-b.Quantity;Movement(b,delta,"CountAdjustment",r.Reference,a.UserId,r.Reason);db.StockAdjustments.Add(new StockAdjustment{Movement=db.ChangeTracker.Entries<StockMovement>().Last().Entity,Counted=r.CountedQuantity,Reason=r.Reason});return new{b.Id,b.Quantity,Delta=delta};}
 public async Task<object> Return(ReturnInput r,Actor a){Confirm(r.Confirmed);Safety.Require(Roles.Clinical.Contains(a.Role),"Pharmacist authorization required for patient returns.");var returnReference=Text(r.Reference,"Return reference");Safety.Require(r.HospitalSourceVerified,"Verify hospital issue evidence, or use outside/uncertain intake without stock credit.");Text(r.HospitalEvidenceReference,"Hospital issue / receipt evidence reference");Safety.Require(!await db.Returns.AnyAsync(x=>x.Reference==returnReference),"Duplicate return reference. Use one unique reference per returned medicine line.");
  if(r.DischargeReturn){Text(r.DischargeReference,"Discharge reference");Safety.Require(r.UnusedVerified,"Verify unused items against the original dispensing record.");Safety.Require(r.PackageCondition is "Sealed/intact" or "Opened" or "Damaged" or "Unknown","Select the returned package condition.");}
  Text(r.Reason,"Return reason");Safety.Positive(r.Quantity);var line=await db.DispenseTransactionLines.SingleAsync(x=>x.Id==r.DispenseLineId);var returned=await db.Returns.Where(x=>x.DispenseLineId==line.Id).SumAsync(x=>x.Quantity);Safety.Require(returned+r.Quantity<=line.Quantity,"Return exceeds dispensed quantity.");var source=await Balance(line.BalanceId);var quarantine=await Target(source,source.LocationId,StockStatus.Quarantined);Movement(quarantine,r.Quantity,"ReturnQuarantine",returnReference,a.UserId,r.Reason);var ret=new Return{DispenseLineId=line.Id,Quantity=r.Quantity,Reason=r.Reason,Reference=returnReference,HospitalSourceVerified=true,HospitalEvidenceReference=r.HospitalEvidenceReference.Trim(),DischargeReturn=r.DischargeReturn,DischargeReference=r.DischargeReturn?r.DischargeReference.Trim():"",PackageCondition=r.DischargeReturn?r.PackageCondition:"",UnusedVerified=r.DischargeReturn&&r.UnusedVerified,MovementId=db.ChangeTracker.Entries<StockMovement>().Last().Entity.Id};db.Returns.Add(ret);return new{ret.Id,QuarantineBalanceId=quarantine.Id,RemainingReturnable=line.Quantity-returned-r.Quantity,Status="Quarantined; requires approved quality review before any release"};}
 public async Task<object> Recall(RecallInput r){Confirm(r.Confirmed);Text(r.Reference,"Recall reference");Text(r.Reason,"Recall reason");Safety.Require(await db.InventoryBatchs.AnyAsync(x=>x.Id==r.BatchId),"Unknown batch.");var recall=new RecallNotice{Reference=r.Reference,Reason=r.Reason};db.RecallNotices.Add(recall);db.RecallAffectedBatchs.Add(new RecallAffectedBatch{Recall=recall,BatchId=r.BatchId});return new{recall.Id,Status="Blocked by server recall check; balances preserved for traceability"};}
 public async Task<object> Reserve(ReserveInput r){Confirm(r.Confirmed);Safety.Positive(r.Quantity);var b=await Balance(r.BalanceId);Safety.CanDispense(b,r.Quantity,Safety.Today);var order=await db.DispenseOrders.SingleAsync(x=>x.Id==r.OrderId);Safety.Require(order.Active&&order.MedicineId==b.Batch.MedicineId&&r.Quantity<=order.Quantity-order.Dispensed,"Order mismatch.");Safety.Require(!await db.RecallAffectedBatchs.AnyAsync(x=>x.BatchId==b.BatchId),"Recalled batch.");Safety.Require(b.Quantity-r.Quantity>=await Reserved(b.Id),"Insufficient unreserved stock.");Safety.Require(!await db.StockReservations.AnyAsync(x=>x.OrderId==order.Id&&!x.Released&&x.ExpiresAt>DateTime.UtcNow),"Order already reserved.");var reservation=new StockReservation{BalanceId=b.Id,OrderId=order.Id,Quantity=r.Quantity,ExpiresAt=DateTime.UtcNow.AddMinutes(15)};db.StockReservations.Add(reservation);return reservation;}
 public async Task<object> Reconcile(Actor a){var run=new ReconciliationRun{Reference=Guid.NewGuid().ToString(),Actor=a.UserId};db.ReconciliationRuns.Add(run);foreach(var b in await db.InventoryBalances.ToListAsync()){var expected=await db.StockMovements.Where(x=>x.BalanceId==b.Id).SumAsync(x=>x.Delta);if(expected!=b.Quantity)db.ReconciliationDiscrepancys.Add(new ReconciliationDiscrepancy{Run=run,Balance=b,Expected=expected,Actual=b.Quantity});}return new{run.Id,run.Reference};}
}
