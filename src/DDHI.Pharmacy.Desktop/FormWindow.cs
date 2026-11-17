using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
namespace DDHI.Pharmacy.Desktop;
public record Field(string Name,string Label,string Kind="text",string Default="",string? Lookup=null);
public sealed class FormWindow:Window {
 readonly Dictionary<string,FrameworkElement> controls=[];readonly Field[] fields;
 public Dictionary<string,object?> Values {get;private set;}=[];
 public FormWindow(string title,Field[] fields,Dictionary<string,List<KeyValuePair<string,string>>> lookups){
  this.fields=fields;Title=title;Width=560;Height=740;WindowStartupLocation=WindowStartupLocation.CenterOwner;Background=System.Windows.Media.Brushes.White;
  var outer=new DockPanel{Margin=new Thickness(24)};Content=outer;var buttons=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(buttons,Dock.Bottom);outer.Children.Add(buttons);
  var confirm=new Button{Content="Review and confirm",Padding=new Thickness(14,9,14,9)};buttons.Children.Add(confirm);confirm.Click+=(s,e)=>Submit();var cancel=new Button{Content="Cancel",Padding=new Thickness(14,9,14,9)};buttons.Children.Add(cancel);cancel.Click+=(s,e)=>{DialogResult=false;};
  var stack=new StackPanel();outer.Children.Add(new ScrollViewer{Content=stack,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
  stack.Children.Add(new TextBlock{Text=title,FontSize=23,Margin=new Thickness(0,0,0,16),TextWrapping=TextWrapping.Wrap});
  foreach(var field in fields){if(field.Kind!="bool")stack.Children.Add(new TextBlock{Text=field.Label,TextWrapping=TextWrapping.Wrap});FrameworkElement element;
   if(field.Lookup!=null){var combo=new ComboBox{DisplayMemberPath="Value",SelectedValuePath="Key",ItemsSource=lookups.GetValueOrDefault(field.Lookup,[])};element=combo;}
   else if(field.Kind=="bool")element=new CheckBox{Content=new TextBlock{Text=field.Label,TextWrapping=TextWrapping.Wrap},IsChecked=field.Default=="true",Margin=new Thickness(0,8,0,14)};
   else if(field.Kind=="date")element=new DatePicker{Margin=new Thickness(0,5,0,14)};
   else if(field.Kind=="choice")element=new ComboBox{ItemsSource=field.Default.Split('|'),SelectedIndex=0};
   else if(field.Kind=="password")element=new PasswordBox{Padding=new Thickness(8),Margin=new Thickness(0,5,0,14)};
   else element=new TextBox{Text=field.Default};
   controls[field.Name]=element;stack.Children.Add(element);
  }
 }
 void Submit(){try{foreach(var f in fields){var c=controls[f.Name];object? value=c switch{CheckBox check=>check.IsChecked==true,DatePicker date=>date.SelectedDate?.ToString("yyyy-MM-dd"),ComboBox combo when f.Lookup!=null=>combo.SelectedValue?.ToString(),ComboBox combo=>combo.SelectedItem?.ToString(),PasswordBox p=>p.Password,TextBox t=>t.Text,_=>null};
   if(f.Kind=="number")value=decimal.Parse(value?.ToString()??"",System.Globalization.CultureInfo.InvariantCulture);
   if(f.Kind!="bool"&&value==null)throw new InvalidOperationException("Complete all fields.");Values[f.Name]=value;
  }Values["confirmed"]=true;if(MessageBox.Show(this,"Confirm this transaction? The central server will validate permissions and safety checks.",Title,MessageBoxButton.OKCancel,MessageBoxImage.Question)==MessageBoxResult.OK)DialogResult=true;
 }catch(Exception e){MessageBox.Show(this,e.Message,"Check entered values");}}
}
