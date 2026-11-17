using System.Text.Json;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    string dischargeEncounter="";
    async Task FindDischargeMedicines()
    {
        var dialog=new FormWindow("Find dispensed medicines for a discharged patient",[new("encounter","Exact patient / encounter reference from the original order")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        dischargeEncounter=dialog.Values["encounter"]?.ToString()?.Trim()??"";
        await vm.Load("returns/discharge-lines?encounter="+Uri.EscapeDataString(dischargeEncounter));
        vm.Status="Check the exact encounter and dispensed brand. Select a medicine line, then choose Hospital discharge return. Returned items go to quarantine for quality review.";
    }
    async Task DischargeReturn()
    {
        if(dischargeEncounter.Length==0)throw new InvalidOperationException("Choose Find discharge medicines and verify the patient / encounter first.");
        var id=SelectedId();var rows=await vm.Get("returns/discharge-lines?encounter="+Uri.EscapeDataString(dischargeEncounter));
        var matches=rows.EnumerateArray().Where(x=>x.GetProperty("id").GetString()==id).ToArray();
        if(matches.Length!=1)throw new InvalidOperationException("Select an original dispensing line from Find discharge medicines.");
        var row=matches[0];var remaining=row.GetProperty("returnableQuantity").GetDecimal();
        if(remaining<=0)throw new InvalidOperationException("This dispensing line has already been fully returned.");
        var title="Discharge return: "+row.GetProperty("medicine")+" / "+row.GetProperty("brand")+" / "+row.GetProperty("lot")+" | "+dischargeEncounter+" | Up to "+remaining+" "+row.GetProperty("baseUnit");
        var dialog=new FormWindow(title,[new("quantity","Unused base units physically returned","number","1"),new("reference","Unique return-line reference"),new("dischargeReference","Discharge / ward-return document reference"),new("packageCondition","Package condition","choice","Sealed/intact|Opened|Damaged|Unknown"),new("unusedVerified","I verified unused items against this encounter and original dispensing record","bool"),new("hospitalSourceVerified","I verified these exact units were issued by the hospital; outside or inseparable mixed items use separate intake","bool"),new("hospitalEvidenceReference","Hospital issue / receipt evidence reference"),new("reason","Return reason / quality-review notes")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        await vm.Post("returns",new ReturnInput(Guid.Parse(id),Convert.ToDecimal(dialog.Values["quantity"]),dialog.Values["reference"]?.ToString()??"",dialog.Values["reason"]?.ToString()??"",true,true,dialog.Values["dischargeReference"]?.ToString()??"",dialog.Values["packageCondition"]?.ToString()??"",dialog.Values["unusedVerified"] is true,dialog.Values["hospitalSourceVerified"] is true,dialog.Values["hospitalEvidenceReference"]?.ToString()??""));
        await vm.Load("returns/discharge-lines?encounter="+Uri.EscapeDataString(dischargeEncounter));
        vm.Status="Discharge return recorded in quarantine. Available stock and the patient's bill were not automatically credited; follow the hospital's review and billing procedures.";
    }
}
