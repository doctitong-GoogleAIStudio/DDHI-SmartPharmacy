using DDHI.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
namespace DDHI.Pharmacy.Infrastructure;

public static class ClinicalModel
{
    public static readonly Type[] Immutable=[typeof(ClinicalSourceRecord),typeof(DrugIngredient),typeof(ClinicalDrug),typeof(ClinicalDrugIngredient),typeof(ClinicalProduct),typeof(ClinicalPackageRecord),typeof(ClinicalRuleRecord),typeof(ClinicalMonographRecord),typeof(ClinicalPublication),typeof(ClinicalReviewRecord),typeof(ClinicalScreening),typeof(PharmacistIntervention)];
    public static void Configure(ModelBuilder b)
    {
        b.Entity<OperationKey>().Property(x=>x.Result).HasColumnType("text").Metadata.SetMaxLength(null);
        b.Entity<ClinicalContentVersion>().HasIndex(x=>x.Version).IsUnique();b.Entity<ClinicalContentVersion>().Property(x=>x.PayloadJson).HasColumnType("text").Metadata.SetMaxLength(null);
        b.Entity<ClinicalSourceRecord>().HasIndex(x=>new{x.VersionId,x.Key}).IsUnique();
        b.Entity<DrugIngredient>().HasIndex(x=>new{x.VersionId,x.Key}).IsUnique();b.Entity<DrugIngredient>().HasIndex(x=>new{x.VersionId,x.SearchText});
        b.Entity<ClinicalDrug>().HasIndex(x=>new{x.VersionId,x.Key}).IsUnique();b.Entity<ClinicalDrug>().HasIndex(x=>new{x.VersionId,x.SearchText});
        b.Entity<ClinicalDrugIngredient>().HasIndex(x=>new{x.DrugId,x.IngredientId}).IsUnique();b.Entity<ClinicalDrugIngredient>().Property(x=>x.Amount).HasPrecision(20,6);
        b.Entity<ClinicalProduct>().HasIndex(x=>new{x.VersionId,x.Key}).IsUnique();b.Entity<ClinicalProduct>().HasIndex(x=>new{x.VersionId,x.SearchText});
        b.Entity<ClinicalPackageRecord>().HasIndex(x=>new{x.ProductId,x.Key}).IsUnique();b.Entity<ClinicalPackageRecord>().HasIndex(x=>x.Barcode);
        b.Entity<DrugClinicalMapping>().HasIndex(x=>x.MedicineId).IsUnique();
        b.Entity<ClinicalRuleRecord>().HasIndex(x=>new{x.VersionId,x.Key}).IsUnique();b.Entity<ClinicalRuleRecord>().HasIndex(x=>new{x.VersionId,x.PairKey}).IsUnique();b.Entity<ClinicalRuleRecord>().Property(x=>x.RuleJson).HasColumnType("text").Metadata.SetMaxLength(null);
        b.Entity<ClinicalMonographRecord>().HasIndex(x=>x.DrugId).IsUnique();b.Entity<ClinicalMonographRecord>().Property(x=>x.SectionsJson).HasColumnType("text").Metadata.SetMaxLength(null);
        b.Entity<ClinicalPublication>().HasIndex(x=>x.Revision).IsUnique();b.Entity<ClinicalScreening>().HasIndex(x=>new{x.Actor,x.CreatedAt});
        b.Entity<ClinicalScreening>().Property(x=>x.InputJson).HasColumnType("text").Metadata.SetMaxLength(null);b.Entity<ClinicalScreening>().Property(x=>x.ResultJson).HasColumnType("text").Metadata.SetMaxLength(null);
        b.Entity<ClinicalScreening>().Property(x=>x.ResolutionJson).HasColumnType("text").Metadata.SetMaxLength(null);b.Entity<PharmacistIntervention>().HasIndex(x=>new{x.ScreenId,x.Revision}).IsUnique();
        b.Entity<DispenseTransaction>().HasIndex(x=>x.SafetyAssessmentId).IsUnique().HasFilter("\"SafetyAssessmentId\" IS NOT NULL");
        b.Entity<ReferencePreference>().HasIndex(x=>new{x.Actor,x.MedicineId}).IsUnique();
        b.Entity<DispenseTransaction>().HasOne<ClinicalScreening>().WithMany().HasForeignKey(x=>x.SafetyAssessmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
