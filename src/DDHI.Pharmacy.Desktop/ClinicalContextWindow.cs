using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;

public sealed class ClinicalContextWindow:Window
{
    public ClinicalScreenInput? Assessment {get;private set;}
    public ClinicalContextWindow((Guid Id,string Name)[] medicines,string mode="",string encounter="",Guid? order=null,Guid? balance=null,decimal? issueQuantity=null)
    {
        Title="Medication reconciliation and screening context";Width=850;Height=Math.Min(850,SystemParameters.WorkArea.Height);WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var dock=new DockPanel{Margin=new Thickness(20)};Content=dock;var actions=new WrapPanel();DockPanel.SetDock(actions,Dock.Bottom);dock.Children.Add(actions);var run=new Button{Content="Run deterministic screening"};actions.Children.Add(run);var cancel=new Button{Content="Cancel"};actions.Children.Add(cancel);cancel.Click+=(_,_)=>DialogResult=false;
        var panel=new StackPanel();dock.Children.Add(new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});ClinicalPresentation.Heading(panel,"Review the active medication list and patient context");ClinicalPresentation.Line(panel,"History is not automatically treated as active treatment. Omit patient names. Optional daily-dose fields describe the regimen, not stock units being issued.");
        var rows=new List<(Guid Id,TextBox Dose,TextBox Unit,TextBox Route,TextBox Frequency)>();
        foreach(var medicine in medicines){ClinicalPresentation.Heading(panel,medicine.Name,17);var grid=new UniformGrid{Columns=4};panel.Children.Add(grid);
            TextBox Box(string label){var stack=new StackPanel();grid.Children.Add(stack);ClinicalPresentation.Line(stack,label);var box=new TextBox{Margin=new Thickness(0,0,6,8)};stack.Children.Add(box);return box;}
            rows.Add((medicine.Id,Box("Total daily dose (optional)"),Box("Dose unit, e.g. mg"),Box("Patient route"),Box("Frequency (text)")));
        }
        var reconciled=new CheckBox{Content=new TextBlock{Text="I reconciled the full active medication list, including outside medicines, OTC products and supplements.",TextWrapping=TextWrapping.Wrap},Margin=new Thickness(0,12,0,12)};panel.Children.Add(reconciled);
        TextBox Input(string label,int height=30){ClinicalPresentation.Line(panel,label);var box=new TextBox{MinHeight=height,AcceptsReturn=height>35,TextWrapping=TextWrapping.Wrap};panel.Children.Add(box);return box;}
        var reference=Input("Minimal encounter reference (required for dispensing)");reference.Text=encounter;reference.IsReadOnly=order!=null;
        ClinicalPresentation.Line(panel,"Allergy history");var allergy=new ComboBox{ItemsSource=new[]{"Unknown","NoneKnown","Documented"},SelectedIndex=0};panel.Children.Add(allergy);
        var allergens=Input("Documented allergen ingredient keys, comma separated (from published ingredient directory)");
        var records=new List<DocumentedDrugAllergy>();var allergyList=new ListBox{MinHeight=45,MaxHeight=110};panel.Children.Add(allergyList);var allergyButtons=new WrapPanel();panel.Children.Add(allergyButtons);var addAllergy=new Button{Content="Add allergy / intolerance details"};allergyButtons.Children.Add(addAllergy);var removeAllergy=new Button{Content="Remove selected allergy"};allergyButtons.Children.Add(removeAllergy);
        void RefreshAllergies(){allergyList.ItemsSource=null;allergyList.ItemsSource=records.Select(x=>x.Kind+": "+x.Key+" | "+x.Reaction+" | "+x.Certainty+" | "+(x.Intolerance?"Intolerance":"Allergy")+" | "+x.VerificationStatus).ToArray();}
        removeAllergy.Click+=(_,_)=>{if(allergyList.SelectedIndex>=0)records.RemoveAt(allergyList.SelectedIndex);RefreshAllergies();};
        addAllergy.Click+=(_,_)=>{var form=new FormWindow("Structured allergy or intolerance documentation",[new("kind","Allergen identity","choice","Ingredient|Class|Brand"),new("key","Published ingredient/class key, or exact brand reference/inventory ID"),new("reaction","Reported reaction"),new("severity","Reported severity","choice","Unknown|Mild|Moderate|Severe"),new("date","Reaction date yyyy-MM-dd (optional)"),new("certainty","Certainty","choice","Unknown|Suspected|Confirmed"),new("intolerance","This is documented intolerance rather than a confirmed allergy","bool"),new("source","Information source (patient/carer/record); omit patient names"),new("verified","Verification status","choice","Unverified|Verified")],[]){Owner=this};if(form.ShowDialog()!=true)return;try{var v=form.Values;var date=v["date"]!.ToString()!.Trim();records.Add(new(v["kind"]!.ToString()!,v["key"]!.ToString()!,v["reaction"]!.ToString()!,v["severity"]!.ToString()!,date==""?null:DateOnly.ParseExact(date,"yyyy-MM-dd",CultureInfo.InvariantCulture),v["certainty"]!.ToString()!,(bool)v["intolerance"]!,v["source"]!.ToString()!,v["verified"]!.ToString()!));allergy.SelectedItem="Documented";RefreshAllergies();}catch(Exception ex){MessageBox.Show(this,ex.Message,"Check allergy documentation");}};
        var conditions=Input("Documented condition keys, comma separated");var conditionsComplete=new CheckBox{Content="Patient conditions / relevant risks were reviewed; undocumented conditions are not assumed absent",Margin=new Thickness(0,10,0,10)};panel.Children.Add(conditionsComplete);
        var foods=Input("Relevant food keys, comma separated");var supplements=Input("Relevant supplement keys, comma separated");
        var factors=Input("Reviewed rule conditions: key=value, one per line. Include units in IV context values; no guessed renal/hepatic inputs.",90);
        if(mode=="IVCompatibility")factors.Text="ivConcentrationA=\nivConcentrationB=\nivDiluent=\nivTemperature=\nivContainer=\nivContact=";
        run.Click+=(_,_)=>{try{
            static string[] Keys(string value)=>value.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
            var context=new Dictionary<string,string>();if(mode!="")context["mode"]=mode;foreach(var line in factors.Text.Split('\n',StringSplitOptions.RemoveEmptyEntries)){var split=line.Split('=',2);if(split.Length!=2||split[0].Trim()=="")throw new InvalidOperationException("Use key=value for each factor.");if(split[1].Trim()!=""&&!context.TryAdd(split[0].Trim(),split[1].Trim()))throw new InvalidOperationException("Duplicate factor key.");}
            var selected=rows.Select(row=>new MedicationContext(row.Id,row.Dose.Text.Trim()==""?null:decimal.Parse(row.Dose.Text,System.Globalization.NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture),row.Unit.Text.Trim(),row.Route.Text.Trim(),row.Frequency.Text.Trim())).ToArray();
            Assessment=new(selected,reference.Text.Trim(),reconciled.IsChecked==true,allergy.SelectedItem!.ToString()!,Keys(allergens.Text),Keys(conditions.Text),conditionsComplete.IsChecked==true,context,Keys(foods.Text),Keys(supplements.Text),order,balance,issueQuantity,records.ToArray());DialogResult=true;
        }catch(Exception ex){MessageBox.Show(this,ex.Message,"Check screening context");}};
    }
}
