using System.Globalization;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    async Task ExternalPatientIntake()
    {
        var dialog=new FormWindow("Outside / uncertain-source medicines - documentation only; no hospital stock or refund credit",[
            new("encounterReference","Patient / encounter reference","text",dischargeEncounter),new("reference","Unique intake-line reference"),
            new("source","Source","choice","Uncertain/mixed|Outside pharmacy"),new("itemDescription","Generic, brand, strength and dosage form from the package"),
            new("quantity","Quantity physically presented","number","1"),new("unit","Unit, e.g. tablet or bottle"),new("packageCondition","Package condition","choice","Unknown|Sealed/intact|Opened|Damaged"),
            new("lot","Batch / lot (optional)"),new("expiration","Expiry YYYY-MM-DD (optional; leave blank if unknown)"),new("barcode","Barcode (optional; does not prove hospital origin)"),
            new("receiptReference","Outside receipt / source document reference (optional)"),new("custodyDisposition","Custody / disposition under hospital policy","choice","Held separately pending review|Returned to patient/family|Disposal referred for authorization"),
            new("notes","Source assessment and custody notes"),new("noHospitalCreditAcknowledged","I acknowledge this intake does not add hospital stock or create a hospital refund credit","bool")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        string Value(string key)=>dialog.Values[key]?.ToString()??"";
        DateOnly? expiry=null;if(!string.IsNullOrWhiteSpace(Value("expiration"))){if(!DateOnly.TryParseExact(Value("expiration"),"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsed))throw new InvalidOperationException("Enter expiry as YYYY-MM-DD or leave it blank if unknown.");expiry=parsed;}
        await vm.Post("returns/external-intakes",new ExternalReturnInput(Value("reference"),Value("encounterReference"),Value("source"),Value("itemDescription"),Convert.ToDecimal(dialog.Values["quantity"]),Value("unit"),Value("packageCondition"),Value("lot"),expiry,Value("barcode"),Value("receiptReference"),Value("custodyDisposition"),Value("notes"),dialog.Values["noHospitalCreditAcknowledged"] is true,true));
        await vm.Load("returns/external-intakes");vm.Status="Outside/uncertain intake documented. No hospital stock balance or refund credit was created. Keep any held items separate from hospital stock.";
    }
}
