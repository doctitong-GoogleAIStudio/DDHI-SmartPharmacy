using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;
public sealed class GenericCatalog(PharmacyDb db)
{
    async Task<List<Medicine>> Catalog()=>await db.Medicines.AsNoTracking().OrderBy(m=>m.GenericName).ThenBy(m=>m.BrandName).ThenBy(m=>m.Id).ToListAsync();
    public async Task<GenericCatalogGroup[]> Groups()=>(await Catalog()).GroupBy(MedicineGrouping.Definition).Select(group=>{
        var template=group.First();return new GenericCatalogGroup(template.Id,template.GenericName,template.Strength,template.DosageForm,template.Route,template.BaseUnit,
            group.Select(m=>MedicineGrouping.Brand(m.BrandName)).Distinct(StringComparer.Ordinal).Count(),group.Count(),group.Count(m=>m.ActiveStatus));
    }).ToArray();
    public async Task<Medicine[]> Brands(Guid templateId)
    {
        var template=await db.Medicines.AsNoTracking().SingleOrDefaultAsync(m=>m.Id==templateId)??throw new InvalidOperationException("Select an existing generic group or brand product.");
        var key=MedicineGrouping.Definition(template);
        return (await Catalog()).Where(m=>MedicineGrouping.Definition(m)==key).ToArray();
    }
    public async Task<Medicine> AddBrand(Guid templateId,BrandInput input)
    {
        Safety.Require(input.Confirmed&&!string.IsNullOrWhiteSpace(input.Reason)&&input.Reason.Length<=500,"Confirm the new brand and provide a catalog approval reason (up to 500 characters).");
        string Text(string? value){var text=value?.Trim()??"";Safety.Require(text.Length<=2000&&!text.Any(char.IsControl),"Company and brand details must be at most 2000 characters without control characters.");return text;}
        var brand=Text(input.BrandName);var maker=Text(input.Manufacturer);Safety.Require(brand.Length>0,"Enter the verified brand name, or explicitly use Unbranded for an unbranded product.");
        Safety.Require(input.ReorderLevel>=0,"Reorder level cannot be negative.");
        var template=await db.Medicines.SingleOrDefaultAsync(m=>m.Id==templateId)??throw new InvalidOperationException("The selected generic group no longer exists. Refresh the catalog.");
        var members=await Brands(templateId);
        Safety.Require(!members.Any(m=>MedicineGrouping.Brand(m.BrandName)==MedicineGrouping.Brand(brand)&&MedicineGrouping.Normalize(m.Manufacturer)==MedicineGrouping.Normalize(maker)),"This brand and manufacturer already exist in the selected generic group. Select the existing product or have catalog staff review it.");
        var medicine=new Medicine{GenericName=template.GenericName,Strength=template.Strength,DosageForm=template.DosageForm,Route=template.Route,BaseUnit=template.BaseUnit,Category=template.Category,
            BrandName=brand,Manufacturer=maker,ImportedBy=Text(input.ImportedBy),DistributedBy=Text(input.DistributedBy),ReorderLevel=input.ReorderLevel};
        db.Medicines.Add(medicine);return medicine;
    }
}
