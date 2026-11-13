using DDHI.Pharmacy.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;
public sealed class PharmacyUser : IdentityUser { }
public sealed class PharmacyDb(DbContextOptions<PharmacyDb> options) : IdentityDbContext<PharmacyUser>(options) {
 public DbSet<Medicine> Medicines => Set<Medicine>();
 public DbSet<MedicineBarcode> MedicineBarcodes => Set<MedicineBarcode>();
 public DbSet<PackagingUnit> PackagingUnits => Set<PackagingUnit>();
 public DbSet<Supplier> Suppliers => Set<Supplier>();
 public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
 public DbSet<InventoryLocation> InventoryLocations => Set<InventoryLocation>();
 public DbSet<InventoryBatch> InventoryBatchs => Set<InventoryBatch>();
 public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
 public DbSet<StockMovement> StockMovements => Set<StockMovement>();
 public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
 public DbSet<StockReservation> StockReservations => Set<StockReservation>();
 public DbSet<DispenseOrder> DispenseOrders => Set<DispenseOrder>();
 public DbSet<DispenseTransaction> DispenseTransactions => Set<DispenseTransaction>();
 public DbSet<DispenseTransactionLine> DispenseTransactionLines => Set<DispenseTransactionLine>();
 public DbSet<Return> Returns => Set<Return>();
 public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
 public DbSet<RecallNotice> RecallNotices => Set<RecallNotice>();
 public DbSet<RecallAffectedBatch> RecallAffectedBatchs => Set<RecallAffectedBatch>();
 public DbSet<CerebroMedicineMapping> CerebroMedicineMappings => Set<CerebroMedicineMapping>();
 public DbSet<CerebroOrderMapping> CerebroOrderMappings => Set<CerebroOrderMapping>();
 public DbSet<IntegrationOutbox> IntegrationOutboxs => Set<IntegrationOutbox>();
 public DbSet<IntegrationInbox> IntegrationInboxs => Set<IntegrationInbox>();
 public DbSet<IntegrationAcknowledgment> IntegrationAcknowledgments => Set<IntegrationAcknowledgment>();
 public DbSet<ReconciliationRun> ReconciliationRuns => Set<ReconciliationRun>();
 public DbSet<ReconciliationDiscrepancy> ReconciliationDiscrepancys => Set<ReconciliationDiscrepancy>();
 public DbSet<Workstation> Workstations => Set<Workstation>();
 public DbSet<ScannerConfiguration> ScannerConfigurations => Set<ScannerConfiguration>();
 public DbSet<ScannerEvent> ScannerEvents => Set<ScannerEvent>();
 public DbSet<Permission> Permissions => Set<Permission>();
 public DbSet<UserSession> UserSessions => Set<UserSession>();
 public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
 public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
 public DbSet<OperationKey> OperationKeys => Set<OperationKey>();
 protected override void OnModelCreating(ModelBuilder b) {
  base.OnModelCreating(b);
  foreach(var type in b.Model.GetEntityTypes().Where(t=>typeof(Entity).IsAssignableFrom(t.ClrType))) {
   b.Entity(type.ClrType).ToTable(type.ClrType.Name+"s");
   foreach(var p in type.GetProperties().Where(p=>p.ClrType==typeof(decimal))) {p.SetPrecision(20);p.SetScale(4);}
   foreach(var p in type.GetProperties().Where(p=>p.ClrType==typeof(string)))p.SetMaxLength(2000);
  }
  foreach(var fk in b.Model.GetEntityTypes().SelectMany(t=>t.GetForeignKeys()))fk.DeleteBehavior=DeleteBehavior.Restrict;
  b.Entity<InventoryBatch>().ToTable("InventoryBatches");
  b.Entity<RecallAffectedBatch>().ToTable("RecallAffectedBatches");
  b.Entity<ReconciliationDiscrepancy>().ToTable("ReconciliationDiscrepancies");
  b.Entity<IntegrationOutbox>().ToTable("IntegrationOutbox");
  b.Entity<IntegrationInbox>().ToTable("IntegrationInbox");
  b.Entity<Medicine>().HasIndex(x=>x.CerebroMedicineId).IsUnique();
  b.Entity<MedicineBarcode>().HasIndex(x=>x.Barcode).IsUnique();
  b.Entity<InventoryBatch>().HasIndex(x=>new{x.MedicineId,x.Lot,x.Expiration}).IsUnique();
  b.Entity<InventoryBatch>().HasIndex(x=>x.Expiration);
  b.Entity<InventoryBalance>().HasIndex(x=>new{x.BatchId,x.LocationId,x.Status}).IsUnique();
  b.Entity<InventoryBalance>().ToTable(t=>t.HasCheckConstraint("CK_Balance_Nonnegative","\"Quantity\" >= 0"));
  b.Entity<DispenseOrder>().ToTable(t=>t.HasCheckConstraint("CK_Order_Quantity","\"Quantity\" > 0 AND \"Dispensed\" >= 0 AND \"Dispensed\" <= \"Quantity\""));
  b.Entity<MedicineBarcode>().ToTable(t=>t.HasCheckConstraint("CK_Barcode_Factor","\"ConversionFactor\" > 0"));
  b.Entity<GoodsReceipt>().HasIndex(x=>new{x.SupplierId,x.Reference}).IsUnique();
  b.Entity<PurchaseOrder>().HasIndex(x=>new{x.SupplierId,x.Reference}).IsUnique();
  b.Entity<DispenseOrder>().HasIndex(x=>x.Reference).IsUnique();
  b.Entity<DispenseTransaction>().HasIndex(x=>x.Reference).IsUnique();
  b.Entity<StockMovement>().HasIndex(x=>x.Reference);
  b.Entity<StockMovement>().HasOne<StockMovement>().WithMany().HasForeignKey(x=>x.ReversesMovementId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<StockMovement>().HasIndex(x=>x.ReversesMovementId).IsUnique();
  b.Entity<InventoryLocation>().HasIndex(x=>x.Name).IsUnique();
  b.Entity<Supplier>().HasIndex(x=>x.Name).IsUnique();
  b.Entity<RecallNotice>().HasIndex(x=>x.Reference).IsUnique();
  b.Entity<RecallAffectedBatch>().HasIndex(x=>new{x.RecallId,x.BatchId}).IsUnique();
  b.Entity<IntegrationOutbox>().HasIndex(x=>x.TransactionId).IsUnique();
  b.Entity<IntegrationInbox>().HasIndex(x=>x.ExternalId).IsUnique();
  b.Entity<IntegrationAcknowledgment>().HasIndex(x=>x.OutboxId).IsUnique();
  b.Entity<CerebroMedicineMapping>().HasIndex(x=>x.ExternalId).IsUnique();
  b.Entity<CerebroOrderMapping>().HasIndex(x=>x.ExternalId).IsUnique();
  b.Entity<Workstation>().HasIndex(x=>x.Name).IsUnique();
  b.Entity<SystemSetting>().HasIndex(x=>x.Name).IsUnique();
  b.Entity<OperationKey>().HasIndex(x=>x.Key).IsUnique();
  b.Entity<ScannerConfiguration>().HasIndex(x=>new{x.WorkstationId,x.Name}).IsUnique();
  b.Entity<AuditEvent>().HasIndex(x=>x.CreatedAt);
 }
 public override Task<int> SaveChangesAsync(CancellationToken c=default) {
  foreach(var e in ChangeTracker.Entries().Where(e=>e.State is EntityState.Modified or EntityState.Deleted))
   if(e.Entity is StockMovement or AuditEvent)throw new InvalidOperationException("Ledger and audit events are immutable.");
  return base.SaveChangesAsync(c);
 }
}
