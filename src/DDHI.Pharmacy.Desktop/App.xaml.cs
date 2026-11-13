using System.Windows;
namespace DDHI.Pharmacy.Desktop;
public partial class App:Application { protected override void OnStartup(StartupEventArgs e){new SplashScreen("Assets/hospital-logo.png").Show(true);base.OnStartup(e);DispatcherUnhandledException+=(s,a)=>{MessageBox.Show("The operation could not be completed. Please reconnect and review the transaction before retrying.","DDHI Smart Pharmacy");a.Handled=true;};} }
