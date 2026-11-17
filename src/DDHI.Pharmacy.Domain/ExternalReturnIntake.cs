namespace DDHI.Pharmacy.Domain;
// Custody documentation only. This entity never creates a stock balance or a billing credit.
public sealed class ExternalReturnIntake : Entity
{
    public string Reference {get;set;}=""; public string EncounterReference {get;set;}="";
    public string Source {get;set;}="Uncertain/mixed"; public string ItemDescription {get;set;}="";
    public decimal Quantity {get;set;} public string Unit {get;set;}=""; public string PackageCondition {get;set;}="Unknown";
    public string Lot {get;set;}=""; public DateOnly? Expiration {get;set;} public string Barcode {get;set;}="";
    public string ReceiptReference {get;set;}=""; public string CustodyDisposition {get;set;}="Held separately pending review";
    public string Notes {get;set;}=""; public string RecordedBy {get;set;}="";
}
