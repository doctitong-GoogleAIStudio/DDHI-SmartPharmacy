namespace DDHI.Pharmacy.Domain;
public enum StockStatus { Available, Reserved, Quarantined, Recalled, Expired, Damaged, PendingVerification }
public enum VerificationMode { ManualSelection, ManualBarcode, HidUnverified, PhysicalDevice }
public enum PostingState { Draft, Validated, Confirmed, Queued, Posting, Acknowledged, Rejected, NeedsReconciliation, Reversed }
public abstract class Entity { public Guid Id { get; set; } = Guid.NewGuid(); public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class Medicine : Entity {
 public string? CerebroMedicineId {get;set;} public string GenericName {get;set;}=""; public string BrandName {get;set;}="";
 public string Strength {get;set;}=""; public string DosageForm {get;set;}=""; public string Route {get;set;}="";
 public string Manufacturer {get;set;}=""; public string ImportedBy {get;set;}=""; public string DistributedBy {get;set;}=""; public string Category {get;set;}=""; public string BaseUnit {get;set;}="unit";
 public string PackagingUnit {get;set;}="unit"; public decimal ConversionFactor {get;set;}=1; public decimal ReorderLevel {get;set;}
 public bool ActiveStatus {get;set;}=true; public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
}
public class MedicineBarcode : Entity { public Guid MedicineId {get;set;} public Medicine Medicine {get;set;}=null!; public string Barcode {get;set;}=""; public string Packaging {get;set;}="unit"; public decimal ConversionFactor {get;set;}=1; public string ApprovedBy {get;set;}=""; }
public class PackagingUnit : Entity { public Guid MedicineId {get;set;} public Medicine Medicine {get;set;}=null!; public string Name {get;set;}=""; public decimal Factor {get;set;}=1; }
public class Supplier : Entity { public string Name {get;set;}=""; }
public class PurchaseOrder : Entity { public Guid SupplierId {get;set;} public Supplier Supplier {get;set;}=null!; public string Reference {get;set;}=""; public decimal OrderedQuantity {get;set;} }
public class InventoryLocation : Entity { public string Name {get;set;}=""; }
public class InventoryBatch : Entity { public Guid MedicineId {get;set;} public Medicine Medicine {get;set;}=null!; public string Lot {get;set;}=""; public DateOnly Expiration {get;set;} public decimal UnitCost {get;set;} }
public class InventoryBalance : Entity { public Guid BatchId {get;set;} public InventoryBatch Batch {get;set;}=null!; public Guid LocationId {get;set;} public InventoryLocation Location {get;set;}=null!; public StockStatus Status {get;set;} public decimal Quantity {get;set;} }
public class StockMovement : Entity { public Guid? ReversesMovementId {get;set;} public Guid BalanceId {get;set;} public InventoryBalance Balance {get;set;}=null!; public decimal Delta {get;set;} public string Kind {get;set;}=""; public string Reference {get;set;}=""; public string Actor {get;set;}=""; public string Reason {get;set;}=""; }
public class GoodsReceipt : Entity { public string Reference {get;set;}=""; public Guid SupplierId {get;set;} public Supplier Supplier {get;set;}=null!; public Guid? PurchaseOrderId {get;set;} public PurchaseOrder? PurchaseOrder {get;set;} public Guid MovementId {get;set;} public StockMovement Movement {get;set;}=null!; }
public class StockReservation : Entity { public Guid BalanceId {get;set;} public InventoryBalance Balance {get;set;}=null!; public Guid OrderId {get;set;} public DispenseOrder Order {get;set;}=null!; public decimal Quantity {get;set;} public DateTime ExpiresAt {get;set;} public bool Released {get;set;} }
public class DispenseOrder : Entity { public string Reference {get;set;}=""; public string EncounterReference {get;set;}=""; public Guid MedicineId {get;set;} public Medicine Medicine {get;set;}=null!; public decimal Quantity {get;set;} public decimal Dispensed {get;set;} public bool Active {get;set;}=true; }
public class DispenseTransaction : Entity { public Guid OrderId {get;set;} public DispenseOrder Order {get;set;}=null!; public string Reference {get;set;}=""; public VerificationMode Verification {get;set;} public string Pharmacist {get;set;}=""; public string OverrideReason {get;set;}=""; public PostingState State {get;set;}=PostingState.Confirmed; public bool BrandSubstituted {get;set;} public Guid? RequestedMedicineId {get;set;} public Guid? DispensedMedicineId {get;set;} public string RequestedBrandName {get;set;}=""; public string DispensedBrandName {get;set;}=""; public string SubstitutionApprovalReference {get;set;}=""; public string SubstitutionReason {get;set;}=""; }
public class DispenseTransactionLine : Entity { public Guid TransactionId {get;set;} public DispenseTransaction Transaction {get;set;}=null!; public Guid BalanceId {get;set;} public InventoryBalance Balance {get;set;}=null!; public decimal Quantity {get;set;} }
public class Return : Entity { public string Reference {get;set;}=""; public bool DischargeReturn {get;set;} public string DischargeReference {get;set;}=""; public string PackageCondition {get;set;}=""; public bool UnusedVerified {get;set;} public bool HospitalSourceVerified {get;set;} public string HospitalEvidenceReference {get;set;}=""; public Guid? MovementId {get;set;} public Guid DispenseLineId {get;set;} public DispenseTransactionLine DispenseLine {get;set;}=null!; public decimal Quantity {get;set;} public string Reason {get;set;}=""; }
public class StockAdjustment : Entity { public Guid MovementId {get;set;} public StockMovement Movement {get;set;}=null!; public decimal Counted {get;set;} public string Reason {get;set;}=""; }
public class RecallNotice : Entity { public string Reference {get;set;}=""; public string Reason {get;set;}=""; public bool Closed {get;set;} }
public class RecallAffectedBatch : Entity { public Guid RecallId {get;set;} public RecallNotice Recall {get;set;}=null!; public Guid BatchId {get;set;} public InventoryBatch Batch {get;set;}=null!; }
public class CerebroMedicineMapping : Entity { public Guid MedicineId {get;set;} public Medicine Medicine {get;set;}=null!; public string ExternalId {get;set;}=""; public bool Approved {get;set;} }
public class CerebroOrderMapping : Entity { public Guid OrderId {get;set;} public DispenseOrder Order {get;set;}=null!; public string ExternalId {get;set;}=""; }
public class IntegrationOutbox : Entity { public Guid TransactionId {get;set;} public DispenseTransaction Transaction {get;set;}=null!; public PostingState State {get;set;}=PostingState.Queued; public string Detail {get;set;}="Mock only; live posting disabled"; }
public class IntegrationInbox : Entity { public string ExternalId {get;set;}=""; public string PayloadHash {get;set;}=""; }
public class IntegrationAcknowledgment : Entity { public Guid OutboxId {get;set;} public IntegrationOutbox Outbox {get;set;}=null!; public string Reference {get;set;}=""; public string Actor {get;set;}=""; }
public class ReconciliationRun : Entity { public string Reference {get;set;}=""; public string Actor {get;set;}=""; }
public class ReconciliationDiscrepancy : Entity { public Guid RunId {get;set;} public ReconciliationRun Run {get;set;}=null!; public Guid BalanceId {get;set;} public InventoryBalance Balance {get;set;}=null!; public decimal Expected {get;set;} public decimal Actual {get;set;} }
public class Workstation : Entity { public string Name {get;set;}=""; public DateTime LastSeen {get;set;}=DateTime.UtcNow; }
public class ScannerConfiguration : Entity { public Guid WorkstationId {get;set;} public Workstation Workstation {get;set;}=null!; public string Name {get;set;}=""; public bool Enabled {get;set;} public string Type {get;set;}="HID"; public string Port {get;set;}=""; public string Suffix {get;set;}="Enter"; public DateTime? LastSuccessfulScan {get;set;} }
public class ScannerEvent : Entity { public Guid WorkstationId {get;set;} public Workstation Workstation {get;set;}=null!; public string Mode {get;set;}="ManualBarcode"; public string Symbology {get;set;}="Unknown"; public bool Success {get;set;} }
public class Permission : Entity { public string Role {get;set;}=""; public string Name {get;set;}=""; }
public class UserSession : Entity { public string UserId {get;set;}=""; public Guid WorkstationId {get;set;} public Workstation Workstation {get;set;}=null!; public DateTime ExpiresAt {get;set;} }
public class AuditEvent : Entity { public string Actor {get;set;}=""; public string Role {get;set;}=""; public string Workstation {get;set;}=""; public string Action {get;set;}=""; public string RecordId {get;set;}=""; public string Detail {get;set;}=""; public string PreviousHash {get;set;}=""; public string Hash {get;set;}=""; }
public class SystemSetting : Entity { public string Name {get;set;}=""; public string Value {get;set;}=""; }
public class OperationKey : Entity { public string Key {get;set;}=""; public string Actor {get;set;}=""; public string PayloadHash {get;set;}=""; public string Result {get;set;}=""; }
public static class Safety {
 public static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow,"Asia/Manila"));
 public static void Positive(decimal q) { if(q<=0 || q>100000000 || decimal.Round(q,4)!=q) throw new InvalidOperationException("Quantity must be positive, within limits, and at most four decimal places."); }
 public static void CanDispense(InventoryBalance b, decimal quantity, DateOnly today) { Positive(quantity); if(b.Status!=StockStatus.Available || b.Batch.Expiration<today || quantity>b.Quantity) throw new InvalidOperationException("Unavailable, expired, recalled, quarantined, or insufficient stock."); }
 public static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
}
