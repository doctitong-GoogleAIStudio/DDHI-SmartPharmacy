using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
public partial class WorkflowTests
{
    async Task<(Guid Medicine,Guid Balance,Guid Batch)> Alternate((Guid Medicine,Guid Balance,Guid Batch,Guid Location) original)
    {
        var brand=await Post(chief,"medicines/"+original.Medicine+"/brands",new BrandInput("Alternate "+Guid.NewGuid(),"QA maker","","",0,"QA approval",true));var id=brand.GetProperty("Id").GetGuid();
        var supplier=await Post(chief,"suppliers",new NamedInput("QA supplier "+Guid.NewGuid()));
        var receipt=await Post(chief,"receiving",new ReceiveInput(id,original.Location,supplier.GetProperty("Id").GetGuid(),Guid.NewGuid().ToString(),"ALT-LOT",Safety.Today.AddDays(365),5,1,0,true));
        return(id,receipt.GetProperty("BalanceId").GetGuid(),receipt.GetProperty("Id").GetGuid());
    }
    [Fact] public async Task PharmacistCanDocumentAlternateBrandWhenOriginalIsOutOfStock()
    {
        await Policy();var original=await Stock(1);await Post(chief,"counts",new CountInput(original.Balance,0,Guid.NewGuid().ToString(),"QA original unavailable",true));var alternative=await Alternate(original);var order=await Order(original.Medicine);
        var code=Code();await Post(chief,"barcodes",new BarcodeInput(alternative.Medicine,code,"unit",1,true));
        var input=new DispenseInput(order,alternative.Balance,1,code,"ManualBarcode",true,true,"QA manual verification",true,"QA order authorization","Original brand unavailable");
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",input with{ApproveBrandSubstitution=false})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",input with{SubstitutionApprovalReference=""})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"dispensing",input)).StatusCode);
        var result=await Post(chief,"dispensing",input);Assert.True(result.GetProperty("BrandSubstituted").GetBoolean());Assert.Equal(0,await Quantity(original.Balance));Assert.Equal(4,await Quantity(alternative.Balance));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var tx=await db.DispenseTransactions.SingleAsync(x=>x.Id==result.GetProperty("Id").GetGuid());Assert.Equal(original.Medicine,tx.RequestedMedicineId);Assert.Equal(alternative.Medicine,tx.DispensedMedicineId);Assert.Equal("QA order authorization",tx.SubstitutionApprovalReference);Assert.Equal(original.Medicine,(await db.DispenseOrders.SingleAsync(x=>x.Id==order)).MedicineId);
        var line=await db.DispenseTransactionLines.SingleAsync(x=>x.TransactionId==tx.Id);await Post(chief,"returns",new ReturnInput(line.Id,1,Guid.NewGuid().ToString(),"QA unused on discharge",true,true,"QA discharge","Sealed/intact",true,true,"QA hospital issue evidence"));
        Assert.Equal(0,await Quantity(original.Balance));Assert.Equal(4,await Quantity(alternative.Balance));Assert.Equal(1,await db.InventoryBalances.Where(x=>x.BatchId==alternative.Batch&&x.Status==StockStatus.Quarantined).SumAsync(x=>x.Quantity));
    }
    [Theory][InlineData("generic")][InlineData("strength")][InlineData("form")][InlineData("route")][InlineData("unit")][InlineData("strength-unit-case")][InlineData("base-unit-case")]
    public async Task SubstitutionNeverConvertsDifferentRecordedDefinitions(string field)
    {
        await Policy();var source=await Stock(2);var alternative=await Alternate(source);var order=await Order(source.Medicine);
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var med=await db.Medicines.SingleAsync(x=>x.Id==alternative.Medicine);if(field=="generic")med.GenericName="Different generic";if(field=="strength")med.Strength="Different strength";if(field=="form")med.DosageForm="Different form";if(field=="route")med.Route="Different route";if(field=="unit")med.BaseUnit="Different unit";if(field=="strength-unit-case")med.Strength=med.Strength.Replace("mg","Mg");if(field=="base-unit-case")med.BaseUnit=med.BaseUnit.ToUpperInvariant();await db.SaveChangesAsync();}
        var response=await Raw(chief,"dispensing",new DispenseInput(order,alternative.Balance,1,"","ManualSelection",true,true,"QA verification",true,"QA approval","QA mismatch"));Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(5,await Quantity(alternative.Balance));Assert.Equal(2,await Quantity(source.Balance));
    }
    [Fact] public async Task SubstitutionRetainsRecallExpiryBarcodeAndReservationSafety()
    {
        await Policy();var source=await Stock(2);var alt=await Alternate(source);var order=await Order(source.Medicine);var input=new DispenseInput(order,alt.Balance,1,"","ManualSelection",true,true,"QA check",true,"QA approval","Unavailable brand");
        var originalCode=Code();await Post(chief,"barcodes",new BarcodeInput(source.Medicine,originalCode,"unit",1,true));Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",input with{Barcode=originalCode,VerificationMode="ManualBarcode"})).StatusCode);
        var otherOrder=await Order(alt.Medicine);await Post(chief,"reservations",new ReserveInput(alt.Balance,otherOrder,1,true));
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",input with{Quantity=5})).StatusCode);
        await Post(chief,"recalls",new RecallInput(alt.Batch,Guid.NewGuid().ToString(),"QA recall",true));Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",input)).StatusCode);Assert.Equal(5,await Quantity(alt.Balance));
    }
    [Fact] public async Task DischargeReturnTracksPatientAndConditionWithoutReturningToAvailableStock()
    {
        await Policy();var stock=await Stock(10);var encounter="QA-encounter-"+Guid.NewGuid();var order=(await Post(chief,"orders",new OrderInput(Guid.NewGuid().ToString(),encounter,stock.Medicine,5,true))).GetProperty("Id").GetGuid();
        var dispense=await Post(chief,"dispensing",new DispenseInput(order,stock.Balance,5,"","ManualSelection",true,true,"QA approved"));Guid lineId;
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();lineId=(await db.DispenseTransactionLines.SingleAsync(x=>x.TransactionId==dispense.GetProperty("Id").GetGuid())).Id;}
        var endpoint="/api/returns/discharge-lines?encounter="+Uri.EscapeDataString(encounter);Assert.Equal(HttpStatusCode.Forbidden,(await reader.GetAsync(endpoint)).StatusCode);
        var rows=await chief.GetFromJsonAsync<JsonElement>(endpoint);Assert.Single(rows.EnumerateArray());Assert.Equal(5,rows[0].GetProperty("returnableQuantity").GetDecimal());
        var input=new ReturnInput(lineId,2,Guid.NewGuid().ToString(),"Unused medicines on discharge",true,true,"QA discharge document","Sealed/intact",true,true,"QA hospital issue evidence");
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input with{UnusedVerified=false})).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input with{DischargeReference=""})).StatusCode);
        await Post(chief,"returns",input);Assert.Equal(5,await Quantity(stock.Balance));Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input)).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input with{Reference=Guid.NewGuid().ToString(),Quantity=4})).StatusCode);
        await Post(chief,"returns",input with{Reference=Guid.NewGuid().ToString(),Quantity=3});rows=await chief.GetFromJsonAsync<JsonElement>(endpoint);Assert.Equal(0,rows[0].GetProperty("returnableQuantity").GetDecimal());
        using var verify=factory.Services.CreateScope();var data=verify.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.Equal(5,await data.InventoryBalances.Where(x=>x.BatchId==stock.Batch&&x.Status==StockStatus.Quarantined).SumAsync(x=>x.Quantity));Assert.Equal(5,(await data.DispenseOrders.SingleAsync(x=>x.Id==order)).Dispensed);Assert.Equal(2,await data.Returns.CountAsync(x=>x.DispenseLineId==lineId&&x.DischargeReturn&&x.DischargeReference=="QA discharge document"));
    }
    [Fact] public async Task ConcurrentDischargeReturnsCannotExceedOriginalDispensing()
    {
        await Policy();var stock=await Stock(5);var order=await Order(stock.Medicine);var dispense=await Post(chief,"dispensing",new DispenseInput(order,stock.Balance,1,"","ManualSelection",true,true,"QA approved"));Guid line;
        using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();line=(await db.DispenseTransactionLines.SingleAsync(x=>x.TransactionId==dispense.GetProperty("Id").GetGuid())).Id;}
        var input=new ReturnInput(line,1,Guid.NewGuid().ToString(),"QA discharge",true,true,"QA discharge","Unknown",true,true,"QA hospital issue evidence");
        var results=await Task.WhenAll(Raw(chief,"returns",input),Raw(chief,"returns",input with{Reference=Guid.NewGuid().ToString()}));Assert.Single(results,x=>x.IsSuccessStatusCode);Assert.Single(results,x=>x.StatusCode==HttpStatusCode.Conflict);Assert.Equal(4,await Quantity(stock.Balance));
    }
}
