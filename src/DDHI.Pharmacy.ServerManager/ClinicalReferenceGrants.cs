using Npgsql;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController
{
    static readonly string[] ClinicalImmutableTables=["ClinicalSourceRecords","DrugIngredients","ClinicalDrugs","ClinicalDrugIngredients","ClinicalProducts","ClinicalPackageRecords","ClinicalRuleRecords","ClinicalMonographRecords","ClinicalPublications","ClinicalReviewRecords","ClinicalScreenings","PharmacistInterventions"];
    static async Task ClinicalGrants(NpgsqlConnection connection,NpgsqlTransaction tx)
    {
        await using var exists=new NpgsqlCommand("SELECT to_regclass('public.\"ClinicalContentVersions\"') IS NOT NULL",connection,tx);if(!(bool)(await exists.ExecuteScalarAsync())!)return;
        foreach(var table in ClinicalImmutableTables){await using var command=new NpgsqlCommand($"GRANT SELECT,INSERT ON public.\"{table}\" TO ddhi_runtime; REVOKE UPDATE,DELETE,TRUNCATE ON public.\"{table}\" FROM ddhi_runtime;",connection,tx);await command.ExecuteNonQueryAsync();}
        foreach(var table in new[]{"ClinicalContentVersions","DrugClinicalMappings","ReferencePreferences"}){await using var command=new NpgsqlCommand($"GRANT SELECT,INSERT,UPDATE ON public.\"{table}\" TO ddhi_runtime; REVOKE DELETE,TRUNCATE ON public.\"{table}\" FROM ddhi_runtime;",connection,tx);await command.ExecuteNonQueryAsync();}
    }
}
