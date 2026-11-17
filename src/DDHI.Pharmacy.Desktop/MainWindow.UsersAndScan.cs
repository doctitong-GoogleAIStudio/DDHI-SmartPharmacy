using System.Text.Json;
using System.Windows;
using DDHI.Pharmacy.Contracts;
namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    void UserColumns(object? sender,System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
    {
        if(current=="Medicine Master"){if(e.PropertyName.Equals("templateMedicineId",StringComparison.OrdinalIgnoreCase)){e.Cancel=true;return;}e.Column.Header=e.PropertyName.ToLowerInvariant() switch{"importedby"=>"Imported by","distributedby"=>"Distributed by","genericname"=>"Generic name","brandname"=>"Brand","brandcount"=>"Brands / unbranded labels","productcount"=>"Separate products","activeproducts"=>"Active products","baseunit"=>"Base unit","dosageform"=>"Dosage form",_=>e.Column.Header};return;}
        if(current!="User Administration")return;
        if(e.PropertyName.Equals("id",StringComparison.OrdinalIgnoreCase)||e.PropertyName.Equals("version",StringComparison.OrdinalIgnoreCase)){e.Cancel=true;return;}
        e.Column.Header=e.PropertyName.ToLowerInvariant() switch{"fullname"=>"Full name","email"=>"Staff email","phonenumber"=>"Phone","roles"=>"Assigned roles","lockoutend"=>"Locked until",_=>e.PropertyName};
    }
    async Task LookupBarcode(string raw)
    {
        var generation=vm.SessionVersion;
        JsonElement response;
        try {response=await vm.Post("scan",new{barcode=raw});}
        catch(StaffSessionEndedException ex){EndStaffSession(ex.Message);throw;}
        if(generation!=vm.SessionVersion)throw new InvalidOperationException("The signed-in account changed. Scan again after signing in.");
        if(!response.TryGetProperty("result",out var result))throw new InvalidOperationException("Update the server to the matching release to use the readable scan result.");
        var parsed=result.Deserialize<ScanLookupResult>(new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new InvalidOperationException("The server returned no scan result.");
        ScanResultPanel.ShowResult(parsed);vm.Status=parsed.Message;
    }
    async Task EditUser()
    {
        var id=SelectedId();var user=await vm.Get("users/"+id);
        var dialog=new UserEditWindow(user,id==vm.UserId){Owner=this};
        if(dialog.ShowDialog()!=true||dialog.Result==null)return;
        await vm.Post("users/"+id+"/edit",dialog.Result);
        if(id==vm.UserId){EndStaffSession("Your profile changed. Sign in again with the updated email.");return;}
        await LoadPage();vm.Status="Staff information and roles updated. That staff member must sign in again.";
    }
    async Task DeleteUser()
    {
        var id=SelectedId();var user=await vm.Get("users/"+id);var email=user.GetProperty("email").GetString()??"";
        var dialog=new FormWindow("Delete account "+email+" — clinical transactions and audit history are retained",[
            new("confirmationEmail","Type the selected account's email to confirm permanent removal of its login/profile"),new("reason","Reason for deleting this account")],lookups){Owner=this};
        if(dialog.ShowDialog()!=true)return;
        await vm.Post("users/"+id+"/delete",new UserDeleteInput(dialog.Values["confirmationEmail"]?.ToString()??"",dialog.Values["reason"]?.ToString()??"",true,user.GetProperty("version").GetString()??""));
        await LoadPage();vm.Status="Account login and profile deleted. Historical stock transactions and audit records are retained.";
    }
    void EndStaffSession(string message)
    {
        vm.ClearSession();Toolbar.Children.Clear();Summary.Visibility=Visibility.Collapsed;ScanResultPanel.Reset();ScanResultPanel.Visibility=Visibility.Collapsed;lookups.Clear();Navigation.SelectedIndex=-1;LoginPanel.Visibility=Visibility.Visible;Password.Clear();LoginStatus.Text=message;
    }
}
public sealed class StaffSessionEndedException(string message):InvalidOperationException(message);
