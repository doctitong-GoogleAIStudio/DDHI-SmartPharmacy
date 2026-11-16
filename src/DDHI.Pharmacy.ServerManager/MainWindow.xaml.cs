using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace DDHI.Pharmacy.ServerManager;
public partial class MainWindow : Window
{
    static string InstanceLocator => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DDHI", "server-instance.json");
    ServerProfile profile = InitialProfile();
    ServerController controller = null!;
    bool busy;
    bool? usersExist;
    bool PreviewOnly => File.Exists(Path.Combine(Directory.GetParent(ServerProfile.Defaults().DataRoot)!.FullName, "preparation-only.flag"));
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(10) };

    static ServerProfile ReadExistingProfile(string path)
    {
        var loaded = JsonSerializer.Deserialize<ServerProfile>(File.ReadAllText(path)) ?? throw new InvalidDataException("Invalid server profile.");
        if (!loaded.Prepared || !string.Equals(Path.GetFullPath(path), Path.GetFullPath(loaded.ProfilePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Select the prepared instance's own profile.json in its original data folder.");
        loaded.Validate();
        ServerSecrets.Load(loaded.DataRoot);
        return loaded;
    }
    static ServerProfile InitialProfile()
    {
        var defaults = ServerProfile.Defaults();
        if (File.Exists(Path.Combine(Directory.GetParent(defaults.DataRoot)!.FullName, "preparation-only.flag"))) return defaults;
        try
        {
            if (File.Exists(InstanceLocator))
            {
                var path = JsonSerializer.Deserialize<string>(File.ReadAllText(InstanceLocator));
                if (path != null && File.Exists(path)) return ReadExistingProfile(path);
            }
        }
        catch { /* Explicit existing-profile selection remains available. */ }
        return ServerProfile.LoadOrDefault();
    }
    void RememberInstance()
    {
        if (PreviewOnly) return;
        Directory.CreateDirectory(Path.GetDirectoryName(InstanceLocator)!);
        File.WriteAllText(InstanceLocator, JsonSerializer.Serialize(profile.ProfilePath));
    }
    public MainWindow()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height;
        Height = Math.Min(Height, MaxHeight);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        controller = new(profile, Log);
        Fill();
        timer.Tick += async (_, _) => { if (!busy && profile.Prepared) await Guard(RefreshStatus); };
        timer.Start();
        Closed += (_, _) => { timer.Stop(); recordSession?.Dispose(); };
    }
    void Fill()
    {
        FillSimpleLan();
        FillDatabaseAccess();
        DataRootInput.Text = profile.DataRoot; PgBinInput.Text = profile.PostgresBin;
        PgPortInput.Text = profile.PostgresPort.ToString(); ApiPortInput.Text = profile.ApiPort.ToString();
        HostnameInput.Text = profile.Hostname; PfxInput.Text = profile.CertificatePath;
        ApprovalInput.Text = profile.ApprovalReference; ModeInput.SelectedIndex = profile.Mode == "Hospital LAN" ? 1 : 0;
        EndpointDisplay.Text = profile.Endpoint; DataDisplay.Text = profile.DataRoot;
        CertificatePathDisplay.Text = profile.CertificatePath; ThumbprintDisplay.Text = profile.CertificateThumbprint;
        CertificateNote.Text = profile.Mode == "Simple LAN" ? "LAN enabled. Clients find this server automatically and confirm the code once." : profile.Mode == "Local evaluation"
            ? "This connection is local-only. Open Connect computers and click Enable LAN for separate clients."
            : "Hospital LAN: use Copy client address on each workstation. IT must establish client trust and scoped firewall access.";
        TlsStatus.Text = profile.Prepared ? (profile.Mode == "Simple LAN" ? "App-managed HTTPS" : profile.Mode == "Local evaluation" ? "Local PFX" : "Hospital PFX supplied") : "Not configured";
        PrepareButton.IsEnabled = !profile.Prepared && !busy && !PreviewOnly;
        StartButton.IsEnabled = StopButton.IsEnabled = profile.Prepared && !busy && !PreviewOnly;
        LanApplyButton.IsEnabled = profile.Prepared && !busy && !PreviewOnly && controller.ConnectionChangeState is not ("Applying" or "RecoveryRequired");
        RestoreConnectionButton.IsEnabled = profile.Prepared && !busy && !PreviewOnly && controller.CanRestoreConnection;
        AdminCreateButton.IsEnabled = profile.Prepared && usersExist == false && !busy && !PreviewOnly;
        AdminSetupNote.Text = usersExist switch
        {
            true => "An application administrator already exists. Sign in with the existing account. Use Start server to run the API; administrator creation is not a start command.",
            false => "No application users exist. Create the first administrator once, then use Start server. Assign staff and clinical roles separately.",
            _ => "Check database health first. Existing administrator accounts must not be recreated."
        };
        LanStateDisplay.Text = $"Current connection: {profile.Endpoint}\nConnection change state: {controller.ConnectionChangeState}";
        if (string.IsNullOrWhiteSpace(LanHostnameInput.Text) && profile.Mode == "Hospital LAN") LanHostnameInput.Text = profile.Hostname;
        if (string.IsNullOrWhiteSpace(LanApprovalInput.Text)) LanApprovalInput.Text = profile.ApprovalReference;
        if (PreviewOnly) Banner.Text = "Installer preview on this computer. Open the prepared instance on the dedicated server to configure LAN access.";
        else if (controller.ConnectionChangeState is "Applying" or "RecoveryRequired") Banner.Text = "A connection change needs recovery. Use Restore previous connection before applying another change.";
    }
    void ReadInputs()
    {
        if (profile.Prepared) return;
        profile.DataRoot = DataRootInput.Text.Trim(); profile.PostgresBin = PgBinInput.Text.Trim();
        profile.PostgresPort = int.Parse(PgPortInput.Text); profile.ApiPort = int.Parse(ApiPortInput.Text);
        profile.Hostname = HostnameInput.Text.Trim().ToLowerInvariant(); profile.CertificatePath = PfxInput.Text.Trim();
        profile.ApprovalReference = ApprovalInput.Text.Trim(); profile.Mode = ((ComboBoxItem)ModeInput.SelectedItem).Content.ToString()!;
        controller = new(profile, Log);
    }
    void Log(string text) => Dispatcher.Invoke(() => { Activity.AppendText($"{DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)):yyyy-MM-dd HH:mm:ss} {text}\n"); Activity.ScrollToEnd(); Banner.Text = text; });
    async Task Guard(Func<Task> action)
    {
        if (busy) return;
        busy = true; Fill();
        try { await action(); }
        catch (Exception ex) { Log("Action could not complete: " + ex.Message); }
        finally { busy = false; Fill(); }
    }
    async Task RefreshStatus()
    {
        if (!profile.Prepared) { ApiStatus.Text = DatabaseStatus.Text = "Not configured"; usersExist = null; return; }
        try { DatabaseStatus.Text = await controller.VerifyDatabase(); usersExist = await controller.HasApplicationUsers(); }
        catch (Exception ex) { DatabaseStatus.Text = "Unavailable"; usersExist = null; Banner.Text = ex.Message; }
        try { ApiStatus.Text = await controller.Health(); }
        catch (Exception ex) { ApiStatus.Text = "Stopped / not ready"; Banner.Text = "HTTPS health check: " + ex.GetBaseException().Message; }
    }
    async void LoadedWindow(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly)
        {
            MainTabs.SelectedItem = DatabaseAccessTab;
            await File.WriteAllTextAsync(Path.Combine(Directory.GetParent(ServerProfile.Defaults().DataRoot)!.FullName, "manager-preview.json"), JsonSerializer.Serialize(new { Machine = Environment.MachineName, DesktopWindowLoaded = true, PreparationOnly = true, LocalInitializationDisabled = true, Version = "0.5.0", LanConnectionControlsLoaded = true, ConnectionGuideLoaded = true, SimpleLanControlsLoaded = true, DatabaseAccessControlsLoaded = true }));
            return;
        }
        if (profile.Prepared) await Guard(RefreshStatus);
    }
    async void PrepareServer(object sender, RoutedEventArgs e)
    {
        try { ReadInputs(); } catch (Exception ex) { Log(ex.Message); return; }
        if (MessageBox.Show(this, "Create this independent server instance with the displayed paths and ports? For an existing server, use Open existing server profile and LAN connection instead.", "Confirm new setup", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
        var password = PfxPassword.Password; PfxPassword.Clear();
        await Guard(async () => { await controller.Prepare(password); RememberInstance(); if (UseWindowsServices.IsChecked == true) await controller.InstallWindowsServices(); if(AutoLanAfterSetup.IsChecked==true && profile.Mode=="Local evaluation" && NetworkAddress.SelectedItem is DDHI.Pharmacy.Contracts.LanAddress address)await controller.EnableSimpleLan(address); await RefreshStatus(); Log("Database and certificate are ready. Create the first administrator once, then click Start server."); });
    }
    async void ApplyLanConnection(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly) return;
        var options = new LanConnectionOptions(LanHostnameInput.Text, LanPfxInput.Text.Trim(), LanApprovalInput.Text);
        var password = LanPfxPassword.Password; LanPfxPassword.Clear();
        if (MessageBox.Show(this, "Apply the LAN hostname and certificate to this existing instance and start the server? API and worker connections will be interrupted. The database is retained. Connection settings are backed up and automatic rollback is attempted if HTTPS activation fails. Confirm an approved downtime window and maintain your encrypted database backups.", "Configure existing server connection", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
        await Guard(async () => { await controller.ConfigureLanConnection(options, password); RememberInstance(); await RefreshStatus(); });
    }
    async void RestoreConnection(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly) return;
        if (MessageBox.Show(this, "Restore the last saved connection settings and restart the API and worker? Existing clients will be disconnected; the database and users remain in this instance.", "Restore previous connection", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        await Guard(async () => { await controller.RestorePreviousConnection(); await RefreshStatus(); });
    }
    async void OpenExistingProfile(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Filter = "Existing server profile|profile.json", Title = "Select the prepared server's profile.json in its original data folder" };
        if (dialog.ShowDialog() != true) return;
        await Guard(async () => { var loaded = ReadExistingProfile(dialog.FileName); profile = loaded; controller = new(profile, Log); usersExist = null; RememberInstance(); await RefreshStatus(); Log("Opened existing instance: " + profile.DataRoot); });
    }
    async void ConfigureServices(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly) return;
        if (MessageBox.Show(this, "Configure the three DDHI Windows services for this instance? Existing owned processes will be stopped during configuration.", "Configure Windows services", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            await Guard(async () => { await controller.InstallWindowsServices(); await RefreshStatus(); });
    }
    void TrustLocalCertificate(object sender, RoutedEventArgs e)
    {
        if (PreviewOnly) return;
        if (MessageBox.Show(this, "Trust the displayed localhost certificate for this Windows account? Verify its fingerprint. This does not establish hospital LAN or workstation trust.", "Authorize local certificate trust", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        try { controller.TrustLocalCertificateForCurrentUser(); TlsStatus.Text = "Trusted locally (this Windows account)"; } catch (Exception ex) { Log(ex.Message); }
    }
    async void StartServer(object sender, RoutedEventArgs e) => await Guard(async () => { await controller.Start(); await RefreshStatus(); });
    async void StopServer(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, "Stop this server and disconnect active clients? Confirm an approved downtime window.", "Stop server", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            await Guard(async () => { await controller.Stop(); await RefreshStatus(); });
    }
    async void CheckHealth(object sender, RoutedEventArgs e) => await Guard(RefreshStatus);
    async void CreateAdmin(object sender, RoutedEventArgs e)
    {
        var email = AdminEmail.Text.Trim(); var password = AdminPassword.Password; AdminPassword.Clear();
        if (string.IsNullOrWhiteSpace(email) || password.Length < 12) { Log("Enter an individual email and a strong password of at least 12 characters."); return; }
        await Guard(async () => { if (await controller.HasApplicationUsers()) throw new InvalidOperationException("An administrator already exists. Use the existing account and Start server."); await controller.Bootstrap(email, password); await RefreshStatus(); });
    }
    static string? PickPfx() { var dialog = new OpenFileDialog { Filter = "PFX certificate|*.pfx;*.p12" }; return dialog.ShowDialog() == true ? dialog.FileName : null; }
    void BrowsePfx(object sender, RoutedEventArgs e) { var path = PickPfx(); if (path != null) PfxInput.Text = path; }
    void BrowseLanPfx(object sender, RoutedEventArgs e) { var path = PickPfx(); if (path != null) LanPfxInput.Text = path; }
    void CopyClientAddress(object sender, RoutedEventArgs e) { try { Clipboard.SetText(profile.Endpoint); Log("Copied client base address: " + profile.Endpoint); } catch (Exception ex) { Log("Could not copy the address. Select the Server endpoint text instead. " + ex.Message); } }
    void OpenConnectionGuide(object sender, RoutedEventArgs e)
    {
        try
        {
            const string name = "DDHI-Server-and-Client-Connection-Guide.pdf";
            var root = ServerProfile.Defaults().ServerRoot;
            var path = Path.Combine(root, "Documentation", name);
            if (!File.Exists(path)) path = Path.Combine(Directory.GetParent(root)!.FullName, "Documentation", name);
            if (!File.Exists(path)) { Log("The complete connection guide is missing. Obtain the complete server installer/release package."); return; }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) { Log("Could not open the PDF viewer: " + ex.Message); }
    }
    void OpenLogs(object sender, RoutedEventArgs e) { var path = Path.Combine(profile.DataRoot, "logs"); if (Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe") { Arguments = "\"" + path + "\"", UseShellExecute = true }); }
    void OpenCertificate(object sender, RoutedEventArgs e) { var path = Path.Combine(profile.DataRoot, "localhost-server.cer"); if (File.Exists(path)) Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); else Log("No local public certificate exists. Supply the approved hospital PFX for LAN mode."); }
}
