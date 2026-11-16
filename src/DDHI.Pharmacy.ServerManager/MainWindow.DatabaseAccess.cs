using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace DDHI.Pharmacy.ServerManager;
public partial class MainWindow
{
    DatabaseAccessSession? recordSession;
    string recordSessionInstance = "";
    JsonElement? displayedRecords;
    DatabaseDataset? displayedDataset;
    string RecordInstance => profile.ProfilePath + "|" + profile.Endpoint + "|" + profile.CertificateThumbprint;
    void FillDatabaseAccess()
    {
        DbConnectionDetails.Text = controller.DatabaseConnectionDetails;
        if (DbDataset.ItemsSource == null) { DbDataset.ItemsSource = DatabaseDataset.All; DbDataset.SelectedIndex = 0; }
        if (recordSession != null && (recordSessionInstance != RecordInstance || !recordSession.IsAuthenticated)) ClearDatabaseSession();
        var enabled = profile.Prepared && !PreviewOnly && !busy;
        DbCheckButton.IsEnabled = DbSignInButton.IsEnabled = DbPsqlButton.IsEnabled = DbAdminButton.IsEnabled = enabled;
        DbLoadButton.IsEnabled = enabled && recordSession?.IsAuthenticated == true;
        DbSignOutButton.IsEnabled = !busy && recordSession != null;
        DbExportButton.IsEnabled = enabled && recordSession?.IsAuthenticated == true && displayedRecords != null;
    }
    void ClearDatabaseSession()
    {
        recordSession?.Dispose(); recordSession = null; recordSessionInstance = "";
        displayedRecords = null; displayedDataset = null; DbRecords.ItemsSource = null;
        DbSessionStatus.Text = "Not signed in. Record access uses your application role.";
    }
    async Task DatabaseAction(Func<Task> action)
    {
        await Guard(async () => {
            try { await action(); }
            catch (Exception ex) { DbAccessMessage.Text = ex.Message; throw; }
        });
    }
    async void CheckDatabaseAccess(object sender, RoutedEventArgs e) => await DatabaseAction(async () => {
        DbAccessMessage.Text = "Checking the existing database and HTTPS endpoint...";
        DbAccessMessage.Text = await controller.VerifyDatabase() + "\n" + await controller.Health();
    });
    void CopyDatabaseDetails(object sender, RoutedEventArgs e)
    {
        try { Clipboard.SetText(controller.DatabaseConnectionDetails); DbAccessMessage.Text = "Copied connection details without passwords."; }
        catch (Exception ex) { DbAccessMessage.Text = ex.Message; }
    }
    async void SignInDatabaseAccess(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        var email = DbStaffEmail.Text.Trim(); var password = DbStaffPassword.Password; DbStaffPassword.Clear();
        await DatabaseAction(async () => {
            ClearDatabaseSession();
            recordSession = controller.OpenRecordSession(); recordSessionInstance = RecordInstance;
            try { await recordSession.SignIn(email, password); }
            catch { ClearDatabaseSession(); throw; }
            DbSessionStatus.Text = recordSession.Identity;
            DbAccessMessage.Text = "Signed in. Select a dataset and click View records. Sessions expire within 20 minutes.";
        });
    }
    void SignOutDatabaseAccess(object sender, RoutedEventArgs e) { if (busy) return; ClearDatabaseSession(); FillDatabaseAccess(); DbAccessMessage.Text = "Signed out. Records and session token cleared from this window."; }
    async void LoadDatabaseRecords(object sender, RoutedEventArgs e)
    {
        if (busy || DbDataset.SelectedItem is not DatabaseDataset selected) return;
        await DatabaseAction(async () => {
            displayedRecords = null; displayedDataset = null; DbRecords.ItemsSource = null;
            if (recordSession == null) throw new InvalidOperationException("Sign in first.");
            var result = await recordSession.Read(selected);
            displayedRecords = result; displayedDataset = selected; DbRecords.ItemsSource = DatabaseRecordExport.ToView(result);
            DbAccessMessage.Text = $"{selected.Name}: {result.GetArrayLength()} records loaded. Read-only API snapshot; server result limits apply. Export saves this loaded snapshot.";
        });
    }
    void ExportDatabaseRecords(object sender, RoutedEventArgs e)
    {
        if (busy || recordSession?.IsAuthenticated != true || displayedRecords is not JsonElement rows || displayedDataset == null) return;
        var dialog = new SaveFileDialog { Filter = "CSV for Excel|*.csv|JSON|*.json", FileName = "DDHI-records-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), AddExtension = true };
        if (dialog.ShowDialog(this) != true) return;
        try {
            // Recheck after the file picker: an expired session cannot export an old snapshot.
            if (recordSession?.IsAuthenticated != true) throw new InvalidOperationException("Session expired. Sign in and reload records before exporting.");
            if (dialog.FilterIndex == 2) File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
            else File.WriteAllText(dialog.FileName, DatabaseRecordExport.Csv(rows), new UTF8Encoding(true));
            DbAccessMessage.Text = $"Exported {rows.GetArrayLength()} {displayedDataset.Name} records. Protect this file according to hospital data policy.";
        } catch (Exception ex) { DbAccessMessage.Text = ex.Message; }
    }
    void OpenDatabaseShell(object sender, RoutedEventArgs e)
    {
        try {
            var user = DbToolUser.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(user, "^[A-Za-z_][A-Za-z0-9_]{0,62}$")) throw new InvalidOperationException("Enter the PostgreSQL account supplied by IT.");
            var exe = Path.Combine(profile.PostgresBin, "psql.exe");
            if (!File.Exists(exe)) throw new FileNotFoundException("The bundled SQL Shell is missing. Install the complete server package.");
            if (MessageBox.Show(this, "Open PostgreSQL SQL Shell for IT administration?\n\nIt prompts for this DATABASE account's password, not your pharmacy staff password. App-managed passwords are not revealed or passed to the tool. The shell starts with transactions read-only; PostgreSQL account privileges still govern access.", "Open database tool", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) return;
            var info = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = false, WorkingDirectory = profile.PostgresBin };
            foreach (var arg in new[] { "-X", "-W", "-h", "127.0.0.1", "-p", profile.PostgresPort.ToString(), "-d", "ddhi_pharmacy", "-U", user }) info.ArgumentList.Add(arg);
            info.Environment.Remove("PGPASSWORD"); info.Environment.Remove("PGPASSFILE");
            info.Environment["PGOPTIONS"] = "-c default_transaction_read_only=on -c statement_timeout=30000";
            Process.Start(info); DbAccessMessage.Text = "SQL Shell opened. IT must use its database login; no saved password was shared.";
        } catch (Exception ex) { DbAccessMessage.Text = ex.Message; }
    }
    void OpenDatabaseAdmin(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Installed pgAdmin|pgAdmin4.exe", Title = "Select an installed pgAdmin4.exe (pgAdmin is optional and not bundled)" };
        if (dialog.ShowDialog(this) != true) return;
        try {
            if (!string.Equals(Path.GetFileName(dialog.FileName), "pgAdmin4.exe", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Select the installed pgAdmin4.exe.");
            Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(dialog.FileName) });
            DbAccessMessage.Text = "pgAdmin opened. Use the displayed loopback host, port and database with an IT-provisioned PostgreSQL login. Saved passwords were not shared.";
        } catch (Exception ex) { DbAccessMessage.Text = ex.Message; }
    }
}
