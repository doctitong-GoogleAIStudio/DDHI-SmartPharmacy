using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Application;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;
public sealed class ExternalReturnService(PharmacyDb db)
{
    public async Task<object> Record(ExternalReturnInput input,Actor actor)
    {
        Safety.Require(Roles.Clinical.Contains(actor.Role),"Pharmacist authorization required.");
        Safety.Require(input.Confirmed&&input.NoHospitalCreditAcknowledged,"Confirm that outside/uncertain items do not create hospital stock or a refund credit.");
        string Text(string? value,int limit,bool required=false){var text=value?.Trim()??"";Safety.Require(text.Length<=limit&&!text.Any(char.IsControl)&&(!required||text.Length>0),"Complete the intake fields with bounded, single-line values.");return text;}
        var reference=Text(input.Reference,200,true);Safety.Require(!await db.ExternalReturnIntakes.AnyAsync(x=>x.Reference==reference),"Duplicate external intake reference. Use a unique intake-line reference.");
        Safety.Require(input.Source is "Outside pharmacy" or "Uncertain/mixed","Hospital-issued returns must use the original hospital dispensing line with verified source evidence.");
        Safety.Positive(input.Quantity);Safety.Require(decimal.Round(input.Quantity,4)==input.Quantity,"Use at most four decimal places for intake quantity.");
        Safety.Require(input.PackageCondition is "Sealed/intact" or "Opened" or "Damaged" or "Unknown","Select package condition.");
        Safety.Require(input.CustodyDisposition is "Held separately pending review" or "Returned to patient/family" or "Disposal referred for authorization","Select a documented custody/disposition decision.");
        var item=new ExternalReturnIntake{Reference=reference,EncounterReference=Text(input.EncounterReference,200,true),Source=input.Source,ItemDescription=Text(input.ItemDescription,500,true),Quantity=input.Quantity,Unit=Text(input.Unit,40,true),PackageCondition=input.PackageCondition,Lot=Text(input.Lot,200),Expiration=input.Expiration,Barcode=Text(input.Barcode,256),ReceiptReference=Text(input.ReceiptReference,200),CustodyDisposition=input.CustodyDisposition,Notes=Text(input.Notes,500,true),RecordedBy=actor.UserId};
        db.ExternalReturnIntakes.Add(item);return new{item.Id,StockEffect="None",BillingEffect="None",item.CustodyDisposition};
    }
}
