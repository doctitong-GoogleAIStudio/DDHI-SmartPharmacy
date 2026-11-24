using System.Text.Json;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;

public sealed class ClinicalSafetyService(PharmacyDb db)
{
    static Guid[] MappingIds(ClinicalScreenInput input)=>input.Medications.Select(x=>x.MedicineId).Concat((input.Allergies??[]).Where(x=>x.Kind=="Brand"&&Guid.TryParse(x.Key,out _)).Select(x=>Guid.Parse(x.Key))).Distinct().ToArray();
    public async Task<ClinicalSafetyPolicy> Policy(){var json=await db.SystemSettings.Where(x=>x.Name=="ClinicalSafetyPolicy").Select(x=>x.Value).SingleOrDefaultAsync();return json==null?new():JsonSerializer.Deserialize<ClinicalSafetyPolicy>(json)!;}
    public async Task<object> SetPolicy(ClinicalSafetyPolicy policy,Actor actor){Safety.Require(actor.Role==Roles.Chief&&!string.IsNullOrWhiteSpace(policy.ApprovalReference)&&policy.ApprovalReference.Length<=500,"Chief Pharmacist and written hospital medication-safety policy reference required.");var row=await db.SystemSettings.SingleOrDefaultAsync(x=>x.Name=="ClinicalSafetyPolicy");if(row==null){row=new(){Name="ClinicalSafetyPolicy"};db.SystemSettings.Add(row);}var json=JsonSerializer.Serialize(policy);db.AuditCapture?.Note("Clinical safety policy",row.Id.ToString(),"Approved hospital policy",row.Value,json);row.Value=json;return policy;}
    public async Task<object> Screen(ClinicalScreenInput input,Actor actor)
    {
        Safety.Require(input.Medications is {Length:>0 and <=30}&&input.Medications.All(x=>x!=null&&x.MedicineId!=Guid.Empty&&(x.DailyDose==null||x.DailyDose>0&&x.DailyDose<=100000000&&decimal.Round(x.DailyDose.Value,6)==x.DailyDose)&&x.DoseUnit is {Length:<=40}&&x.Route is {Length:<=100}&&x.Frequency is {Length:<=100}&&(x.DailyDose==null||!string.IsNullOrWhiteSpace(x.DoseUnit)))&&input.Medications.Select(x=>x.MedicineId).Distinct().Count()==input.Medications.Length,"Select 1 to 30 unique known medicines. Optional daily doses need explicit units, positive values within limits and at most six decimal places.");
        Safety.Require(input.EncounterReference is {Length:<=120}&&input.AllergyStatus is "Unknown" or "NoneKnown" or "Documented","Invalid encounter reference or allergy-history status.");
        Safety.Require(input.AllergyStatus=="Documented"||((input.AllergyIngredients??[]).Length==0&&(input.Allergies??[]).Length==0),"Allergen entries require Documented allergy status.");
        Safety.Require((input.Allergies??[]).Length<=50&&(input.Allergies??[]).All(x=>x!=null&&(x.Kind is "Ingredient" or "Class" or "Brand")&&x.Key is {Length:>0 and <=100}&&x.Reaction is {Length:<=500}&&x.Source is {Length:<=500}&&(x.Severity is "Unknown" or "Mild" or "Moderate" or "Severe")&&(x.Certainty is "Unknown" or "Suspected" or "Confirmed")&&(x.VerificationStatus is "Unverified" or "Verified")&&(x.ReactionDate is null||x.ReactionDate<=Safety.Today)),"Invalid structured allergy documentation.");
        Safety.Require((input.AllergyIngredients??[]).Length<=100&&(input.Conditions??[]).Length<=100&&(input.Factors?.Count??0)<=40&&(input.Foods??[]).Length<=100&&(input.Supplements??[]).Length<=100,"Patient context exceeds limits.");
        Safety.Require(input.Factors==null||input.Factors.All(x=>x.Key.Length<=64&&x.Value is {Length:<=200}),"Factor keys/values exceed limits.");
        var content=new ClinicalContentService(db);var publication=await content.Current();var package=await content.Published();var ids=input.Medications.Select(x=>x.MedicineId).ToArray();
        var resolved=await content.Resolve(ids,publication);var derived=new List<DocumentedDrugAllergy>();var brandMappings=new List<ResolvedClinicalMedicine>();var unresolved=new List<string>();
        foreach(var allergy in (input.Allergies??[]).Where(x=>x.Kind=="Brand")){if(Guid.TryParse(allergy.Key,out var brand)){try{var brandItem=(await content.Resolve([brand],publication))[0];brandMappings.Add(brandItem);if(brandItem.Verified)derived.AddRange(brandItem.Ingredients.Select(x=>allergy with{Kind="Ingredient",Key=x.Ingredient,Source="Verified brand mapping "+allergy.Key+"; "+allergy.Source}));else unresolved.Add("Brand allergy has no current verified ingredient mapping: "+allergy.Key);}catch(InvalidOperationException){unresolved.Add("Unresolved brand allergy: "+allergy.Key);}}else unresolved.Add("Brand allergy requires a reference/inventory item ID: "+allergy.Key);}
        var effective=input with{Allergies=(input.Allergies??[]).Concat(derived).ToArray()};var result=DrugSafetyEngine.Evaluate(publication?.Version.Version??"No published clinical data",package,resolved,effective,Safety.Today);if(unresolved.Count>0)result=result with{Complete=false,Gaps=result.Gaps.Concat(unresolved).ToArray()};
        var request=JsonSerializer.Serialize(input);var json=JsonSerializer.Serialize(result);Safety.Require(request.Length<=40000&&json.Length<=500000,"Screening request/result exceeds limits.");
        var resolution=JsonSerializer.Serialize(new{Medicines=resolved,AllergyBrands=brandMappings});var row=new ClinicalScreening{PublicationId=publication?.Id,Actor=actor.UserId,Workstation=actor.Workstation,EncounterReference=input.EncounterReference,MappingDigest=await content.MappingDigest(MappingIds(input),publication),InputJson=request,ResolutionJson=resolution,ResultJson=json,Digest=InventoryService.Hash(request+"|"+json+"|"+resolution)};db.ClinicalScreenings.Add(row);
        return new{row.Id,row.CreatedAt,Result=result,Resolution=resolved,Sources=package?.Sources??[],Policy=await Policy(),ExpiresAt=row.CreatedAt.AddMinutes(15)};
    }
    static ClinicalScreenResult Result(ClinicalScreening row)=>JsonSerializer.Deserialize<ClinicalScreenResult>(row.ResultJson)!;
    public async Task<object> Intervene(ClinicalInterventionInput input,Actor actor)
    {
        Safety.Require(Roles.Clinical.Contains(actor.Role)&&input.Confirmed&&input.Decision is "Proceed with documented review" or "Hold" or "Escalate","A pharmacist must confirm the clinical decision.");
        Safety.Require(!string.IsNullOrWhiteSpace(input.Management)&&input.Management.Length<=2000&&!string.IsNullOrWhiteSpace(input.ApprovalReference)&&input.ApprovalReference.Length<=500&&input.PrescriberClarification.Length<=2000,"Document management and authorization reference (no patient names).");
        var row=await db.ClinicalScreenings.AsNoTracking().SingleAsync(x=>x.Id==input.ScreenId);Safety.Require(row.Actor==actor.UserId&&row.CreatedAt>DateTime.UtcNow.AddMinutes(-15),"Review your own current screening assessment.");var result=Result(row);
        if(input.Decision=="Proceed with documented review"){
            Safety.Require(result.Complete||(await Policy()).AllowDocumentedIncompleteReview,"Chief Pharmacist must first approve the hospital's documented manual-review policy for incomplete clinical coverage. Otherwise hold/escalate.");
            Safety.Require(result.Complete||input.ReviewedIncomplete,"Document independent clinical review of incomplete/unknown coverage before proceeding.");
            Safety.Require(!result.Findings.Any(x=>x.Severity=="Contraindicated"||x.PotentialSeverity=="Contraindicated")||!string.IsNullOrWhiteSpace(input.PrescriberClarification),"Contraindicated or potentially contraindicated findings require documented prescriber clarification / authorized escalation.");
        }
        var revision=(await db.PharmacistInterventions.Where(x=>x.ScreenId==row.Id).MaxAsync(x=>(long?)x.Revision)??0)+1;
        var intervention=new PharmacistIntervention{Revision=revision,ScreenId=row.Id,Actor=actor.UserId,Role=actor.Role,Decision=input.Decision,Management=input.Management,PrescriberClarification=input.PrescriberClarification,ApprovalReference=input.ApprovalReference,ReviewedIncomplete=input.ReviewedIncomplete};db.PharmacistInterventions.Add(intervention);return new{intervention.Id,intervention.Decision};
    }
    public async Task Enforce(DispenseInput input,Actor actor,DispenseOrder order,Guid actualMedicine)
    {
        Safety.Require(input.SafetyAssessmentId!=null,"Update the client and complete medication-safety screening and pharmacist review before dispensing.");
        var screen=await db.ClinicalScreenings.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.SafetyAssessmentId)??throw new InvalidOperationException("Safety assessment not found.");
        Safety.Require(screen.Actor==actor.UserId&&screen.Workstation==actor.Workstation&&screen.CreatedAt>DateTime.UtcNow.AddMinutes(-15)&&screen.EncounterReference==order.EncounterReference,"Assessment expired or belongs to a different pharmacist, workstation or encounter.");
        Safety.Require(screen.Digest==InventoryService.Hash(screen.InputJson+"|"+screen.ResultJson+"|"+screen.ResolutionJson),"Safety assessment integrity failed.");
        var recorded=JsonSerializer.Deserialize<ClinicalScreenInput>(screen.InputJson)!;
        Safety.Require(recorded.OrderId==input.OrderId&&recorded.BalanceId==input.BalanceId&&recorded.IssueQuantity==input.Quantity&&recorded.Medications.Any(x=>x.MedicineId==actualMedicine),"Assessment does not match the exact order, batch, quantity or actual brand. Re-screen.");
        var content=new ClinicalContentService(db);var current=await content.Current();Safety.Require(screen.PublicationId==current?.Id&&screen.MappingDigest==await content.MappingDigest(MappingIds(recorded),current),"Clinical content, policy or medicine/allergen mapping changed. Re-screen before dispensing.");
        var decision=await db.PharmacistInterventions.AsNoTracking().Where(x=>x.ScreenId==screen.Id&&x.Actor==actor.UserId).OrderByDescending(x=>x.Revision).FirstOrDefaultAsync();
        Safety.Require(decision?.Decision=="Proceed with documented review","A pharmacist must record the management decision; held/escalated assessments cannot dispense.");
        Safety.Require(!await db.DispenseTransactions.AnyAsync(x=>x.SafetyAssessmentId==screen.Id),"This dispensing safety assessment was already used. Re-screen the next issue.");
    }
}
