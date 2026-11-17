namespace DDHI.Pharmacy.Contracts;
public sealed record ExternalReturnInput(string Reference,string EncounterReference,string Source,string ItemDescription,decimal Quantity,string Unit,string PackageCondition,string Lot,DateOnly? Expiration,string Barcode,string ReceiptReference,string CustodyDisposition,string Notes,bool NoHospitalCreditAcknowledged,bool Confirmed);
