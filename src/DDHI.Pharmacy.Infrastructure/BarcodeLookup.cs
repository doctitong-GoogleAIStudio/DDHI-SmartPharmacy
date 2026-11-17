using DDHI.Pharmacy.Barcode;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;

public sealed record ResolvedBarcode(BarcodeData Data,Medicine? Medicine,MedicineBarcode? Mapping,InventoryBalance? InternalStock,ScanLookupResult Result);
public sealed class BarcodeLookup(PharmacyDb db)
{
    public async Task<ResolvedBarcode> Resolve(string input)
    {
        BarcodeData data;
        try { data=BarcodeParser.Parse(input); }
        catch(FormatException) when(BarcodeIdentity.PlainCode128Candidate(input.TrimEnd('\r','\n')) &&
            db.MedicineBarcodes.Any(x=>x.Barcode==input.TrimEnd('\r','\n')&&x.ApprovedBy!="")) {
            data=BarcodeParser.Parse("]C0"+input.TrimEnd('\r','\n'));
        }
        var warnings=new List<string>();Medicine? medicine=null;MedicineBarcode? mapping=null;InventoryBalance? internalStock=null;
        ResolvedBarcode Missing(string status,string message)=>new(data,null,null,null,new(status,message,data.Raw,data.Symbology,data.Gtin,data.Lot,data.Expiration,data.Serial,null,null,[],[]));
        if(data.Raw.StartsWith("DDHI-STOCK-",StringComparison.Ordinal)) {
            if(!Guid.TryParseExact(data.Raw[11..],"N",out var id))return Missing("NotFound","This internal stock label is invalid. No medicine was selected.");
            internalStock=await db.InventoryBalances.Include(x=>x.Batch).ThenInclude(x=>x.Medicine).Include(x=>x.Location).SingleOrDefaultAsync(x=>x.Id==id);
            if(internalStock==null)return Missing("NotFound","This stock label has no matching balance in this server database.");
            medicine=internalStock.Batch.Medicine;
        } else {
            var codes=BarcodeIdentity.EquivalentCodes(data);
            var matches=await db.MedicineBarcodes.Include(x=>x.Medicine).Where(x=>codes.Contains(x.Barcode)&&x.ApprovedBy!="").ToListAsync();
            if(matches.Count==0)return Missing("NotRegistered","Barcode read successfully, but no approved package mapping was found. Ask the Chief Pharmacist or Pharmacy Administrator to register this exact package.");
            if(matches.Select(x=>new{x.MedicineId,x.ConversionFactor,Packaging=x.Packaging.Trim().ToUpperInvariant()}).Distinct().Count()!=1)
                return Missing("Ambiguous","Conflicting medicine or package mappings exist for this barcode. Catalog staff must resolve them before use. No medicine was selected.");
            mapping=matches.OrderByDescending(x=>x.Barcode==data.Gtin).ThenBy(x=>x.Id).First();medicine=mapping.Medicine;
        }
        var query=db.InventoryBalances.AsNoTracking().Where(x=>x.Batch.MedicineId==medicine.Id);
        if(internalStock!=null)query=query.Where(x=>x.Id==internalStock.Id);
        else {
            if(data.Lot!=null)query=query.Where(x=>x.Batch.Lot==data.Lot);
            if(data.Expiration!=null)query=query.Where(x=>x.Batch.Expiration==data.Expiration);
        }
        var stock=await query.OrderBy(x=>x.Batch.Expiration).ThenBy(x=>x.Id).Select(x=>new ScanStockInfo(x.Id,x.Batch.Lot,x.Batch.Expiration,x.Location.Name,x.Status.ToString(),x.Quantity,medicine.BaseUnit,
            db.RecallAffectedBatchs.Any(r=>r.BatchId==x.BatchId),x.Batch.Expiration<Safety.Today)).Take(2001).ToListAsync();
        if(stock.Count>2000){stock.RemoveAt(2000);warnings.Add("Showing the first 2,000 stock balances. Use Inventory Management for additional records.");}
        if(!medicine.ActiveStatus)warnings.Add("This medicine is inactive. Scanning does not authorize its use.");
        if(data.Expiration<Safety.Today)warnings.Add("The expiration encoded on this package is in the past.");
        if((data.Lot!=null||data.Expiration!=null)&&stock.Count==0)warnings.Add("No recorded stock matches the batch/expiration encoded on this package. Verify the package and receiving records.");
        if(stock.Any(x=>x.Expired))warnings.Add("Recorded expired stock is present; it is not eligible for dispensing.");
        if(stock.Any(x=>x.Recalled))warnings.Add("Recorded recalled stock is present; it is not eligible for dispensing.");
        var package=mapping==null?new ScanPackageInfo("Internal stock balance",1,medicine.BaseUnit):new ScanPackageInfo(mapping.Packaging,mapping.ConversionFactor,medicine.BaseUnit);
        var info=new ScanMedicineInfo(medicine.Id,medicine.GenericName,medicine.BrandName,medicine.Strength,medicine.DosageForm,medicine.Route,medicine.Manufacturer,medicine.BaseUnit,medicine.ActiveStatus,medicine.ImportedBy,medicine.DistributedBy);
        return new(data,medicine,mapping,internalStock,new("Matched","Registered medicine/package found. Confirm the physical label; scanning does not receive or dispense stock.",data.Raw,data.Symbology,data.Gtin,data.Lot,data.Expiration,data.Serial,info,package,stock,warnings));
    }
}
