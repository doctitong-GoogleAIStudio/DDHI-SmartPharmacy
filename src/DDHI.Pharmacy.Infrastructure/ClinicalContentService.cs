using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;

public sealed class ClinicalContentService(PharmacyDb db)
{
    static string SearchText(params string[] values){var text=string.Join(' ',values).ToLowerInvariant();return text.Length>400?text[..400]:text;}
    public async Task<ClinicalPublication?> Current()=>await db.ClinicalPublications.AsNoTracking().Include(x=>x.Version).OrderByDescending(x=>x.Revision).FirstOrDefaultAsync();
    public async Task<ClinicalPackage?> Published(){var current=await Current();if(current==null)return null;Safety.Require(InventoryService.Hash(current.Version.PayloadJson)==current.Version.Digest,"Published content checksum failed. Clinical reference unavailable.");return ClinicalPackageValidation.Parse(current.Version.PayloadJson);}
    public object Preview(PackageImport input)
    {
        var package=ClinicalPackageValidation.Parse(input.Json);var hash=InventoryService.Hash(input.Json);
        Safety.Require(string.Equals(input.Sha256,hash,StringComparison.OrdinalIgnoreCase),"Package checksum mismatch. Nothing imported.");
        return new{package.Version,Sha256=hash,Ingredients=package.Ingredients.Length,Drugs=package.Drugs.Length,Products=package.Products.Length,Packages=package.Packages.Length,Monographs=package.Monographs.Length,Rules=package.Rules.Length,Sources=package.Sources.Length,
            Warning="Preview only. Source permissions, clinical accuracy and completeness need independent hospital review. No automatic clinical approval."};
    }
    public async Task<object> Stage(PackageImport input,Actor actor)
    {
        Preview(input);Safety.Require(input.RightsConfirmed&&!string.IsNullOrWhiteSpace(input.ApprovalReference)&&input.ApprovalReference.Length<=500,"Confirm permitted use and provide the import authorization reference.");
        var p=ClinicalPackageValidation.Parse(input.Json);Safety.Require(!await db.ClinicalContentVersions.AnyAsync(x=>x.Version==p.Version),"Version already staged; create a distinct version for edits.");
        var v=new ClinicalContentVersion{Version=p.Version,Digest=InventoryService.Hash(input.Json),PayloadJson=input.Json,Author=actor.UserId,ApprovalReference=input.ApprovalReference};db.ClinicalContentVersions.Add(v);
        foreach(var x in p.Sources)db.ClinicalSourceRecords.Add(new(){VersionId=v.Id,Key=x.Key,Organization=x.Organization,Document=x.Document,Url=x.Url,LicenseBasis=x.LicenseBasis,RevisionDate=x.RevisionDate,RetrievedDate=x.RetrievedDate});
        var ingredients=p.Ingredients.ToDictionary(x=>x.Key,x=>new DrugIngredient{VersionId=v.Id,Key=x.Key,Name=x.Name,SearchText=SearchText(x.Name,string.Join(' ',x.Synonyms))});db.DrugIngredients.AddRange(ingredients.Values);
        var drugs=p.Drugs.ToDictionary(x=>x.Key,x=>new ClinicalDrug{VersionId=v.Id,Key=x.Key,Name=x.Name,Strength=x.Strength,Form=x.Form,Route=x.Route,SearchText=SearchText(x.Name,string.Join(' ',x.Synonyms),string.Join(' ',x.Classes))});db.ClinicalDrugs.AddRange(drugs.Values);
        foreach(var drug in p.Drugs)foreach(var x in drug.Ingredients)db.ClinicalDrugIngredients.Add(new(){DrugId=drugs[drug.Key].Id,IngredientId=ingredients[x.Ingredient].Id,Amount=x.Amount,Unit=x.Unit});
        var products=p.Products.ToDictionary(x=>x.Key,x=>new ClinicalProduct{VersionId=v.Id,DrugId=drugs[x.ClinicalDrug].Id,Key=x.Key,Brand=x.Brand,SearchText=SearchText(x.Brand,x.Manufacturer,x.RegistrationNumber)});db.ClinicalProducts.AddRange(products.Values);
        foreach(var x in p.Packages)db.ClinicalPackageRecords.Add(new(){ProductId=products[x.Product].Id,Key=x.Key,Barcode=x.Barcode,Packaging=x.Packaging,UnitsPerPackage=x.UnitsPerPackage});
        foreach(var x in p.Monographs)db.ClinicalMonographRecords.Add(new(){DrugId=drugs[x.ClinicalDrug].Id,SectionsJson=JsonSerializer.Serialize(x),ReviewDue=x.ReviewDue});
        foreach(var x in p.Rules)db.ClinicalRuleRecords.Add(new(){VersionId=v.Id,Key=x.Key,PairKey=ClinicalPackageValidation.PairKey(x),Kind=x.Kind,Severity=x.Severity,RuleJson=JsonSerializer.Serialize(x)});
        db.ClinicalReviewRecords.Add(new(){VersionId=v.Id,Actor=actor.UserId,Action="Draft imported",Reason=input.ApprovalReference});
        return new{v.Id,v.Version,v.State,v.Digest};
    }
    public async Task<object> Decide(Guid id,ContentDecision input,Actor actor)
    {
        Safety.Require(input.Confirmed&&!string.IsNullOrWhiteSpace(input.Reason)&&input.Reason.Length<=1000,"Confirmation and a documented content decision are required.");
        var version=await db.ClinicalContentVersions.SingleAsync(x=>x.Id==id);Safety.Require(InventoryService.Hash(version.PayloadJson)==version.Digest,"Content checksum failed.");var package=ClinicalPackageValidation.Parse(version.PayloadJson);
        if(input.Action=="Submit"){Safety.Require(version.State=="Draft","Submit a draft version.");version.State="Pharmacist Review";}
        else if(input.Action=="Review"){
            Safety.Require(Roles.Clinical.Contains(actor.Role)&&version.State=="Pharmacist Review"&&actor.UserId!=version.Author,"A different pharmacist must review submitted content.");version.Reviewer=actor.UserId;version.ReviewedAt=DateTime.UtcNow;version.State="Reviewed";
        }else if(input.Action=="Approve"){
            Safety.Require(actor.Role==Roles.Chief&&version.State=="Reviewed"&&actor.UserId!=version.Author,"A different Chief Pharmacist must approve reviewed content; authors cannot approve their own package.");
            Safety.Require(package.Drugs.Length>0&&package.Monographs.Length>0,"Source-only/empty packages cannot become clinical publications.");version.Approver=actor.UserId;version.ApprovedAt=DateTime.UtcNow;version.State="Approved";
        }else if(input.Action is "Publish" or "Restore publication"){
            Safety.Require(actor.Role==Roles.Chief&&version.State is "Approved" or "Published","Chief Pharmacist authorization and an approved version are required.");
            Safety.Require(package.Monographs.All(m=>m.ReviewDue>=Safety.Today)&&package.Rules.All(r=>r.ReviewDue>=Safety.Today),"Overdue clinical content cannot be published/restored.");
            if(version.State!="Published")version.State="Published";
            var revision=(await db.ClinicalPublications.MaxAsync(x=>(long?)x.Revision)??0)+1;
            db.ClinicalPublications.Add(new(){VersionId=id,Revision=revision,Actor=actor.UserId,Reason=input.Reason});
        }else if(input.Action=="Archive"){Safety.Require(version.State is "Draft" or "Pharmacist Review" or "Reviewed" or "Approved","Published history cannot be edited or archived in place.");version.State="Archived";}
        else throw new InvalidOperationException("Unsupported content decision.");
        db.ClinicalReviewRecords.Add(new(){VersionId=id,Actor=actor.UserId,Action=input.Action,Reason=input.Reason});return new{version.Id,version.Version,version.State};
    }
    public async Task<object> Map(ClinicalMappingInput input,Actor actor)
    {
        Safety.Require(input.Confirmed&&!string.IsNullOrWhiteSpace(input.Reason)&&input.Reason.Length<=1000,"Confirm the exact product/ingredient/strength/form/route mapping and document verification.");
        var publication=await Current()??throw new InvalidOperationException("Publish reviewed reference content before mapping medicines.");
        var medicine=await db.Medicines.SingleOrDefaultAsync(x=>x.Id==input.MedicineId)??throw new InvalidOperationException("Unknown inventory medicine.");
        var product=await db.ClinicalProducts.SingleOrDefaultAsync(x=>x.VersionId==publication.VersionId&&x.Key==input.ProductKey)??throw new InvalidOperationException("Select a product from the current published version.");
        var drug=await db.ClinicalDrugs.SingleAsync(x=>x.Id==product.DrugId);
        static string N(string value)=>string.Join(' ',value.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
        Safety.Require(N(medicine.Strength)==N(drug.Strength)&&N(medicine.DosageForm)==N(drug.Form)&&N(medicine.Route)==N(drug.Route)&&N(medicine.BrandName)==N(product.Brand),"Reference brand, strength, form and route must match the inventory product. Correct and review source/catalog records first.");
        var package=ClinicalPackageValidation.Parse(publication.Version.PayloadJson);var definition=package.Drugs.Single(x=>x.Key==drug.Key);
        Safety.Require(new[]{definition.Name}.Concat(definition.Synonyms).Any(x=>N(x)==N(medicine.GenericName)),"Generic name must match the clinical drug or an approved synonym. Do not guess an ingredient mapping from brand or strength alone.");
        var mapping=await db.DrugClinicalMappings.SingleOrDefaultAsync(x=>x.MedicineId==input.MedicineId);
        if(mapping==null){mapping=new(){MedicineId=input.MedicineId};db.DrugClinicalMappings.Add(mapping);}mapping.ProductId=product.Id;mapping.ApprovedBy=actor.UserId;mapping.Reason=input.Reason;mapping.CreatedAt=DateTime.UtcNow;
        return new{mapping.Id,mapping.MedicineId,Product=product.Key,Version=publication.Version.Version};
    }
    public async Task<ResolvedClinicalMedicine[]> Resolve(Guid[] ids,ClinicalPublication? publication)
    {
        var medicines=await db.Medicines.AsNoTracking().Where(x=>ids.Contains(x.Id)).ToListAsync();
        var referenceDrugs=await db.ClinicalDrugs.AsNoTracking().Where(x=>ids.Contains(x.Id)&&x.VersionId==(publication==null?Guid.Empty:publication.VersionId)).ToListAsync();
        var referenceProducts=await db.ClinicalProducts.AsNoTracking().Include(x=>x.Drug).Where(x=>ids.Contains(x.Id)&&x.VersionId==(publication==null?Guid.Empty:publication.VersionId)).ToListAsync();
        Safety.Require(medicines.Count+referenceDrugs.Count+referenceProducts.Count==ids.Length,"Unknown or unpublished reference/inventory medicine; screening did not silently drop it.");
        var mappings=await db.DrugClinicalMappings.AsNoTracking().Include(x=>x.Product).ThenInclude(x=>x.Drug).Where(x=>ids.Contains(x.MedicineId)).ToListAsync();var package=publication==null?null:ClinicalPackageValidation.Parse(publication.Version.PayloadJson);
        return ids.Select(id=>{var m=medicines.SingleOrDefault(x=>x.Id==id);if(m==null){var product=referenceProducts.SingleOrDefault(x=>x.Id==id);var reference=product?.Drug??referenceDrugs.Single(x=>x.Id==id);var definition=package!.Drugs.Single(x=>x.Key==reference.Key);return new ResolvedClinicalMedicine(id,reference.Name+" / "+(product?.Brand??"")+" / "+reference.Strength,reference.Key,definition.Ingredients,definition.Classes,true);}var map=mappings.SingleOrDefault(x=>x.MedicineId==id&&x.Product.VersionId==publication?.VersionId);var drug=map==null?null:package?.Drugs.SingleOrDefault(x=>x.Key==map.Product.Drug.Key);
            return new ResolvedClinicalMedicine(id,string.Join(" / ",new[]{m.GenericName,m.BrandName,m.Strength,m.DosageForm}.Where(x=>x!="")),drug?.Key,drug?.Ingredients??[],drug?.Classes??[],drug!=null);}).ToArray();
    }
    public async Task<string> MappingDigest(Guid[] ids,ClinicalPublication? publication)
    {
        var rows=await db.DrugClinicalMappings.AsNoTracking().Where(x=>ids.Contains(x.MedicineId)).OrderBy(x=>x.MedicineId).Select(x=>new{x.MedicineId,x.ProductId,x.CreatedAt,x.ApprovedBy}).ToListAsync();
        var med=await db.Medicines.AsNoTracking().Where(x=>ids.Contains(x.Id)).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.UpdatedAt,x.GenericName,x.Strength,x.DosageForm,x.Route,x.ActiveStatus}).ToListAsync();
        var policy=await db.SystemSettings.Where(x=>x.Name=="ClinicalSafetyPolicy").Select(x=>x.Value).SingleOrDefaultAsync();
        return InventoryService.Hash(JsonSerializer.Serialize(new{Publication=publication?.Id,Version=publication?.Version.Digest,Policy=policy,Ids=ids.Order(),rows,med}));
    }
}
