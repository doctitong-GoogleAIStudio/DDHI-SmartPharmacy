using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using DDHI.Pharmacy.Contracts;
using Microsoft.Win32;

namespace DDHI.Pharmacy.Desktop;
public partial class MainWindow
{
    bool connectionBusy;
    void ConnectionBusy(bool value)
    {
        connectionBusy = value;
        FindServerButton.IsEnabled = ImportConnectionButton.IsEnabled = LoginButton.IsEnabled = !value;
        PairServerButton.IsEnabled = !value && DiscoveredServers.SelectedItem is ServerConnection;
    }
    async void ConnectionLoaded(object sender, RoutedEventArgs e) => await FindServerAsync(false);
    async void FindServer(object sender, RoutedEventArgs e) => await FindServerAsync(true);
    async Task FindServerAsync(bool requested)
    {
        if (connectionBusy) return;
        ConnectionBusy(true);
        LoginStatus.Text = "Finding the pharmacy server on this network...";
        try
        {
            ServerConnection? known = null;
            try { known = ConnectionSettingsStore.Default.Load(); } catch { }
            var found = (await LanDiscovery.FindAsync()).GroupBy(c => c.PublicKeySha256).Select(g => g.First()).ToArray();
            DiscoveredServers.ItemsSource = found;
            DiscoveredServers.SelectedIndex = found.Length > 0 ? 0 : -1;
            var trusted = found.FirstOrDefault(c => c.PublicKeySha256 == known?.PublicKeySha256);
            if (trusted != null) await UseConnection(trusted, false);
            else if (known != null) await UseConnection(known, false);
            else if (found.Length == 1) await UseConnection(found[0], true);
            else LoginStatus.Text = found.Length > 1 ? "Select your pharmacy server, then click Pair selected server." : "No server found yet. On the server click Enable LAN. If discovery is blocked, open its connection file here.";
        }
        catch (Exception ex) { LoginStatus.Text = "Connection not ready: " + ex.Message + " You can retry Find server or open the server's connection file."; }
        finally { ConnectionBusy(false); }
    }
    async Task UseConnection(ServerConnection connection, bool confirm)
    {
        connection = connection.Validate();
        if (confirm && MessageBox.Show(this,
            $"Server: {connection.ServerName}\nAddress: {connection.Address}\n\nConnection code:\n{connection.PairingCode}\n\nCheck that this code matches the one on the SERVER screen. Does it match?\nDo not confirm a different code.",
            "Pair with your pharmacy server once", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        { LoginStatus.Text = "Pairing cancelled. No sign-in details were sent."; return; }
        using var client = new HttpClient(connection.CreateHandler()) { Timeout = TimeSpan.FromSeconds(7) };
        using var health = await client.GetAsync(connection.Address + "/health");
        health.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await health.Content.ReadAsStringAsync());
        if (json.RootElement.GetProperty("status").GetString() != "ready") throw new InvalidOperationException("The server database is not ready. Check Server Control.");
        ConnectionSettingsStore.Default.Save(connection);
        ApiAddress.Text = connection.Address;
        ConnectedServer.Text = "Server: " + connection.DisplayName;
        LoginStatus.Text = "Connected securely. Sign in with your own staff account. This server will be remembered.";
    }
    async void PairSelectedServer(object sender, RoutedEventArgs e)
    {
        if (connectionBusy || DiscoveredServers.SelectedItem is not ServerConnection selected) return;
        ConnectionBusy(true);
        try { await UseConnection(selected, true); } catch (Exception ex) { LoginStatus.Text = "Could not pair: " + ex.Message; }
        finally { ConnectionBusy(false); }
    }
    void SelectedServerChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
        PairServerButton.IsEnabled = !connectionBusy && DiscoveredServers.SelectedItem is ServerConnection;
    async void ImportConnection(object sender, RoutedEventArgs e)
    {
        if (connectionBusy) return;
        var dialog = new OpenFileDialog { Filter = "DDHI server connection|*.ddhiconnect", Title = "Open the connection file saved on the server" };
        if (dialog.ShowDialog(this) != true) return;
        ConnectionBusy(true);
        try
        {
            if (new FileInfo(dialog.FileName).Length > 8192) throw new InvalidDataException("This is not a DDHI connection file.");
            await UseConnection(ServerConnection.Parse(await File.ReadAllTextAsync(dialog.FileName)), true);
        }
        catch (Exception ex) { LoginStatus.Text = "Could not open the server connection: " + ex.Message; }
        finally { ConnectionBusy(false); }
    }
    void OpenClientConnectionGuide(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Documentation", "DDHI-Server-and-Client-Connection-Guide.pdf");
            if (!File.Exists(path)) { LoginStatus.Text = "The guide is in the complete release Documentation folder."; return; }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) { LoginStatus.Text = ex.Message; }
    }
}
