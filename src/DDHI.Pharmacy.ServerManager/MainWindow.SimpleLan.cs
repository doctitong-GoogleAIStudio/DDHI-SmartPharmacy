using System.IO;
using System.Text.Json;
using System.Windows;
using DDHI.Pharmacy.Contracts;
using Microsoft.Win32;

namespace DDHI.Pharmacy.ServerManager;
public partial class MainWindow
{
    void FillSimpleLan()
    {
        if (NetworkAddress.ItemsSource == null)
        {
            var addresses=LanAddresses.Available();NetworkAddress.ItemsSource = addresses;
            NetworkAddress.SelectedItem=addresses.FirstOrDefault(a=>a.Name==profile.SimpleLanInterface)??addresses.FirstOrDefault();
        }
        EnableLanButton.IsEnabled = profile.Prepared && !busy && !PreviewOnly;
        SaveConnectionButton.IsEnabled = profile.Prepared && profile.Mode == "Simple LAN" && !busy && !PreviewOnly;
        SimpleRestoreButton.IsEnabled = controller.CanRestoreConnection && !busy && !PreviewOnly;
        SimpleEndpoint.Text = !profile.Prepared ? "New server: use Initial setup once. LAN will be enabled automatically." : profile.Mode == "Simple LAN" ? profile.Endpoint : "Click Enable LAN to connect separate computers.";
        PairingCodeDisplay.Text = profile.Mode == "Simple LAN" ? controller.ClientConnection().PairingCode : "Shown after LAN is enabled";
    }
    async void EnableLan(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly || busy) return;
        if (NetworkAddress.SelectedItem is not LanAddress selected) { Log("Connect the server to the hospital network, reopen Server Control, and select its network adapter."); return; }
        if (MessageBox.Show(this, "Enable pharmacy clients on this local network?\n\n" + selected + "\n\nThis creates an app-managed encrypted connection and scoped firewall rules for this adapter's local subnet. Existing API/worker connections restart; your database and users are retained. Use approved downtime and a trusted hospital network.", "Enable LAN", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
        await Guard(async () => { await controller.EnableSimpleLan(selected); RememberInstance(); await RefreshStatus(); });
    }
    void SaveConnection(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly || profile.Mode != "Simple LAN") return;
        var dialog = new SaveFileDialog { Filter = "DDHI connection file|*.ddhiconnect", DefaultExt = ".ddhiconnect", FileName = "DDHI-server.ddhiconnect" };
        if (dialog.ShowDialog(this) != true) return;
        try { File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(controller.ClientConnection(), new JsonSerializerOptions { WriteIndented = true })); Log("Connection file saved. It contains the server address and public identity only, not passwords or private keys."); }
        catch (Exception ex) { Log("Could not save the connection file: " + ex.Message); }
    }
}
