using System.Text.Json;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    async Task OpenClinicalReference(Guid id){Navigation.SelectionChanged-=Navigate;try{Navigation.SelectedItem="Drug Reference";current="Drug Reference";}finally{Navigation.SelectionChanged+=Navigate;}await LoadPage();await DrugReferencePanel.OpenMedicine(id);}
    async Task<bool> ReviewDispensingSafety(Dictionary<string,object?> values)
    {
        var orderId=Guid.Parse(values["orderId"]!.ToString()!);var balanceId=Guid.Parse(values["balanceId"]!.ToString()!);var quantity=Convert.ToDecimal(values["quantity"]);
        var orders=await vm.Get("orders");var order=orders.EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==orderId);var inventory=await vm.Get("inventory");var balance=inventory.EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==balanceId);
        var actual=balance.GetProperty("medicineId").GetGuid();var list=DrugReferencePanel.Basket.ToDictionary(x=>x.Id,x=>x.Name);list[actual]=balance.GetProperty("medicine").ToString()+" / "+balance.GetProperty("brandName");
        var context=new ClinicalContextWindow(list.Select(x=>(x.Key,x.Value)).ToArray(),encounter:order.GetProperty("encounterReference").ToString(),order:orderId,balance:balanceId,issueQuantity:quantity){Owner=this};
        if(context.ShowDialog()!=true||context.Assessment==null)return false;
        var screen=await vm.Post("clinical/screen",context.Assessment);var id=screen.GetProperty("Id").GetGuid();var result=screen.GetProperty("Result").Deserialize<ClinicalScreenResult>()!;
        var review=new SafetyReviewWindow(id,result){Owner=this};if(review.ShowDialog()!=true||review.Decision==null)return false;
        await vm.Post("clinical/interventions",review.Decision);if(review.Decision.Decision!="Proceed with documented review"){vm.Status="Dispensing held/escalated. No stock movement was committed.";return false;}
        values["safetyAssessmentId"]=id;return true;
    }
}
