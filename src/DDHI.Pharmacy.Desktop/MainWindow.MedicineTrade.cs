using System.Text.Json;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    async Task EditMedicineTrade()
    {
        var id=SelectedId();var medicine=await vm.Get("medicines/"+id);
        string Value(string key)=>medicine.TryGetProperty(key,out var value)?value.GetString()??"":"";
        var dialog=new FormWindow("Edit importer / distributor — "+Value("genericName"),[
            new("importedBy","Imported by (optional; clear to remove)","text",Value("importedBy")),
            new("distributedBy","Distributed by (optional; clear to remove)","text",Value("distributedBy")),
            new("reason","Reason for catalog change")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        await vm.Post("medicines/"+id+"/trade",new MedicineTradeInput(dialog.Values["importedBy"]?.ToString()??"",dialog.Values["distributedBy"]?.ToString()??"",dialog.Values["reason"]?.ToString()??"",true));
        await LoadPage();vm.Status="Importer and distributor updated in the medicine catalog.";
    }
}
