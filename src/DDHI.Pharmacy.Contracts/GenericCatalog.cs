namespace DDHI.Pharmacy.Contracts;
public sealed record GenericCatalogGroup(Guid TemplateMedicineId,string GenericName,string Strength,string DosageForm,string Route,string BaseUnit,int BrandCount,int ProductCount,int ActiveProducts);
public sealed record BrandInput(string BrandName,string Manufacturer,string ImportedBy,string DistributedBy,decimal ReorderLevel,string Reason,bool Confirmed);
