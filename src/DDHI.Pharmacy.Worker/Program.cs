using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
var builder=Host.CreateApplicationBuilder(args);
var protectedConfig=Environment.GetEnvironmentVariable("DDHI_CONFIG_FILE");
if(!string.IsNullOrWhiteSpace(protectedConfig))builder.Configuration.AddJsonFile(protectedConfig,false,true);
ProtectedServiceConfiguration.Apply(builder.Configuration);
builder.Services.AddWindowsService(o=>o.ServiceName="DDHIPharmacyWorker");
builder.Services.AddDbContext<PharmacyDb>(o=>o.UseNpgsql(builder.Configuration.GetConnectionString("Pharmacy")??throw new InvalidOperationException("Configure ConnectionStrings__Pharmacy.")));
builder.Services.AddHostedService<MaintenanceWorker>();builder.Services.AddHostedService<DiscoveryWorker>();await builder.Build().RunAsync();
sealed class MaintenanceWorker(IServiceScopeFactory scopes,ILogger<MaintenanceWorker> log):BackgroundService {
 protected override async Task ExecuteAsync(CancellationToken stop){using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));do{try{await using var scope=scopes.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();await using var tx=await db.Database.BeginTransactionAsync(stop);await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(772991001)",stop);await db.StockReservations.Where(x=>!x.Released&&x.ExpiresAt<DateTime.UtcNow).ExecuteUpdateAsync(x=>x.SetProperty(r=>r.Released,true),stop);await tx.CommitAsync(stop);}catch(Exception ex) when(!stop.IsCancellationRequested){log.LogError("Maintenance failed: {Type}",ex.GetType().Name);}}while(await timer.WaitForNextTickAsync(stop));}
}
