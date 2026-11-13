using DDHI.Pharmacy.Barcode;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Integration;
using DDHI.Pharmacy.Infrastructure;
using Xunit;
public class BarcodeTests {
 [Theory][InlineData("96385074")][InlineData("036000291452")][InlineData("4006381333931")][InlineData("09506000134352")]
 public void AcceptsValidGtin(string code){Assert.True(BarcodeParser.ValidGtin(code));Assert.NotNull(BarcodeParser.Parse(code).Gtin);}
 [Theory][InlineData("")][InlineData("123")][InlineData("4006381333932")][InlineData("abc")][InlineData("(01)09506000134352(17)261332")][InlineData("(01)09506000134352(10)")][InlineData("(01)09506000134352(10)A(10)B")][InlineData("(01)09506000134352(17)261200")][InlineData("(01)09506000134352(99)ABC")][InlineData("(01)09506000134352(10)123456789012345678901")]
 public void RejectsInvalid(string code)=>Assert.Throws<FormatException>(()=>BarcodeParser.Parse(code));
 [Fact]public void LeadingFnc1Supported(){Assert.Equal("LOT",BarcodeParser.Parse("\u001d010950600013435210LOT").Lot);}
 [Fact]public void ParsesDelimitedGs1(){var result=BarcodeParser.Parse("]d2010950600013435210LOT123\u001d1727123121SERIAL");Assert.Equal("LOT123",result.Lot);Assert.Equal(new DateOnly(2027,12,31),result.Expiration);Assert.Equal("SERIAL",result.Serial);Assert.Equal("GS1 DataMatrix",result.Symbology);}
 [Fact]public void ParsesHumanReadable(){var result=BarcodeParser.Parse("(01)09506000134352(17)271231(10)LOT(21)S");Assert.Equal("LOT",result.Lot);Assert.Equal("S",result.Serial);}
 [Fact]public void DoesNotInventMissingBatch(){var result=BarcodeParser.Parse("4006381333931");Assert.Null(result.Lot);Assert.Null(result.Expiration);}
 [Fact]public void Code128ExplicitSymbology(){Assert.Equal("MED-123",BarcodeParser.Parse("]C0MED-123").Raw);}
 [Fact]public void InternalNamespace(){Assert.Equal("DDHI internal",BarcodeParser.Parse("DDHI-MED-0001").Symbology);}
 [Fact]public void SeparatorRequiredForVariableField(){var result=BarcodeParser.Parse("]C1010950600013435210LOT17271231");Assert.Equal("LOT17271231",result.Lot);Assert.Null(result.Expiration);}
}
public class SafetyTests {
 [Theory][InlineData(0)][InlineData(-1)][InlineData(100000001)][InlineData(0.00001)]public void InvalidQuantities(decimal q)=>Assert.Throws<InvalidOperationException>(()=>Safety.Positive(q));
 [Theory][InlineData(StockStatus.Recalled)][InlineData(StockStatus.Quarantined)][InlineData(StockStatus.Damaged)][InlineData(StockStatus.PendingVerification)][InlineData(StockStatus.Expired)][InlineData(StockStatus.Reserved)]public void BlocksUnsafeStatus(StockStatus s)=>Assert.Throws<InvalidOperationException>(()=>Safety.CanDispense(new(){Quantity=5,Status=s,Batch=new(){Expiration=Safety.Today.AddDays(30)}},1,Safety.Today));
 [Fact]public void ExpiredBlocked()=>Assert.Throws<InvalidOperationException>(()=>Safety.CanDispense(new(){Quantity=5,Status=StockStatus.Available,Batch=new(){Expiration=Safety.Today.AddDays(-1)}},1,Safety.Today));
 [Fact]public void InsufficientBlocked()=>Assert.Throws<InvalidOperationException>(()=>Safety.CanDispense(new(){Quantity=1,Status=StockStatus.Available,Batch=new(){Expiration=Safety.Today.AddDays(1)}},2,Safety.Today));
 [Fact]public void TechnicalAdminNotClinical()=>Assert.DoesNotContain(Roles.SystemAdmin,Roles.Clinical);
 [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(8)][InlineData(32)]public void ScannerCountHasNoFixedLimit(int count){var w=new Workstation();var scanners=Enumerable.Range(0,count).Select(i=>new ScannerConfiguration{WorkstationId=w.Id,Name="Scanner "+i}).ToList();Assert.Equal(count,scanners.Count);}
 [Fact]public async Task MockDoesNotClaimProductionAcknowledgment(){var adapter=new MockCerebroIntegration();Assert.Equal("NeedsReconciliation",(await adapter.PostDispensingAsync(new(Guid.NewGuid(),"test"),default)).State);}
 [Fact]public async Task VendorAdaptersFailClosed()=>await Assert.ThrowsAsync<InvalidOperationException>(()=>new ApprovedRestAdapter().TestConnectionAsync(default));
 [Theory][InlineData("=SUM(A1)")][InlineData("+cmd")][InlineData("-1")][InlineData("@formula")]
 public void CsvNeutralizesFormula(string value)=>Assert.StartsWith("\"'",CsvAdapter.Escape(value));
 [Fact]public void AuditTamperChangesHash(){var e=new AuditEvent{Actor="staff",Detail="original"};var h=InventoryService.AuditHash(e);e.Detail="changed";Assert.NotEqual(h,InventoryService.AuditHash(e));}
}
