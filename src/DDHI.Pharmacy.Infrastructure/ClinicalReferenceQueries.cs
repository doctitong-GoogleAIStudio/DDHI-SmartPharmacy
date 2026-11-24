using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;

public sealed record ReferenceSearchRow(Guid Id,string Kind,string Generic,string Brand,string Strength,string Form,string Route,string Manufacturer,string Mapping,string Match);
public sealed class ClinicalReferenceQueries(PharmacyDb db)
{
    public async Task<object> Status()
    {
        var content=new ClinicalContentService(db);var current=await content.Current();var package=await content.Published();
        var mapped=await db.DrugClinicalMappings.CountAsync(x=>x.Product.VersionId==(current==null?Guid.Empty:current.VersionId));
        return new{DatabaseVersion=current?.Version.Version??"No published clinical dataset",PublicationRevision=current?.Revision??0,LastPublished=current?.CreatedAt,Ingredients=package?.Ingredients.Length??0,Monographs=package?.Monographs.Length??0,InteractionRules=package?.Rules.Length??0,MappedInventory=mapped,UnmappedInventory=await db.Medicines.CountAsync()-mapped,
            OverdueMonographs=package?.Monographs.Count(x=>x.ReviewDue<Safety.Today)??0,OverdueRules=package?.Rules.Count(x=>x.ReviewDue<Safety.Today)??0,Coverage="Limited / not comprehensive. Missing content and context remain UNKNOWN.",ExternalAI="Disabled. No patient information is transmitted to an external AI service.",ClinicalValidation="Hospital clinical validation and source licensing are required before clinical reliance.",Disclaimer=DrugReferenceSections.Disclaimer};
    }
    public async Task<object> Search(string? query,bool stockOnly,string? directory=null)
    {
        var q=(query??"").Trim().ToLowerInvariant();if(string.IsNullOrEmpty(directory))directory=null;Safety.Require(q.Length<=120,"Search text is too long.");var current=await new ClinicalContentService(db).Current();var package=await new ClinicalContentService(db).Published();
        var mapped=await db.DrugClinicalMappings.AsNoTracking().Include(x=>x.Product).Where(x=>x.Product.VersionId==(current==null?Guid.Empty:current.VersionId)).ToDictionaryAsync(x=>x.MedicineId);
        var barcodes=await db.MedicineBarcodes.AsNoTracking().Where(x=>q!=""&&x.Barcode==q).Select(x=>x.MedicineId).ToListAsync();
        var sourceDrugs=(package?.Drugs??[]).ToDictionary(x=>x.Key);
        string IngredientWords(ClinicalDrugDefinition? drug)=>drug==null?"":string.Join(' ',drug.Ingredients.Select(i=>{var ingredient=package!.Ingredients.Single(x=>x.Key==i.Ingredient);return ingredient.Key+" "+ingredient.Name+" "+string.Join(' ',ingredient.Synonyms);}));
        var items=new List<(ReferenceSearchRow Row,string Search)>();
        if(directory is null or "inventory" or "formulary"){
            var meds=await db.Medicines.AsNoTracking().Where(m=>!stockOnly||db.InventoryBalances.Any(b=>b.Batch.MedicineId==m.Id&&b.Status==StockStatus.Available&&b.Quantity>0&&b.Batch.Expiration>=Safety.Today&&!db.RecallAffectedBatchs.Any(r=>r.BatchId==b.BatchId))).OrderBy(m=>m.GenericName).Take(5001).ToListAsync();
            foreach(var m in meds){var mapping=mapped.GetValueOrDefault(m.Id);var product=mapping==null?null:package?.Products.Single(p=>p.Key==mapping.Product.Key);var drug=product==null?null:sourceDrugs[product.ClinicalDrug];
                var search=string.Join(' ',new[]{m.GenericName,m.BrandName,m.Strength,m.DosageForm,m.Route,m.Manufacturer,product?.RegistrationNumber??"",drug==null?"":string.Join(' ',drug.Synonyms.Concat(drug.Classes)),IngredientWords(drug),barcodes.Contains(m.Id)?q:""}).ToLowerInvariant();
                if(directory=="formulary"&&(drug==null||!package!.Monographs.Any(x=>x.ClinicalDrug==drug.Key&&x.Sections.Any(s=>s.Heading=="Formulary"))))continue;
                items.Add((new(m.Id,"Inventory",m.GenericName,m.BrandName,m.Strength,m.DosageForm,m.Route,m.Manufacturer,mapping==null?"Unmapped / not verified for current version":"Verified current mapping","Exact / partial"),search));
            }
        }
        if(!stockOnly&&current!=null){
            if(directory is null or "drugs" or "classes")foreach(var d in await db.ClinicalDrugs.AsNoTracking().Where(x=>x.VersionId==current.VersionId).ToListAsync()){
                var def=sourceDrugs[d.Key];var mono=package!.Monographs.SingleOrDefault(x=>x.ClinicalDrug==d.Key);var search=string.Join(' ',new[]{d.SearchText,string.Join(' ',def.Synonyms),IngredientWords(def),mono==null?"":string.Join(' ',mono.Sections.Where(s=>s.Heading=="ApprovedIndications").Select(s=>s.Text))});
                items.Add((new(d.Id,"Clinical drug",d.Name,"",d.Strength,d.Form,d.Route,"","Published reference; not an inventory record","Exact / partial"),search.ToLowerInvariant()));}
            if(directory is null or "products")foreach(var product in await db.ClinicalProducts.AsNoTracking().Include(x=>x.Drug).Where(x=>x.VersionId==current.VersionId).ToListAsync()){
                var definition=package!.Products.Single(x=>x.Key==product.Key);var packageCodes=string.Join(' ',package.Packages.Where(x=>x.Product==product.Key).Select(x=>x.Barcode));items.Add((new(product.Id,"Reference product",product.Drug.Name,product.Brand,product.Drug.Strength,product.Drug.Form,product.Drug.Route,definition.Manufacturer,"Published reference; PH registration requires separate verification","Exact / partial"),(product.SearchText+" "+product.Drug.SearchText+" "+definition.RegistrationNumber+" "+packageCodes).ToLowerInvariant()));}
        }
        bool Match(string text)=>q==""||q.Split(' ',StringSplitOptions.RemoveEmptyEntries).All(text.Contains);
        var matches=items.Where(x=>Match(x.Search)).Select(x=>x.Row).ToList();var suggestion=false;
        if(matches.Count==0&&q.Length>=4){suggestion=true;matches=items.Where(x=>x.Search.Split(' ',StringSplitOptions.RemoveEmptyEntries).Any(w=>Distance(q,w)<=2)).Select(x=>x.Row with{Match="Spelling suggestion - verify exact product"}).ToList();}
        return new{Rows=matches.Take(200),Total=matches.Count,Limited=matches.Count>200||items.Count>=5000,Suggestions=suggestion,StockMeaning="Available-status, unexpired, non-recalled stock filter; reservation/quantity checks still apply at dispensing."};
    }
    static int Distance(string a,string b){if(Math.Abs(a.Length-b.Length)>2||a.Length>120||b.Length>120)return 3;var row=Enumerable.Range(0,b.Length+1).ToArray();for(int i=1;i<=a.Length;i++){var previous=row[0];row[0]=i;for(int j=1;j<=b.Length;j++){var old=row[j];row[j]=Math.Min(Math.Min(row[j]+1,row[j-1]+1),previous+(a[i-1]==b[j-1]?0:1));previous=old;}}return row[^1];}
    public async Task<object> Monograph(Guid id)
    {
        var content=new ClinicalContentService(db);var current=await content.Current();var package=await content.Published();var medicine=await db.Medicines.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id);
        var mapping=medicine==null?null:await db.DrugClinicalMappings.AsNoTracking().Include(x=>x.Product).ThenInclude(x=>x.Drug).SingleOrDefaultAsync(x=>x.MedicineId==id);
        var product=await db.ClinicalProducts.AsNoTracking().Include(x=>x.Drug).SingleOrDefaultAsync(x=>x.Id==id&&x.VersionId==(current==null?Guid.Empty:current.VersionId));
        var drug=product?.Drug??(mapping?.Product.VersionId==current?.VersionId?mapping?.Product.Drug:null)??await db.ClinicalDrugs.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.VersionId==(current==null?Guid.Empty:current.VersionId));
        Safety.Require(medicine!=null||drug!=null,"Reference item is unavailable in the current publication.");
        var definition=drug==null?null:package?.Drugs.SingleOrDefault(x=>x.Key==drug.Key);var mono=drug==null?null:package?.Monographs.SingleOrDefault(x=>x.ClinicalDrug==drug.Key);
        var inventoryIds=drug==null?(medicine==null?Array.Empty<Guid>():[medicine.Id]):await db.DrugClinicalMappings.Where(x=>x.Product.DrugId==drug.Id).Select(x=>x.MedicineId).ToArrayAsync();
        var stock=await db.InventoryBalances.AsNoTracking().Where(b=>inventoryIds.Contains(b.Batch.MedicineId)).OrderBy(b=>b.Batch.Expiration).Select(b=>new{b.Id,b.Batch.MedicineId,b.Batch.Medicine.GenericName,b.Batch.Medicine.BrandName,b.Batch.Lot,b.Batch.Expiration,Location=b.Location.Name,b.Status,b.Quantity,Reserved=db.StockReservations.Where(r=>r.BalanceId==b.Id&&!r.Released&&r.ExpiresAt>DateTime.UtcNow).Sum(r=>r.Quantity),Recalled=db.RecallAffectedBatchs.Any(r=>r.BatchId==b.BatchId)}).Take(1000).ToListAsync();
        return new{Inventory=medicine,Drug=definition,Product=product==null?mapping==null||drug==null?null:package?.Products.SingleOrDefault(x=>x.Key==mapping.Product.Key):package?.Products.SingleOrDefault(x=>x.Key==product.Key),Monograph=mono,
            SourceReferences=package?.Sources??[],DatabaseVersion=current?.Version.Version??"None",ReviewingPharmacistId=current?.Version.Reviewer,ApprovingChiefPharmacistId=current?.Version.Approver,ApprovalStatus=mono==null?"No approved monograph":mono.ReviewDue<Safety.Today?"Published - review overdue":"Published",Mapping=medicine==null?"Published clinical reference":drug==null?"Unmapped or old-version mapping":"Verified current inventory mapping",
            MissingSections=DrugReferenceSections.MonographFields.Except(mono?.Sections.Select(s=>s.Heading)??[]),Stock=stock,SellingPrice="Not configured; inventory unit cost is not a selling price.",RegulatoryWarning="Hospital formulary and PH FDA registration are separate. No registration or availability is inferred from international terminology.",Disclaimer=DrugReferenceSections.Disclaimer};
    }
}
