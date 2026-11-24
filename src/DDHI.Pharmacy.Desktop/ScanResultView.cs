using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DDHI.Pharmacy.Contracts;

namespace DDHI.Pharmacy.Desktop;
public sealed class ScanResultView : UserControl
{
    readonly TextBox input=new(){FontSize=18};
    readonly Button lookup=new(){Content="Look up barcode"};
    readonly Button copy=new(){Content="Copy product code",IsEnabled=false};
    readonly TextBlock state=new(){FontSize=22,FontWeight=FontWeights.SemiBold};
    readonly TextBlock message=new(){TextWrapping=TextWrapping.Wrap};
    readonly StackPanel fields=new();
    readonly TextBlock warning=new(){TextWrapping=TextWrapping.Wrap,Foreground=Brushes.DarkRed};
    readonly DataGrid stock=new(){IsReadOnly=true,CanUserAddRows=false,AutoGenerateColumns=false,Height=220,HeadersVisibility=DataGridHeadersVisibility.Column};
    ScanLookupResult? result;
    public Guid? SelectedMedicineId=>result?.Medicine?.Id;
    public Func<string,Task>? LookupRequested { get; set; }
    public ScanResultView()
    {
        var root=new StackPanel{Margin=new Thickness(0,0,12,12)};
        root.Children.Add(new TextBlock{Text="Scan a package or type its barcode, then press Enter.",FontSize=17,Margin=new Thickness(0,0,0,8)});
        var entry=new DockPanel();DockPanel.SetDock(lookup,Dock.Right);entry.Children.Add(lookup);entry.Children.Add(input);root.Children.Add(entry);
        root.Children.Add(new Border{Background=(Brush)new BrushConverter().ConvertFromString("#E0F2E7")!,Padding=new Thickness(16),Margin=new Thickness(0,14,0,8),Child=new StackPanel{Children={state,message}}});
        root.Children.Add(copy);root.Children.Add(fields);root.Children.Add(warning);
        root.Children.Add(new TextBlock{Text="Recorded stock — quantities are base units, not package counts",FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,18,0,8)});
        foreach(var (header,path) in new[]{("Location","Location"),("Batch / lot","Lot"),("Expiration","Expiration"),("Status","Status"),("Quantity","Quantity"),("Unit","BaseUnit"),("Recalled","Recalled"),("Expired","Expired")})
            stock.Columns.Add(new DataGridTextColumn{Header=header,Binding=new System.Windows.Data.Binding(path)});
        root.Children.Add(stock);
        root.Children.Add(new TextBlock{Text="Results come from your registered hospital catalog and current server stock. A scan does not receive, reserve or dispense medicine. Verify the physical medicine, strength, dosage form and package.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,0)});
        Content=new ScrollViewer{Content=root,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        lookup.Click+=async(_,_)=>await Submit();input.KeyDown+=async(_,e)=>{if(e.Key==Key.Enter){e.Handled=true;await Submit();}};
        copy.Click+=(_,_)=>{if(result!=null)Clipboard.SetText(result.Gtin??result.Raw);};
        Reset();
    }
    async Task Submit()
    {
        if(!lookup.IsEnabled||LookupRequested==null)return;
        var raw=input.Text;lookup.IsEnabled=false;input.IsEnabled=false;
        Clear("Looking up barcode...","Checking the registered medicine and package on the central server.");
        try {await LookupRequested(raw);}
        catch(Exception ex){Clear("Lookup unavailable",ex.Message);}
        finally{lookup.IsEnabled=true;input.IsEnabled=true;input.Focus();input.SelectAll();}
    }
    public void FocusInput(){input.Focus();input.SelectAll();}
    public void Reset()=>Clear("Ready to scan","No medicine selected yet.");
    public void Clear(string title,string detail){result=null;copy.IsEnabled=false;state.Text=title;message.Text=detail;fields.Children.Clear();stock.ItemsSource=null;warning.Text="";}
    public void ShowResult(ScanLookupResult value)
    {
        result=value;copy.IsEnabled=true;
        state.Text=value.Status switch{"Matched"=>"Registered medicine found","NotRegistered"=>"Barcode is not registered","Ambiguous"=>"Conflicting barcode mappings",_=>"No matching stock record"};
        message.Text=value.Message;fields.Children.Clear();
        var barcode=new UniformGrid{Columns=2,Margin=new Thickness(0,10,0,8)};
        Add(barcode,"Product code / GTIN",value.Gtin??value.Raw.Replace("\u001d"," ␝ "));Add(barcode,"Barcode type",value.Symbology);
        Add(barcode,"Batch encoded on package",value.EncodedLot??"Not encoded");Add(barcode,"Expiration encoded on package",value.EncodedExpiration?.ToString("yyyy-MM-dd")??"Not encoded");
        Add(barcode,"Serial number",value.Serial??"Not encoded");fields.Children.Add(barcode);
        if(value.Medicine is { } med){
            fields.Children.Add(new TextBlock{Text=med.GenericName,FontSize=26,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8)});
            var grid=new UniformGrid{Columns=3};Add(grid,"Brand",Blank(med.BrandName));Add(grid,"Strength",med.Strength);Add(grid,"Dosage form",med.DosageForm);Add(grid,"Route",Blank(med.Route));Add(grid,"Manufacturer",Blank(med.Manufacturer));Add(grid,"Imported by",Blank(med.ImportedBy));Add(grid,"Distributed by",Blank(med.DistributedBy));Add(grid,"Catalog status",med.Active?"Active":"Inactive");
            if(value.Package is { } pack)Add(grid,"Registered package",$"{pack.Packaging} = {pack.BaseUnitsPerPackage:0.####} {pack.BaseUnit}");fields.Children.Add(grid);
        }
        warning.Text=string.Join("\n",value.Warnings);
        stock.ItemsSource=value.Stock;
        if(value.Medicine!=null&&value.Stock.Count==0)warning.Text+=(warning.Text.Length>0?"\n":"")+"No recorded stock balances match this lookup. Do not infer batch or quantity from a product-only barcode.";
    }
    static string Blank(string value)=>string.IsNullOrWhiteSpace(value)?"Not recorded":value;
    static void Add(Panel panel,string label,string value)
    {
        var group=new StackPanel{Margin=new Thickness(0,3,18,8)};
        group.Children.Add(new TextBlock{Text=label,FontSize=12,Foreground=Brushes.DimGray});
        group.Children.Add(new TextBlock{Text=value,FontSize=16,TextWrapping=TextWrapping.Wrap});panel.Children.Add(group);
    }
}
