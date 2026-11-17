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
    [Fact] public async Task ImporterAndDistributorPersistAndAppearInScanAndCanBeCleared()
    {
        var med=await Post(chief,"medicines",new MedicineInput("QA medicine","Brand","5 mg","Tablet","Oral","Manufacturer","QA","tablet",0," Importer Ltd ","Distributor Ltd"));
        var id=med.GetProperty("Id").GetGuid();var code=Code();await Post(chief,"barcodes",new BarcodeInput(id,code,"box",10,true));
        var station=await Post(reader,"workstations",new WorkstationInput("QA-trade-"+Guid.NewGuid()));reader.DefaultRequestHeaders.Remove("X-Workstation");reader.DefaultRequestHeaders.Add("X-Workstation",station.GetProperty("Id").GetGuid().ToString());
        var scan=(await Post(reader,"scan",new{barcode=code})).GetProperty("result").GetProperty("Medicine");Assert.Equal("Importer Ltd",scan.GetProperty("ImportedBy").GetString());Assert.Equal("Distributor Ltd",scan.GetProperty("DistributedBy").GetString());
        await Post(chief,"medicines/"+id+"/trade",new MedicineTradeInput("Updated importer","","QA correction",true));
        var row=(await reader.GetFromJsonAsync<JsonElement>("/api/medicines")).EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==id);Assert.Equal("Updated importer",row.GetProperty("importedBy").GetString());Assert.Equal("",row.GetProperty("distributedBy").GetString());
        await Post(chief,"medicines/"+id+"/trade",new MedicineTradeInput("","","QA clear optional information",true));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var saved=await db.Medicines.SingleAsync(m=>m.Id==id);Assert.Empty(saved.ImportedBy);Assert.Empty(saved.DistributedBy);Assert.Empty(await db.InventoryBatchs.Where(b=>b.MedicineId==id).ToListAsync());
    }
    [Fact] public async Task TradeDetailsRequireCatalogPermissionConfirmationAndBoundedValues()
    {
        var stock=await Stock(10);var path="medicines/"+stock.Medicine+"/trade";var input=new MedicineTradeInput("Importer","Distributor","QA edit",true);
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,path,input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,input with{Confirmed=false})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,input with{ImportedBy=new string('a',2001)})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,input with{DistributedBy="Name\nInjected"})).StatusCode);
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var med=await db.Medicines.SingleAsync(m=>m.Id==stock.Medicine);Assert.Empty(med.ImportedBy);Assert.Empty(med.DistributedBy);Assert.Equal(10,(await db.InventoryBalances.SingleAsync(b=>b.Id==stock.Balance)).Quantity);
    }
    [Fact] public async Task ApprovedImportsAcceptBothLegacyAndExtendedTradeHeaders()
    {
        const string header="CerebroMedicineId,GenericName,BrandName,Strength,DosageForm,Route,Manufacturer,Category,BaseUnit,ReorderLevel";
        var legacy="QA-"+Guid.NewGuid();var extended="QA-"+Guid.NewGuid();
        await Post(chief,"imports/medicines",new CsvInput(header+"\n"+legacy+",QA legacy,,5 mg,Tablet,Oral,Maker,QA,tablet,0","QA approval",true));
        await Post(chief,"imports/medicines",new CsvInput(header+",ImportedBy,DistributedBy\n"+extended+",QA new,,5 mg,Tablet,Oral,Maker,QA,tablet,0,\"Importer, Ltd\",Distributor Ltd","QA approval",true));
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();var old=await db.Medicines.SingleAsync(m=>m.CerebroMedicineId==legacy);Assert.Empty(old.ImportedBy);Assert.Empty(old.DistributedBy);var added=await db.Medicines.SingleAsync(m=>m.CerebroMedicineId==extended);Assert.Equal("Importer, Ltd",added.ImportedBy);Assert.Equal("Distributor Ltd",added.DistributedBy);
    }
}
