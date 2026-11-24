namespace DDHI.Pharmacy.Contracts;

public static class DrugReferenceSections
{
    public static readonly string[] All=["Drug Search","Generic Drug Directory","Brand Directory","Therapeutic Classification","Drug Interaction Checker","Drug Allergy Checker","Drug-Disease Checker","Dosage Reference","Renal Dosage Guide","Hepatic Dosage Guide","Pregnancy and Lactation","Antibiotic Reference","IV Compatibility","DDHI Hospital Formulary","AI Drug Assistant","Drug Safety Alerts","Database Administration","Clinical References and Updates"];
    public const string Disclaimer="DDHI Drug Reference & Interaction Checker is a clinical decision-support and pharmaceutical information system intended for authorized healthcare professionals. It does not replace professional clinical judgment, official prescribing information, or individualized patient assessment. Drug information and interaction results depend on available database coverage and documented patient information. Absence of an identified interaction does not guarantee medication safety.";
    public static readonly string[] MonographFields=["Identification","Composition","Mechanism","ApprovedIndications","OffLabelIndications","AdultDose","PediatricDose","GeriatricDose","InitialDose","MaintenanceDose","MaximumDose","Titration","Duration","Administration","Meals","MissedDose","Contraindications","Precautions","SeriousWarnings","CommonAdverseReactions","SeriousAdverseReactions","Interactions","FoodAlcoholSupplements","Renal","Hepatic","Pregnancy","Lactation","Monitoring","Overdose","Antidote","Counseling","Storage","Antibiotic","IVCompatibility","Formulary"];
}
public sealed record ClinicalSource(string Key,string Organization,string Document,string Url,string LicenseBasis,DateOnly RevisionDate,DateOnly RetrievedDate,string OriginalIdentifier="");
public sealed record IngredientDefinition(string Key,string Name,string[] Synonyms);
public sealed record IngredientAmount(string Ingredient,decimal? Amount,string Unit);
public sealed record ClinicalDrugDefinition(string Key,string Name,string Strength,string Form,string Route,IngredientAmount[] Ingredients,string[] Classes,string[] Synonyms);
public sealed record ProductDefinition(string Key,string ClinicalDrug,string Brand,string Manufacturer,string Distributor,string ImportedBy,string Country,string RegistrationNumber,string RegistrationStatus,DateOnly? RegistrationVerified,string PrescriptionClass,string[] Sources);
public sealed record PackageDefinition(string Key,string Product,string Barcode,string Packaging,decimal UnitsPerPackage);
public sealed record MonographSection(string Heading,string Text,string[] Sources);
public sealed record MonographDefinition(string ClinicalDrug,MonographSection[] Sections,string[] Sources,DateOnly LastReviewed,DateOnly ReviewDue);
public sealed record RuleCondition(string Field,string Operator,string Value);
public sealed record ClinicalRuleDefinition(string Key,string Kind,string LeftKind,string Left,string RightKind,string Right,string Severity,string OriginalSeverity,string Evidence,string Significance,string Mechanism,string Consequences,string Management,string Monitoring,string[] Sources,DateOnly LastReviewed,DateOnly ReviewDue,RuleCondition[] Conditions,bool Directional=false,string DoseGuidance="",string Alternatives="");
public sealed record ClinicalPackage(int Schema,string Version,string LicenseStatement,ClinicalSource[] Sources,IngredientDefinition[] Ingredients,ClinicalDrugDefinition[] Drugs,ProductDefinition[] Products,PackageDefinition[] Packages,MonographDefinition[] Monographs,ClinicalRuleDefinition[] Rules);
public sealed record PackageImport(string Json,string Sha256,bool RightsConfirmed,string ApprovalReference);
public sealed record ContentDecision(string Action,string Reason,bool Confirmed);
public sealed record ClinicalMappingInput(Guid MedicineId,string ProductKey,string Reason,bool Confirmed);
public sealed record MedicationContext(Guid MedicineId,decimal? DailyDose=null,string DoseUnit="",string Route="",string Frequency="");
public sealed record DocumentedDrugAllergy(string Kind,string Key,string Reaction,string Severity,DateOnly? ReactionDate,string Certainty,bool Intolerance,string Source,string VerificationStatus);
public sealed record ClinicalScreenInput(MedicationContext[] Medications,string EncounterReference="",bool Reconciled=false,string AllergyStatus="Unknown",string[]? AllergyIngredients=null,string[]? Conditions=null,bool ConditionsComplete=false,Dictionary<string,string>? Factors=null,string[]? Foods=null,string[]? Supplements=null,Guid? OrderId=null,Guid? BalanceId=null,decimal? IssueQuantity=null,DocumentedDrugAllergy[]? Allergies=null);
public sealed record ClinicalInterventionInput(Guid ScreenId,string Decision,string Management,string PrescriberClarification,string ApprovalReference,bool ReviewedIncomplete,bool Confirmed);
public sealed record SafetyFinding(string Kind,string Severity,string Title,string[] Medicines,string Evidence,string Management,string Monitoring,string[] Sources,string RuleKey="",string OriginalSeverity="",string PotentialSeverity="",string Mechanism="",string Consequences="",string DoseGuidance="",string Alternatives="",DateOnly? LastReviewed=null,DateOnly? ReviewDue=null,RuleCondition[]? Conditions=null);
public sealed record ClinicalScreenResult(string DatabaseVersion,bool Complete,int TotalMedications,int CombinationsEvaluated,SafetyFinding[] Findings,string[] Gaps,string Disclaimer);
public sealed record ResolvedClinicalMedicine(Guid MedicineId,string Name,string? ClinicalDrug,IngredientAmount[] Ingredients,string[] Classes,bool Verified);
public sealed record ClinicalSafetyPolicy(bool AllowDocumentedIncompleteReview=false,bool InterruptModerate=true,string ApprovalReference="");
