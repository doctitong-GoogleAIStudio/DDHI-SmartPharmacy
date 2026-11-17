namespace DDHI.Pharmacy.Contracts;
public sealed record ScanMedicineInfo(Guid Id,string GenericName,string BrandName,string Strength,string DosageForm,string Route,string Manufacturer,string BaseUnit,bool Active);
public sealed record ScanPackageInfo(string Packaging,decimal BaseUnitsPerPackage,string BaseUnit);
public sealed record ScanStockInfo(Guid Id,string Lot,DateOnly Expiration,string Location,string Status,decimal Quantity,string BaseUnit,bool Recalled,bool Expired);
public sealed record ScanLookupResult(string Status,string Message,string Raw,string Symbology,string? Gtin,string? EncodedLot,DateOnly? EncodedExpiration,string? Serial,ScanMedicineInfo? Medicine,ScanPackageInfo? Package,IReadOnlyList<ScanStockInfo> Stock,IReadOnlyList<string> Warnings);
public sealed record UserEditInput(string Email,string FullName,string PhoneNumber,string[] Roles,string Reason,bool Confirmed,string Version);
public sealed record UserDeleteInput(string ConfirmationEmail,string Reason,bool Confirmed,string Version);
