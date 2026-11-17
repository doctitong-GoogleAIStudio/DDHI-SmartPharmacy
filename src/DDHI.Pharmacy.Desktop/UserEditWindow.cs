using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using DDHI.Pharmacy.Contracts;

namespace DDHI.Pharmacy.Desktop;
public sealed class UserEditWindow : Window
{
    readonly TextBox email=new(),name=new(),phone=new(),reason=new();readonly List<CheckBox> roles=[];
    public UserEditInput? Result {get;private set;}
    public UserEditWindow(JsonElement user,bool self)
    {
        Title="Edit staff profile and roles";Width=600;Height=710;MaxHeight=SystemParameters.WorkArea.Height;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var root=new StackPanel{Margin=new Thickness(24)};Content=new ScrollViewer{Content=root,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        root.Children.Add(new TextBlock{Text=Title,FontSize=23,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,16)});
        string Text(string key)=>user.TryGetProperty(key,out var value)&&value.ValueKind!=JsonValueKind.Null?value.ToString():"";
        email.Text=Text("email");name.Text=Text("fullName");phone.Text=Text("phoneNumber");
        foreach(var (label,field) in new[]{("Staff email",email),("Full name (optional; clear to remove)",name),("Phone (optional; clear to remove)",phone),("Reason for this change",reason)}){root.Children.Add(new TextBlock{Text=label});root.Children.Add(field);}
        root.Children.Add(new TextBlock{Text="Assigned roles",FontSize=17,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,12,0,8)});
        var current=user.GetProperty("roles").EnumerateArray().Select(r=>r.GetString()).ToArray();
        foreach(var role in ClientAccessPolicy.KnownRoles){var box=new CheckBox{Content=role,IsChecked=current.Contains(role),IsEnabled=!self,Margin=new Thickness(0,4,0,4)};roles.Add(box);root.Children.Add(box);}
        root.Children.Add(new TextBlock{Text=self?"Your own roles require another System Administrator to change them. Saving your profile will require you to sign in again.":"Saving changes ends this staff member's current sessions. Their permissions will refresh when they sign in again.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8)});
        var save=new Button{Content="Review and save changes"};root.Children.Add(save);
        save.Click+=(_,_)=>{
            if(string.IsNullOrWhiteSpace(reason.Text)){MessageBox.Show(this,"Enter a reason for the account change.");return;}
            if(MessageBox.Show(this,"Save this staff profile and selected roles? The server will validate permissions and preserve the last active administrator.",Title,MessageBoxButton.OKCancel,MessageBoxImage.Question)!=MessageBoxResult.OK)return;
            Result=new(email.Text,name.Text,phone.Text,roles.Where(r=>r.IsChecked==true).Select(r=>r.Content.ToString()!).ToArray(),reason.Text,true,Text("version"));DialogResult=true;
        };
        var cancel=new Button{Content="Cancel"};root.Children.Add(cancel);cancel.Click+=(_,_)=>DialogResult=false;
    }
}
