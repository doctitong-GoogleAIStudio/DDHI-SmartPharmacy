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
        await tx.CommitAsync();
    }
}
