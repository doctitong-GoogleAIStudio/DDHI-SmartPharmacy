using System.Data;
using System.Windows;
using System.Text.Json;
using System.Windows.Controls;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    void CatalogMode(bool grouped)
    {
        foreach(var button in Toolbar.Children.OfType<Button>())if(button.Content?.ToString() is "Set active status" or "Edit importer / distributor")button.IsEnabled=!grouped;
    }
    string SelectedGenericTemplate()
    {
        if(Table.SelectedItem is DataRowView row)foreach(DataColumn column in row.DataView.Table!.Columns)if(column.ColumnName.Equals("templateMedicineId",StringComparison.OrdinalIgnoreCase)||column.ColumnName.Equals("id",StringComparison.OrdinalIgnoreCase))return row[column.ColumnName].ToString()!;
        throw new InvalidOperationException("Select a generic group or an individual brand product first.");
    }
    async Task BrowseGenerics(){await vm.Load("medicines/generics");CatalogMode(true);vm.Status="Generic groups: select one and choose View brands or Add brand to selected generic. Product status and company edits require an individual brand row.";}
    async Task ViewBrands(){await vm.Load("medicines/"+SelectedGenericTemplate()+"/brands");CatalogMode(false);vm.Status="Brand products in the selected generic group. Stock, barcodes and orders remain separate for each product.";}
    async Task AllBrandProducts(){await vm.Load("medicines");CatalogMode(false);}
    async Task AddGenericBrand()
    {
        var template=SelectedGenericTemplate();var medicine=await vm.Get("medicines/"+template);
        string Value(string key)=>medicine.GetProperty(key).ToString();
        var title="Add brand: "+Value("genericName")+" / "+Value("strength")+" / "+Value("dosageForm")+" / "+Value("route")+" / "+Value("baseUnit");
        var dialog=new FormWindow(title,[new("brandName","Verified brand name (or Unbranded)"),new("manufacturer","Manufacturer (optional)"),new("importedBy","Imported by (optional)"),new("distributedBy","Distributed by (optional)"),new("reorderLevel","Reorder level in base units","number","0"),new("reason","Catalog approval reason")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        await vm.Post("medicines/"+template+"/brands",new BrandInput(dialog.Values["brandName"]?.ToString()??"",dialog.Values["manufacturer"]?.ToString()??"",dialog.Values["importedBy"]?.ToString()??"",dialog.Values["distributedBy"]?.ToString()??"",Convert.ToDecimal(dialog.Values["reorderLevel"]),dialog.Values["reason"]?.ToString()??"",true));
        await vm.Load("medicines/"+template+"/brands");CatalogMode(false);vm.Status="Brand added to the generic group. Register its exact package barcode, then receive its stock separately.";
    }
    async void OpenManual(object sender,RoutedEventArgs e)=>await Guard(OpenUserManual);
    Task OpenUserManual()
    {
        var path=System.IO.Path.Combine(AppContext.BaseDirectory,"Documentation","DDHI-SmartPharmacy-Complete-User-Manual.pdf");
        if(!System.IO.File.Exists(path))throw new InvalidOperationException("Install the complete updated client package to open the user manual.");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path){UseShellExecute=true});return Task.CompletedTask;
    }
}
