namespace DDHI.Pharmacy.Contracts;
public record MedicineInput(string GenericName,string BrandName,string Strength,string DosageForm,string Route,string Manufacturer,string Category,string BaseUnit,decimal ReorderLevel,string ImportedBy="",string DistributedBy="");
public record BarcodeInput(Guid MedicineId,string Barcode,string Packaging,decimal ConversionFactor,bool Confirmed,string Format="Auto");
public record ReceiveInput(Guid MedicineId,Guid LocationId,Guid SupplierId,string Reference,string Lot,DateOnly Expiration,decimal Quantity,decimal ConversionFactor,decimal UnitCost,bool Confirmed,Guid? PurchaseOrderId=null);
public record OrderInput(string Reference,string EncounterReference,Guid MedicineId,decimal Quantity,bool Confirmed);
public record DispenseInput(Guid OrderId,Guid BalanceId,decimal Quantity,string Barcode,string VerificationMode,bool PatientVerified,bool Confirmed,string OverrideReason,bool ApproveBrandSubstitution=false,string SubstitutionApprovalReference="",string SubstitutionReason="",Guid? SafetyAssessmentId=null);
public record MoveInput(Guid BalanceId,Guid DestinationId,decimal Quantity,string Reference,string Reason,bool Confirmed);
public record CountInput(Guid BalanceId,decimal CountedQuantity,string Reference,string Reason,bool Confirmed);
public record ReturnInput(Guid DispenseLineId,decimal Quantity,string Reference,string Reason,bool Confirmed,bool DischargeReturn=false,string DischargeReference="",string PackageCondition="",bool UnusedVerified=false,bool HospitalSourceVerified=false,string HospitalEvidenceReference="");
public record RecallInput(Guid BatchId,string Reference,string Reason,bool Confirmed);
public record NamedInput(string Name);
public record WorkstationInput(string Name);
public record ScannerInput(Guid WorkstationId,string Name,bool Enabled,string Type,string Port,string Suffix);
public record UserInput(string Email,string Password,string Role);
public record SettingInput(string Name,string Value,string ApprovalReference);
public record AcknowledgeInput(string Reference,bool Confirmed);
public record ReserveInput(Guid BalanceId,Guid OrderId,decimal Quantity,bool Confirmed);
public record PurchaseInput(Guid SupplierId,string Reference,decimal OrderedQuantity);

public record StockStateInput(Guid BalanceId,string TargetStatus,decimal Quantity,string Reference,string ApprovalReference,bool Confirmed);
public record DisposeInput(Guid BalanceId,decimal Quantity,string Reference,string Reason,bool Confirmed);
public record ReasonInput(string Reason,bool Confirmed);

public record MedicineTradeInput(string ImportedBy,string DistributedBy,string Reason,bool Confirmed);
