using System.Globalization;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;

namespace DDHI.Pharmacy.Application;

// Pure, deterministic evaluation of a single immutable published content snapshot.
public static class DrugSafetyEngine
{
    public static int Rank(string value)=>value switch{"Contraindicated"=>5,"Major"=>4,"Moderate"=>3,"Minor"=>2,"Unknown"=>1,_=>0};
    static bool Same(string a,string b)=>string.Equals(a,b,StringComparison.OrdinalIgnoreCase);
    static bool Select(ResolvedClinicalMedicine m,string kind,string key)=>m.Verified&&(kind switch{"Ingredient"=>m.Ingredients.Any(x=>Same(x.Ingredient,key)),"Class"=>m.Classes.Any(x=>Same(x,key)),"ClinicalDrug"=>Same(m.ClinicalDrug??"",key),_=>false});
    static bool? Conditions(ClinicalRuleDefinition rule,ClinicalScreenInput input,MedicationContext? left,MedicationContext? right)
    {
        var factors=new Dictionary<string,string>(input.Factors??[],StringComparer.OrdinalIgnoreCase);
        void Medication(string prefix,MedicationContext? medication){if(medication==null)return;if(medication.DailyDose!=null)factors[prefix+"DailyDose"]=medication.DailyDose.Value.ToString(CultureInfo.InvariantCulture);if(medication.DoseUnit!="")factors[prefix+"DoseUnit"]=medication.DoseUnit;if(medication.Route!="")factors[prefix+"Route"]=medication.Route;if(medication.Frequency!="")factors[prefix+"Frequency"]=medication.Frequency;}
        Medication("left",left);Medication("right",right);var unknown=false;
        foreach(var condition in rule.Conditions){if(!factors.TryGetValue(condition.Field,out var value)||string.IsNullOrWhiteSpace(value)){unknown=true;continue;}
            if(condition.Operator=="eq"){if(!Same(value,condition.Value))return false;}
            else if(decimal.TryParse(value,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var number)&&decimal.TryParse(condition.Value,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var threshold)){
                if(!(condition.Operator switch{"lt"=>number<threshold,"lte"=>number<=threshold,"gt"=>number>threshold,"gte"=>number>=threshold,_=>false}))return false;
            }else unknown=true;
        }
        return unknown?null:true;
    }
    public static ClinicalScreenResult Evaluate(string version,ClinicalPackage? package,ResolvedClinicalMedicine[] medicines,ClinicalScreenInput input,DateOnly today)
    {
        var findings=new List<SafetyFinding>();var gaps=new HashSet<string>();
        var documented=input.Allergies??[];var allergenKeys=(input.AllergyIngredients??[]).Concat(documented.Where(x=>x.Kind=="Ingredient").Select(x=>x.Key)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach(var allergy in documented)if(allergy.VerificationStatus!="Verified"||allergy.Certainty!="Confirmed"||string.IsNullOrWhiteSpace(allergy.Reaction)||string.IsNullOrWhiteSpace(allergy.Source))gaps.Add("Allergy documentation is unverified or incomplete: "+allergy.Key);
        if(package==null)gaps.Add("No published clinical dataset. Comprehensive interaction, allergy, disease, dose and IV coverage is unavailable.");
        if(!input.Reconciled)gaps.Add("Active medication reconciliation is incomplete. Dispensing history has not been treated as active therapy.");
        if(input.AllergyStatus=="Unknown")gaps.Add("Allergy history is unknown.");
        foreach(var allergen in allergenKeys)if(package==null||!package.Ingredients.Any(x=>Same(x.Key,allergen)))gaps.Add("Unrecognized allergen key; cross-reactivity and identity are unresolved: "+allergen);
        if(!input.ConditionsComplete)gaps.Add("Patient conditions and risk factors may be incomplete.");
        foreach(var condition in input.Conditions??[])if(package==null||!package.Rules.Any(x=>x.RightKind=="Condition"&&Same(x.Right,condition)))gaps.Add("Condition is unrecognized or lacks reviewed coverage: "+condition);
        foreach(var food in input.Foods??[])if(package==null||!package.Rules.Any(x=>x.Kind=="DrugFood"&&Same(x.Right,food)))gaps.Add("Food assessment coverage is unknown: "+food);
        foreach(var supplement in input.Supplements??[])if(package==null||!package.Rules.Any(x=>x.Kind=="DrugSupplement"&&Same(x.Right,supplement)))gaps.Add("Supplement assessment coverage is unknown: "+supplement);
        foreach(var m in medicines)if(!m.Verified)gaps.Add("Unmapped medicine: "+m.Name);
        var rules=package?.Rules??[];var context=input.Medications.ToDictionary(x=>x.MedicineId);
        var iv=input.Factors?.GetValueOrDefault("mode")=="IVCompatibility";
        void AddRule(ClinicalRuleDefinition rule,string[] names,MedicationContext? a,MedicationContext? b){
            Safety.Require(findings.Count<2000,"Clinical rule expansion exceeds safety limits. Content review is required; no screening clearance is recorded.");
            var applies=Conditions(rule,input,a,b);if(applies==false)return;
            var stale=rule.ReviewDue<today;var incomplete=applies==null||stale;
            if(incomplete)gaps.Add(stale?"Rule review overdue: "+rule.Key:"Missing/invalid conditions for rule: "+rule.Key);
            findings.Add(new(rule.Kind,incomplete?"Unknown":rule.Severity,rule.Significance,names,rule.Evidence,incomplete?"Clinical assessment incomplete. Review the cited guidance and missing context. "+rule.Management:rule.Management,rule.Monitoring,rule.Sources,rule.Key,rule.OriginalSeverity,incomplete?rule.Severity:"",rule.Mechanism,rule.Consequences,rule.DoseGuidance,rule.Alternatives,rule.LastReviewed,rule.ReviewDue,rule.Conditions));
        }
        for(var i=0;i<medicines.Length;i++)for(var j=i+1;j<medicines.Length;j++){
            var a=medicines[i];var b=medicines[j];var before=findings.Count;
            if(!iv&&a.Verified&&b.Verified){var duplicate=a.Ingredients.Select(x=>x.Ingredient).Intersect(b.Ingredients.Select(x=>x.Ingredient),StringComparer.OrdinalIgnoreCase).ToArray();
                if(duplicate.Length>0)findings.Add(new("Duplication","Moderate","Potential duplicate ingredient: "+string.Join(", ",duplicate),[a.Name,b.Name],"Verified ingredient mapping; intentional therapy and cumulative dose require pharmacist assessment.","Review combination products and all daily doses. No dose equivalence or intentional combination is assumed.","Cumulative exposure is not calculated when amounts, units or frequency are incomplete.",[]));}
            foreach(var rule in rules.Where(r=>iv?r.Kind=="IVCompatibility":r.Kind is "DrugDrug" or "Duplication")){
                if(Select(a,rule.LeftKind,rule.Left)&&Select(b,rule.RightKind,rule.Right))AddRule(rule,[a.Name,b.Name],context[a.MedicineId],context[b.MedicineId]);
                else if(Select(b,rule.LeftKind,rule.Left)&&Select(a,rule.RightKind,rule.Right))AddRule(rule,[b.Name,a.Name],context[b.MedicineId],context[a.MedicineId]);
            }
            // Duplication detection alone does not establish pairwise interaction coverage.
            if(!findings.Skip(before).Any(f=>f.Kind==(iv?"IVCompatibility":"DrugDrug"))){gaps.Add("No evaluated "+(iv?"IV compatibility":"drug-drug interaction")+" coverage: "+a.Name+" + "+b.Name);findings.Add(new(iv?"IVCompatibility":"DrugDrug","Unknown","Insufficient database coverage",[a.Name,b.Name],"No applicable reviewed pair rule found.","Use an authorized clinical reference and pharmacist review. Absence of a rule is not a negative interaction result.","",[]));}
        }
        if(!iv)foreach(var medicine in medicines){
            foreach(var ingredient in medicine.Ingredients.Where(_=>medicine.Verified))if(allergenKeys.Any(a=>Same(a,ingredient.Ingredient))){var records=documented.Where(x=>x.Kind=="Ingredient"&&Same(x.Key,ingredient.Ingredient)).ToArray();var intolerance=records.Length>0&&records.All(x=>x.Intolerance);findings.Add(new(intolerance?"Intolerance":"Allergy",intolerance?"Unknown":"Major",(intolerance?"Documented intolerance match: ":"Documented ingredient allergy match: ")+ingredient.Ingredient,[medicine.Name],"Patient declaration matched to verified ingredient identity; "+string.Join("; ",records.Select(x=>x.Reaction+" / "+x.Certainty+" / "+x.VerificationStatus+" / source: "+x.Source)),"Hold for pharmacist assessment and document the reaction, source and management.","",[]));}
            foreach(var rule in rules.Where(r=>r.Kind is "DrugDisease" or "Contraindication" or "AllergyCross" or "DrugFood" or "DrugSupplement")){
                if(!Select(medicine,rule.LeftKind,rule.Left))continue;
                var values=rule.Kind switch{"AllergyCross"=>rule.RightKind=="Class"?documented.Where(x=>x.Kind=="Class").Select(x=>x.Key).ToArray():allergenKeys,"DrugFood"=>input.Foods??[],"DrugSupplement"=>input.Supplements??[],_=>input.Conditions??[]};
                if(values.Any(x=>Same(x,rule.Right)))AddRule(rule,[medicine.Name,rule.Right],context[medicine.MedicineId],null);
            }
            if(medicine.Verified){var monograph=package?.Monographs.SingleOrDefault(m=>m.ClinicalDrug==medicine.ClinicalDrug);if(monograph==null)gaps.Add("No published monograph: "+medicine.Name);else if(monograph.ReviewDue<today)gaps.Add("Monograph review overdue: "+medicine.Name);}
        }
        if(findings.Any(f=>f.Severity=="Unknown"))gaps.Add("One or more assessments are unknown or conditional.");
        // A limited rule collection never establishes comprehensive coverage for unrepresented domains.
        gaps.Add("Limited curated dataset: full dose, cross-reactivity, food/supplement and patient-specific coverage is not established.");
        return new(version,gaps.Count==0,medicines.Length,medicines.Length*(medicines.Length-1)/2,findings.OrderByDescending(x=>Math.Max(Rank(x.Severity),Rank(x.PotentialSeverity))).ToArray(),gaps.Order().ToArray(),DrugReferenceSections.Disclaimer);
    }
}
