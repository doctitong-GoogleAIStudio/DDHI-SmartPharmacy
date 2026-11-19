using System.ComponentModel;
using System.Data;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    int auditPage=1,auditTotal;bool auditSessions,auditTotals,closingAfterSignout;
    string auditFrom="",auditTo="",auditStaff="",auditComputer="",auditActivity="",auditOutcome="";
    void ConfigureAuditToolbar()
    {
        Button("Activities",async()=>{auditTotals=false;auditSessions=false;auditPage=1;await LoadAudit();});
        Button("Timed sessions",async()=>{auditTotals=false;auditSessions=true;auditPage=1;await LoadAudit();});
        Button("Daily totals",async()=>{auditTotals=true;auditSessions=true;auditPage=1;await LoadAudit();});
        Button("Filter",FilterAudit);
        Button("Previous page",async()=>{if(auditPage>1){auditPage--;await LoadAudit();}});
        Button("Next page",async()=>{if(auditPage*200<auditTotal){auditPage++;await LoadAudit();}});
        Button("View details",AuditDetails);
        Button("Verify hash chain",async()=>{var result=await vm.Get("audit/verify");vm.Status=result.ToString();});
    }
    async Task LoadAudit()
    {
        string Q(string name,string value)=>"&"+name+"="+Uri.EscapeDataString(value);
        var result=await vm.Get((auditTotals?"audit/session-totals":auditSessions?"audit/sessions":"audit/trail")+"?page="+auditPage+Q("from",auditFrom)+Q("to",auditTo)+Q("staff",auditStaff)+Q("computer",auditComputer)+Q("activity",auditActivity)+Q("outcome",auditOutcome));
        auditTotal=result.GetProperty("total").GetInt32();vm.SetRows(result.GetProperty("rows"));
        vm.Status=$"{(auditTotals?"Daily observed totals":auditSessions?"Observed app sessions":"Audit activities")} - Page {auditPage} - {auditTotal:N0} matching records - 200 per page - Asia/Manila. "+(auditSessions?(auditTotals?"Default range: last 30 days. Overlap counted once per user/computer/day. ":"")+"Hours and minutes represent observed app connections, not active work or attendance. Unconfirmed endings use last contact.":"Select a row and View details for before/after fields, references and hash. Exports contain this page only.");
    }
    async Task FilterAudit()
    {
        var form=new FormWindow("Audit filters (blank means all)",[
            new("from","From date yyyy-MM-dd (session start date)","text",auditFrom),new("to","Through date yyyy-MM-dd","text",auditTo),
            new("staff","Staff email / name contains","text",auditStaff),new("computer","Computer contains","text",auditComputer),
            new("activity","Activity contains (activities only)","text",auditActivity),new("outcome","Outcome: Succeeded, Denied, Failed, Started, Canceled (activities only)","text",auditOutcome)],lookups){Owner=this};
        if(form.ShowDialog()!=true)return;
        string V(string key)=>form.Values[key]?.ToString()?.Trim()??"";
        auditFrom=V("from");auditTo=V("to");auditStaff=V("staff");auditComputer=V("computer");auditActivity=V("activity");auditOutcome=V("outcome");auditPage=1;await LoadAudit();
    }
    Task AuditDetails()
    {
        if(Table.SelectedItem is not DataRowView row)throw new InvalidOperationException("Select an audit activity or session first.");
        var text=string.Join("\n\n",row.DataView.Table!.Columns.Cast<DataColumn>().Select(c=>c.ColumnName+":\n"+Pretty(row[c.ColumnName]?.ToString()??"")));
        var window=new Window{Title="Audit details - read only",Width=850,Height=650,Owner=this,WindowStartupLocation=WindowStartupLocation.CenterOwner,Content=new TextBox{Text=text,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Padding=new Thickness(20)}};window.ShowDialog();return Task.CompletedTask;
    }
    static string Pretty(string value){try{using var json=JsonDocument.Parse(value);return JsonSerializer.Serialize(json.RootElement,new JsonSerializerOptions{WriteIndented=true});}catch{return value;}}
    async void CloseStaffWindow(object? sender,CancelEventArgs e)
    {
        if(closingAfterSignout||vm.UserId.Length==0)return;e.Cancel=true;IsEnabled=false;await vm.Logout();closingAfterSignout=true;Close();
    }
    async Task TryFileFailure(string action,string name){try{await FileActivity(action,"Failed",name,0);}catch{}}
    Task FileActivity(string action,string outcome,string fileName,int count)=>vm.Post("audit/file-activity",new{action,outcome,fileName=System.IO.Path.GetFileName(fileName),rows=count,page=current});
}
