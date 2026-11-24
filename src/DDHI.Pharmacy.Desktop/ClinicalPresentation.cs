using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;

public static class ClinicalPresentation
{
    public static Brush SeverityBrush(string severity)=>severity is "Contraindicated" or "Major"?Brushes.DarkRed:severity=="Moderate"?Brushes.DarkGoldenrod:Brushes.DarkSlateGray;
    public static StackPanel Safety(ClinicalScreenResult result)
    {
        var panel=new StackPanel();Heading(panel,"Medication safety assessment");Line(panel,"Database: "+result.DatabaseVersion);Line(panel,$"Medicines: {result.TotalMedications} | Pairs evaluated: {result.CombinationsEvaluated} | Contraindicated: {result.Findings.Count(x=>x.Severity=="Contraindicated")} | Major: {result.Findings.Count(x=>x.Severity=="Major")} | Moderate: {result.Findings.Count(x=>x.Severity=="Moderate")} | Minor: {result.Findings.Count(x=>x.Severity=="Minor")} | Unknown/incomplete: {result.Findings.Count(x=>x.Severity=="Unknown")}");
        foreach(var gap in result.Gaps)Line(panel,"INCOMPLETE: "+gap,Brushes.DarkRed);
        if(result.Findings.Length==0)Line(panel,"No evaluated findings. This is not a declaration of safety.",Brushes.DarkRed);
        foreach(var finding in result.Findings){var box=new StackPanel{Margin=new Thickness(12)};var severity=finding.Severity=="NoKnownInteractionFound"?"No known interaction found in cited coverage; no safety guarantee":finding.Severity;
            Heading(box,severity+" - "+finding.Title,18,SeverityBrush(finding.Severity));Line(box,string.Join(" + ",finding.Medicines));
            if(finding.PotentialSeverity!="")Line(box,"Potential severity if missing conditions apply: "+finding.PotentialSeverity,Brushes.DarkRed);
            if(finding.LastReviewed!=null)Line(box,$"Last reviewed: {finding.LastReviewed:yyyy-MM-dd}; review due: {finding.ReviewDue:yyyy-MM-dd}");
            if(finding.Conditions is {Length:>0})Line(box,"Applicable conditions: "+string.Join("; ",finding.Conditions.Select(c=>c.Field+" "+c.Operator+" "+c.Value)));
            foreach(var item in new[]{("Evidence",finding.Evidence),("Original source classification",finding.OriginalSeverity),("Mechanism",finding.Mechanism),("Consequences",finding.Consequences),("Management",finding.Management),("Monitoring",finding.Monitoring),("Dose guidance",finding.DoseGuidance),("Alternatives",finding.Alternatives),("Reference keys",string.Join(", ",finding.Sources))})if(item.Item2!="")Line(box,item.Item1+": "+item.Item2);
            panel.Children.Add(new Border{BorderBrush=SeverityBrush(finding.Severity),BorderThickness=new Thickness(3,0,0,0),Margin=new Thickness(0,10,0,8),Background=Brushes.White,Child=box});
        }
        Line(panel,result.Disclaimer);return panel;
    }
    public static void Heading(Panel panel,string text,double size=22,Brush? color=null)=>panel.Children.Add(new TextBlock{Text=text,FontWeight=FontWeights.SemiBold,FontSize=size,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,7),Foreground=color??(Brush)new BrushConverter().ConvertFromString("#103D31")!});
    public static void Line(Panel panel,string text,Brush? color=null)=>panel.Children.Add(new TextBlock{Text=text,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,2,0,7),Foreground=color??Brushes.DarkSlateGray});
    public static string Text(DependencyObject root){var result=new List<string>();void Visit(DependencyObject node){if(node is TextBlock text)result.Add(text.Text);for(var i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)Visit(VisualTreeHelper.GetChild(node,i));}Visit(root);return string.Join("\n\n",result);}
    public static StackPanel Monograph(JsonElement json,bool expanded,string section="")
    {
        var panel=new StackPanel();string Get(JsonElement e,string key)=>e.TryGetProperty(key,out var v)&&v.ValueKind!=JsonValueKind.Null?v.ToString():"";
        Heading(panel,"Drug monograph");Line(panel,"Database version: "+Get(json,"databaseVersion"));Line(panel,"Clinical approval: "+Get(json,"approvalStatus"));Line(panel,"Mapping: "+Get(json,"mapping"));
        foreach(var name in new[]{"inventory","drug","product"})if(json.TryGetProperty(name,out var objectValue)&&objectValue.ValueKind==JsonValueKind.Object){Heading(panel,name=="inventory"?"Hospital inventory product":name=="drug"?"Clinical drug identity":"Product and regulatory details",18);foreach(var property in objectValue.EnumerateObject().Where(p=>p.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False))if(!new[]{"id","createdAt","updatedAt","cerebroMedicineId"}.Contains(property.Name))Line(panel,System.Text.RegularExpressions.Regex.Replace(property.Name,"([A-Z])"," $1")+": "+property.Value);
            if(objectValue.TryGetProperty("ingredients",out var ingredients))foreach(var ingredient in ingredients.EnumerateArray())Line(panel,"Ingredient: "+string.Join(" | ",ingredient.EnumerateObject().Select(p=>p.Name+": "+p.Value)));
            foreach(var field in new[]{"classes","synonyms"})if(objectValue.TryGetProperty(field,out var values))Line(panel,field+": "+string.Join(", ",values.EnumerateArray().Select(x=>x.ToString())));
        }
        if(json.TryGetProperty("monograph",out var mono)&&mono.ValueKind==JsonValueKind.Object){var parsed=mono.Deserialize<MonographDefinition>(new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;Line(panel,$"Last reviewed: {parsed.LastReviewed:yyyy-MM-dd}; review due: {parsed.ReviewDue:yyyy-MM-dd}");Line(panel,"Reviewing pharmacist account: "+Get(json,"reviewingPharmacistId")+" | Approving Chief Pharmacist: "+Get(json,"approvingChiefPharmacistId"));
            var shown=parsed.Sections.Where(x=>(section==""||x.Heading==section)&&(expanded||new[]{"Identification","ApprovedIndications","Contraindications","SeriousWarnings","Monitoring"}.Contains(x.Heading)||section!="")).ToArray();
            foreach(var field in shown){Heading(panel,field.Heading,18);Line(panel,field.Text);Line(panel,"References: "+string.Join(", ",field.Sources));}if(shown.Length==0)Line(panel,"No approved content is available for this section. Do not infer a dose, compatibility or safety conclusion.",Brushes.DarkRed);
        }else Line(panel,"No approved clinical monograph. Catalog identity and stock are not a clinical drug reference.",Brushes.DarkRed);
        Heading(panel,"Hospital stock",18);if(json.TryGetProperty("stock",out var stock))foreach(var row in stock.EnumerateArray())Line(panel,string.Join(" | ",row.EnumerateObject().Where(p=>p.Name is "genericName" or "brandName" or "location" or "lot" or "expiration" or "status" or "quantity" or "reserved" or "recalled").Select(p=>p.Name+": "+p.Value)));
        Line(panel,Get(json,"sellingPrice"));Line(panel,Get(json,"regulatoryWarning"));
        if(expanded){Heading(panel,"Missing / unapproved sections",18);if(json.TryGetProperty("missingSections",out var missing))Line(panel,string.Join(", ",missing.EnumerateArray().Select(x=>x.GetString())));Heading(panel,"Clinical sources",18);if(json.TryGetProperty("sourceReferences",out var sources))foreach(var source in sources.EnumerateArray())Line(panel,string.Join(" | ",source.EnumerateObject().Select(p=>p.Name+": "+p.Value)));}
        Line(panel,DrugReferenceSections.Disclaimer);return panel;
    }
}

public sealed class SafetyReviewWindow:Window
{
    public ClinicalInterventionInput? Decision {get;private set;}
    public SafetyReviewWindow(Guid screenId,ClinicalScreenResult result)
    {
        Title="Pharmacist safety review";Width=960;Height=Math.Min(820,SystemParameters.WorkArea.Height);WindowStartupLocation=WindowStartupLocation.CenterOwner;var dock=new DockPanel{Margin=new Thickness(20)};Content=dock;
        var controls=new StackPanel();DockPanel.SetDock(controls,Dock.Bottom);dock.Children.Add(controls);
        var choice=new ComboBox{ItemsSource=new[]{"Hold","Escalate","Proceed with documented review"},SelectedIndex=0};ClinicalPresentation.Line(controls,"Clinical decision");controls.Children.Add(choice);
        var management=new TextBox{MinHeight=48,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap};ClinicalPresentation.Line(controls,"Management / intervention / independent reference checked (omit patient names)");controls.Children.Add(management);
        var clarification=new TextBox();ClinicalPresentation.Line(controls,"Prescriber clarification / escalation reference (required for contraindicated findings)");controls.Children.Add(clarification);
        var approval=new TextBox();ClinicalPresentation.Line(controls,"Hospital authorization / policy / source-check reference");controls.Children.Add(approval);
        var incomplete=new CheckBox{Content=new TextBlock{Text="I independently reviewed incomplete/unknown coverage, the active medication list, allergies and relevant patient context. This checkbox is not an automated safety clearance.",TextWrapping=TextWrapping.Wrap},Margin=new Thickness(0,9,0,9)};controls.Children.Add(incomplete);
        var actions=new WrapPanel();controls.Children.Add(actions);var save=new Button{Content="Record pharmacist decision"};actions.Children.Add(save);var cancel=new Button{Content="Cancel"};actions.Children.Add(cancel);cancel.Click+=(_,_)=>DialogResult=false;
        save.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(management.Text)||string.IsNullOrWhiteSpace(approval.Text)){MessageBox.Show(this,"Document the management decision and authorization reference.");return;}Decision=new(screenId,choice.SelectedItem!.ToString()!,management.Text,clarification.Text,approval.Text,incomplete.IsChecked==true,true);DialogResult=true;};
        dock.Children.Add(new ScrollViewer{Content=ClinicalPresentation.Safety(result),VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
    }
}
