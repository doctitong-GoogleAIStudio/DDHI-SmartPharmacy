using DDHI.Pharmacy.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

sealed class DiscoveryWorker(IConfiguration configuration, ILogger<DiscoveryWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        var json = configuration["DDHI:PairingConnection"];
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var connection = ServerConnection.Parse(json);
            var port = configuration.GetValue<int?>("DDHI:DiscoveryPort") ?? LanDiscovery.DefaultPort;
            while (!stop.IsCancellationRequested)
            {
                try { await LanDiscovery.ServeAsync(connection, port, stop); }
                catch (OperationCanceledException) when (stop.IsCancellationRequested) { return; }
                catch (Exception ex) { log.LogWarning("Local server discovery unavailable: {Type}", ex.GetType().Name); }
                if (!stop.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(10), stop);
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { log.LogWarning("Discovery configuration could not be read: {Type}", ex.GetType().Name); }
    }
}
