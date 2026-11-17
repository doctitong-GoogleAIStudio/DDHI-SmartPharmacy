using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.EntityFrameworkCore;
static class PatientReturns
{
    public static void Map(RouteGroupBuilder api)
    {
        api.MapGet("/returns/discharge-lines",async(string encounter,PharmacyDb db)=>{
            Safety.Require(!string.IsNullOrWhiteSpace(encounter)&&encounter.Trim().Length<=200&&!encounter.Any(char.IsControl),"Enter the exact patient/encounter reference from the original order (up to 200 characters).");
            var reference=encounter.Trim();
            var rows=await db.DispenseTransactionLines.AsNoTracking().Where(x=>x.Transaction.Order.EncounterReference==reference).OrderByDescending(x=>x.CreatedAt).Select(x=>new{
                x.Id,x.TransactionId,x.Transaction.Order.EncounterReference,OrderReference=x.Transaction.Order.Reference,
                Medicine=x.Balance.Batch.Medicine.GenericName,Brand=x.Balance.Batch.Medicine.BrandName,x.Balance.Batch.Medicine.Strength,x.Balance.Batch.Medicine.DosageForm,x.Balance.Batch.Medicine.BaseUnit,
                x.Balance.Batch.Lot,x.Balance.Batch.Expiration,Dispensed=x.Quantity,Returned=db.Returns.Where(r=>r.DispenseLineId==x.Id).Sum(r=>r.Quantity),
                ReturnableQuantity=x.Quantity-db.Returns.Where(r=>r.DispenseLineId==x.Id).Sum(r=>r.Quantity),x.CreatedAt
            }).Take(2001).ToListAsync();Safety.Require(rows.Count<=2000,"Too many dispensing lines for this reference. Ask authorized IT to review the encounter reference.");return rows;
        }).RequireAuthorization("Clinical");
        api.MapGet("/returns/external-intakes",async(PharmacyDb db)=>await db.ExternalReturnIntakes.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Select(x=>new{x.Id,x.Reference,x.EncounterReference,x.Source,x.ItemDescription,x.Quantity,x.Unit,x.PackageCondition,x.Lot,x.Expiration,x.Barcode,x.ReceiptReference,x.CustodyDisposition,x.Notes,x.RecordedBy,x.CreatedAt,StockEffect="None",BillingEffect="None"}).Take(20000).ToListAsync()).RequireAuthorization("Clinical");
        api.MapGet("/returns/history",async(PharmacyDb db)=>await db.Returns.AsNoTracking().OrderByDescending(r=>r.CreatedAt).Select(r=>new{
            r.Id,r.Reference,r.DischargeReturn,r.DischargeReference,r.PackageCondition,r.UnusedVerified,r.HospitalSourceVerified,r.HospitalEvidenceReference,r.DispenseLineId,
            r.DispenseLine.Transaction.Order.EncounterReference,OrderReference=r.DispenseLine.Transaction.Order.Reference,
            Medicine=r.DispenseLine.Balance.Batch.Medicine.GenericName,Brand=r.DispenseLine.Balance.Batch.Medicine.BrandName,
            r.DispenseLine.Balance.Batch.Lot,r.Quantity,r.Reason,r.MovementId,r.CreatedAt
        }).Take(20000).ToListAsync()).RequireAuthorization("Clinical");
    }
}
