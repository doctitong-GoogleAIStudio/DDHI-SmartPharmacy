using Npgsql;
namespace DDHI.Pharmacy.ServerManager;
public sealed partial class ServerController
{
    async Task EnsureAccountManagementGrants()
    {
        var secrets=ServerSecrets.Load(profile.DataRoot);
        await using var connection=new NpgsqlConnection(Connection("ddhi_migration",secrets.MigrationPassword));await connection.OpenAsync();
        await using var tx=await connection.BeginTransactionAsync();
        var tables=new[]{"AspNetUsers","AspNetUserClaims","AspNetUserRoles","AspNetUserLogins","AspNetUserTokens"};
        foreach(var table in tables){await using var grant=new NpgsqlCommand($"GRANT DELETE ON public.\"{table}\" TO ddhi_runtime",connection,tx);await grant.ExecuteNonQueryAsync();}
        await using var exists=new NpgsqlCommand("SELECT to_regclass('public.\"ExternalReturnIntakes\"') IS NOT NULL",connection,tx);
        if((bool)(await exists.ExecuteScalarAsync())!){await using var intakeGrant=new NpgsqlCommand("GRANT SELECT, INSERT ON public.\"ExternalReturnIntakes\" TO ddhi_runtime; REVOKE UPDATE, DELETE, TRUNCATE ON public.\"ExternalReturnIntakes\" FROM ddhi_runtime",connection,tx);await intakeGrant.ExecuteNonQueryAsync();}
        await tx.CommitAsync();
    }
}
