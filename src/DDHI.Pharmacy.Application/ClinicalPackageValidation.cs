using System.Text.Json;
using System.Text.RegularExpressions;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
namespace DDHI.Pharmacy.Application;

public static class ClinicalPackageValidation
{
    public static readonly JsonSerializerOptions Json=new(){PropertyNameCaseInsensitive=true,MaxDepth=32,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow};
    public static string PairKey(ClinicalRuleDefinition rule){var a=rule.LeftKind+":"+rule.Left;var b=rule.RightKind+":"+rule.Right;return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rule.Kind+"|"+(rule.Directional?a+">"+b:string.Join("|",new[]{a,b}.Order(StringComparer.Ordinal)))+"|"+JsonSerializer.Serialize(rule.Conditions.OrderBy(x=>x.Field).ThenBy(x=>x.Operator).ThenBy(x=>x.Value)))));}
    public static ClinicalPackage Parse(string json){try{return ParseCore(json);}catch(Exception ex) when(ex is NullReferenceException or ArgumentNullException or IndexOutOfRangeException or JsonException){throw new InvalidOperationException("Clinical package contains missing/null or malformed required fields.");}}
    static ClinicalPackage ParseCore(string json)
    {
        Safety.Require(!string.IsNullOrWhiteSpace(json)&&json.Length<=2000000,"Package must contain 1 to 2,000,000 characters.");
        using(var document=JsonDocument.Parse(json,new JsonDocumentOptions{MaxDepth=32})){
            void Validate(JsonElement element){if(element.ValueKind==JsonValueKind.Object){var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var property in element.EnumerateObject()){Safety.Require(names.Add(property.Name)&&property.Name.Length<=64,"Duplicate or oversized JSON property.");if(property.Value.ValueKind==JsonValueKind.String)Safety.Require((property.Value.GetString()?.Length??0)<=(property.Name.Equals("Text",StringComparison.OrdinalIgnoreCase)?12000:2000),"Clinical text exceeds field limits.");Validate(property.Value);}}else if(element.ValueKind==JsonValueKind.Array)foreach(var item in element.EnumerateArray())Validate(item);}
            Validate(document.RootElement);
        }
        ClinicalPackage p;try{p=JsonSerializer.Deserialize<ClinicalPackage>(json,Json)??throw new JsonException();}catch(JsonException){throw new InvalidOperationException("Invalid clinical package JSON or schema.");}
        Safety.Require(p.Schema==1&&p.Version is {Length:>0 and <=80}&&p.LicenseStatement is {Length:>0 and <=2000},"Schema 1, version and license statement are required.");
        Safety.Require(p.Sources!=null&&p.Ingredients!=null&&p.Drugs!=null&&p.Products!=null&&p.Packages!=null&&p.Monographs!=null&&p.Rules!=null,"All package collections are required, including empty arrays.");
        static HashSet<string> Keys(IEnumerable<string> keys){var all=keys.ToArray();Safety.Require(all.Length<=5000&&all.All(k=>k!=null&&Regex.IsMatch(k,"^[a-z0-9][a-z0-9._-]{0,63}$"))&&all.Distinct().Count()==all.Length,"Unique lowercase keys (max 64 characters) are required; max 5,000 records per collection.");return all.ToHashSet();}
        var sources=Keys(p.Sources!.Select(x=>x.Key));var ingredients=Keys(p.Ingredients!.Select(x=>x.Key));var drugs=Keys(p.Drugs!.Select(x=>x.Key));var products=Keys(p.Products!.Select(x=>x.Key));Keys(p.Packages!.Select(x=>x.Key));Keys(p.Rules!.Select(x=>x.Key));
        void References(string[] refs){Safety.Require(refs is {Length:>0}&&refs.All(sources.Contains),"Every clinical record needs existing source references.");}
        void Text(string? value,string name,int max=2000)=>Safety.Require(value!=null&&value.Length<=max&&!value.Contains('\0'),"Invalid or oversized "+name);
        foreach(var source in p.Sources){Safety.Require(!string.IsNullOrWhiteSpace(source.Organization)&&!string.IsNullOrWhiteSpace(source.Document)&&!string.IsNullOrWhiteSpace(source.LicenseBasis),"Source organization, document and permitted-use basis are required.");Safety.Require(Uri.TryCreate(source.Url,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&source.RevisionDate>=new DateOnly(1900,1,1)&&source.RetrievedDate<=Safety.Today&&source.RevisionDate<=source.RetrievedDate,"Source needs an HTTPS URL and valid revision/retrieval dates.");}
        foreach(var ingredient in p.Ingredients){Text(ingredient.Name,"ingredient name");Safety.Require(!string.IsNullOrWhiteSpace(ingredient.Name)&&ingredient.Synonyms!=null&&string.Join(' ',ingredient.Synonyms).Length<=1500,"Ingredient name/synonyms are invalid.");}
        foreach(var drug in p.Drugs){Safety.Require(!string.IsNullOrWhiteSpace(drug.Name)&&drug.Ingredients is {Length:>0}&&drug.Ingredients.All(i=>ingredients.Contains(i.Ingredient)&&(i.Amount is null or >0)&&(i.Amount==null||!string.IsNullOrWhiteSpace(i.Unit)))&&drug.Ingredients.Select(i=>i.Ingredient).Distinct().Count()==drug.Ingredients.Length&&drug.Classes!=null&&drug.Synonyms!=null,"Drug must have unique valid ingredient links and valid optional amounts.");Text(drug.Strength,"strength");Text(drug.Form,"form");Text(drug.Route,"route");Safety.Require(string.Join(' ',drug.Synonyms.Concat(drug.Classes)).Length<=1000,"Drug search terms exceed limits.");}
        foreach(var product in p.Products){Safety.Require(drugs.Contains(product.ClinicalDrug),"Product references an unknown clinical drug.");References(product.Sources);Text(product.Brand,"brand");Safety.Require(product.RegistrationVerified is null||product.RegistrationVerified<=Safety.Today,"Registration verification cannot be future-dated.");}
        foreach(var package in p.Packages){Safety.Require(products.Contains(package.Product)&&package.UnitsPerPackage>0&&package.UnitsPerPackage<=1000000,"Invalid product package.");Text(package.Barcode,"barcode",200);}
        Safety.Require(p.Packages.Where(x=>x.Barcode!="").Select(x=>x.Barcode).Distinct().Count()==p.Packages.Count(x=>x.Barcode!=""),"Conflicting package barcodes.");
        Safety.Require(p.Monographs.Select(x=>x.ClinicalDrug).Distinct().Count()==p.Monographs.Length,"Duplicate monographs.");
        foreach(var mono in p.Monographs){Safety.Require(drugs.Contains(mono.ClinicalDrug)&&mono.Sections!=null&&mono.Sections.Select(x=>x.Heading).Distinct().Count()==mono.Sections.Length&&mono.LastReviewed>=new DateOnly(1900,1,1)&&mono.LastReviewed<=Safety.Today&&mono.ReviewDue>=mono.LastReviewed,"Invalid monograph/review dates.");References(mono.Sources);foreach(var section in mono.Sections){Safety.Require(DrugReferenceSections.MonographFields.Contains(section.Heading),"Unsupported monograph section.");Text(section.Text,"monograph section",12000);References(section.Sources);}}
        var pairs=new HashSet<string>();foreach(var rule in p.Rules){
            Safety.Require(new[]{"DrugDrug","DrugDisease","DrugFood","DrugSupplement","AllergyCross","Duplication","Contraindication","IVCompatibility"}.Contains(rule.Kind),"Unsupported rule kind.");
            Safety.Require(new[]{"Contraindicated","Major","Moderate","Minor","Unknown","NoKnownInteractionFound"}.Contains(rule.Severity)&&!string.IsNullOrWhiteSpace(rule.OriginalSeverity)&&!string.IsNullOrWhiteSpace(rule.Evidence)&&!string.IsNullOrWhiteSpace(rule.Management),"Severity, original classification, evidence and management are required.");
            bool Selector(string kind,string key)=>kind switch{"Ingredient"=>ingredients.Contains(key),"ClinicalDrug"=>drugs.Contains(key),"Class"=>p.Drugs.Any(d=>d.Classes.Contains(key)),_=>false};
            var rightAllowed=rule.Kind switch{"DrugDrug" or "Duplication" or "IVCompatibility"=>Selector(rule.RightKind,rule.Right),"AllergyCross"=>Selector(rule.RightKind,rule.Right),"DrugFood"=>rule.RightKind=="Food"&&!string.IsNullOrWhiteSpace(rule.Right),"DrugSupplement"=>rule.RightKind=="Supplement"&&!string.IsNullOrWhiteSpace(rule.Right),_=>rule.RightKind=="Condition"&&!string.IsNullOrWhiteSpace(rule.Right)};
            Safety.Require(Selector(rule.LeftKind,rule.Left)&&rightAllowed,"Invalid rule selectors.");
            Safety.Require(rule.Conditions!=null&&rule.Conditions.Length<=20&&rule.Conditions.All(c=>c.Field.Length is >0 and <=64&&c.Value.Length<=200&&new[]{"eq","lt","lte","gt","gte"}.Contains(c.Operator)),"Invalid conditions.");
            foreach(var dose in new[]{"leftDailyDose","rightDailyDose"})if(rule.Conditions.Any(c=>c.Field==dose))Safety.Require(rule.Conditions.Any(c=>c.Field==dose.Replace("DailyDose","DoseUnit")&&c.Operator=="eq"),"Dose thresholds require an exact dose-unit condition.");
            if(rule.Kind=="IVCompatibility")foreach(var field in new[]{"ivConcentrationA","ivConcentrationB","ivDiluent","ivTemperature","ivContainer","ivContact"})Safety.Require(rule.Conditions.Any(c=>c.Field==field&&c.Operator=="eq"),"IV rules require all six exact compatibility conditions, including concentration units.");
            Safety.Require(rule.LastReviewed>=new DateOnly(1900,1,1)&&rule.LastReviewed<=Safety.Today&&rule.ReviewDue>=rule.LastReviewed&&pairs.Add(PairKey(rule)),"Invalid review dates or duplicate/reversed rule.");References(rule.Sources);Text(rule.Significance,"significance");Text(rule.Mechanism,"mechanism");Text(rule.Management,"management");Text(rule.Monitoring,"monitoring");
        }
        return p;
    }
}
