using System.Net;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
public partial class WorkflowTests
{
    static ExternalReturnInput Outside(string source="Outside pharmacy")=>new(Guid.NewGuid().ToString(),"QA-patient-"+Guid.NewGuid(),source,"QA package - generic / brand / strength",3,"tablet","Unknown","QA lot",null,"18904083844874","QA outside receipt","Held separately pending review","Outside source physically separated from hospital items",true,true);
    [Fact] public async Task OutsideAndUncertainItemsAreDocumentedWithoutStockOrBillingCredit()
    {
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();
        var movements=await db.StockMovements.CountAsync();var stock=await db.InventoryBalances.SumAsync(x=>x.Quantity);var outbox=await db.IntegrationOutboxs.CountAsync();
        foreach(var source in new[]{"Outside pharmacy","Uncertain/mixed"}){var input=Outside(source);var result=await Post(chief,"returns/external-intakes",input);Assert.Equal("None",result.GetProperty("StockEffect").GetString());Assert.Equal("None",result.GetProperty("BillingEffect").GetString());Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns/external-intakes",input)).StatusCode);Assert.True(await db.ExternalReturnIntakes.AnyAsync(x=>x.Reference==input.Reference&&x.Source==source));}
        Assert.Equal(movements,await db.StockMovements.CountAsync());Assert.Equal(stock,await db.InventoryBalances.SumAsync(x=>x.Quantity));Assert.Equal(outbox,await db.IntegrationOutboxs.CountAsync());
    }
    [Fact] public async Task ExternalIntakeRequiresClinicalRoleAndCannotPretendToBeHospitalStock()
    {
        var input=Outside();Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,"returns/external-intakes",input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns/external-intakes",input with{Source="Hospital pharmacy"})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns/external-intakes",input with{NoHospitalCreditAcknowledged=false})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns/external-intakes",input with{Quantity=-1})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await reader.GetAsync("/api/returns/external-intakes")).StatusCode);
    }
    [Fact] public async Task HospitalReturnWithoutSourceEvidenceCannotCreateStock()
    {
        await Policy();var stock=await Stock();var order=await Order(stock.Medicine);var tx=await Post(chief,"dispensing",new DispenseInput(order,stock.Balance,1,"","ManualSelection",true,true,"QA manual approval"));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var line=await db.DispenseTransactionLines.SingleAsync(x=>x.TransactionId==tx.GetProperty("Id").GetGuid());
        var input=new ReturnInput(line.Id,1,Guid.NewGuid().ToString(),"QA origin uncertain",true);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input)).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"returns",input with{HospitalSourceVerified=true})).StatusCode);Assert.False(await db.Returns.AnyAsync(x=>x.DispenseLineId==line.Id));Assert.Equal(0,await Quantity(stock.Balance));
    }
}
