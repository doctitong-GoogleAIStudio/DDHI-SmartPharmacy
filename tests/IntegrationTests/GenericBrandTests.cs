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
    [Fact] public async Task GenericGroupsShowSeveralBrandsWithoutMergingProducts()
    {
        var generic="QA Generic "+Guid.NewGuid();var source=await Post(chief,"medicines",new MedicineInput(generic,"Brand A","5 mg","Tablet","Oral","Maker A","QA","tablet",1));var id=source.GetProperty("Id").GetGuid();
        var added=await Post(chief,"medicines/"+id+"/brands",new BrandInput("Brand B","Maker B","Importer B","Distributor B",2,"QA catalog approval",true));var other=added.GetProperty("Id").GetGuid();Assert.NotEqual(id,other);Assert.Equal(generic,added.GetProperty("GenericName").GetString());Assert.Equal("5 mg",added.GetProperty("Strength").GetString());Assert.Equal("tablet",added.GetProperty("BaseUnit").GetString());
        var groups=await reader.GetFromJsonAsync<JsonElement>("/api/medicines/generics");var group=groups.EnumerateArray().Single(x=>x.GetProperty("genericName").GetString()==generic);Assert.Equal(2,group.GetProperty("brandCount").GetInt32());Assert.Equal(2,group.GetProperty("productCount").GetInt32());
        var brands=await reader.GetFromJsonAsync<JsonElement>("/api/medicines/"+id+"/brands");Assert.Equal(2,brands.GetArrayLength());Assert.Contains(brands.EnumerateArray(),x=>x.GetProperty("id").GetGuid()==other&&x.GetProperty("importedBy").GetString()=="Importer B");
        using var scope=factory.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<PharmacyDb>();Assert.False(await db.MedicineBarcodes.AnyAsync(x=>x.MedicineId==other));Assert.False(await db.InventoryBatchs.AnyAsync(x=>x.MedicineId==other));
    }
    [Fact] public async Task GenericGroupingDoesNotEquateDifferentStrengthFormRouteOrUnits()
    {
        var name="QA Group "+Guid.NewGuid();var original=new MedicineInput(name,"A","5 mg","Tablet","Oral","Maker","QA","tablet",0);var first=await Post(chief,"medicines",original);var id=first.GetProperty("Id").GetGuid();
        await Post(chief,"medicines",original with{GenericName=" "+name.ToUpperInvariant()+" ",BrandName="B",Strength=" 5   mg "});
        foreach(var input in new[]{original with{Strength="10 mg"},original with{DosageForm="Capsule"},original with{Route="Other route"},original with{BaseUnit="box"}})await Post(chief,"medicines",input);
        var members=await reader.GetFromJsonAsync<JsonElement>("/api/medicines/"+id+"/brands");Assert.Equal(2,members.GetArrayLength());
        var groups=await reader.GetFromJsonAsync<JsonElement>("/api/medicines/generics");Assert.Equal(5,groups.EnumerateArray().Count(x=>x.GetProperty("genericName").GetString()!.Trim().Equals(name,StringComparison.OrdinalIgnoreCase)));
    }
    [Fact] public async Task BrandCreationRequiresCatalogApprovalAndDispensingKeepsExactBrand()
    {
        var source=await Stock(5);var input=new BrandInput("Second QA brand","QA maker","","",0,"QA approved",true);var path="medicines/"+source.Medicine+"/brands";
        Assert.Equal(HttpStatusCode.Forbidden,(await Raw(reader,path,input)).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,input with{Confirmed=false})).StatusCode);
        var brand=await Post(chief,path,input);var id=brand.GetProperty("Id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,path,input with{BrandName=" second qa brand ",Manufacturer=" qa MAKER "})).StatusCode);
        await Policy();var order=await Order(id);Assert.Equal(HttpStatusCode.Conflict,(await Raw(chief,"dispensing",new DispenseInput(order,source.Balance,1,"","ManualSelection",true,true,"QA wrong-brand check"))).StatusCode);Assert.Equal(5,await Quantity(source.Balance));
    }
}
