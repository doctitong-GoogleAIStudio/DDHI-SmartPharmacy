using DDHI.Pharmacy.ServerManager;
using Npgsql;
using System.IO;
using System.Text.Json;
using Xunit;

internal static class ItDatabaseLoginAssertions
{
    internal static async Task Exercise(ServerController controller,ServerProfile profile,string root)
    {
        controller.TestSkipItLoginElevation=true;
        var secrets=ServerSecrets.Load(profile.DataRoot);
        var originalCredentials=await File.ReadAllBytesAsync(Path.Combine(profile.DataRoot,"credentials.dpapi"));
        await using var owner=new NpgsqlConnection(controller.Connection("ddhi_owner",secrets.OwnerPassword));await owner.OpenAsync();
        async Task<object?> Scalar(string sql)=>await new NpgsqlCommand(sql,owner).ExecuteScalarAsync();
        async Task Execute(string sql)=>await new NpgsqlCommand(sql,owner).ExecuteNonQueryAsync();
        var originalPasswords=await Scalar("SELECT string_agg(rolname||rolpassword,';' ORDER BY rolname) FROM pg_authid WHERE rolname IN ('ddhi_owner','ddhi_runtime','ddhi_migration')");
        var originalStock=await Scalar("SELECT (SELECT count(*)::text FROM \"StockMovements\")||':'||(SELECT coalesce(sum(\"Quantity\"),0)::text FROM \"InventoryBalances\")");
        var suffix=Guid.NewGuid().ToString("N")[..12];
        var reader="ddhi_it_r_"+suffix;var maint="ddhi_it_m_"+suffix;var failed="ddhi_it_f_"+suffix;
        const string password="Test'Only!Strong12345";
        const string otherPassword="Different!Strong123456";
        async Task<bool> RoleExists(string name){await using var cmd=new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_roles WHERE rolname=@name)",owner);cmd.Parameters.AddWithValue("name",name);return (bool)(await cmd.ExecuteScalarAsync())!;}

        await Execute("CREATE SCHEMA ddhi_it AUTHORIZATION ddhi_owner");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.CreateItDatabaseLogin(reader,password,ItDatabasePermission.ReadOnlyInventory,false));
        Assert.False(await RoleExists(reader));Assert.Equal("ddhi_owner",await Scalar("SELECT pg_get_userbyid(nspowner) FROM pg_namespace WHERE nspname='ddhi_it'"));
        await Execute("DROP SCHEMA ddhi_it");

        var created=await controller.CreateItDatabaseLogin(reader,password,ItDatabasePermission.ReadOnlyInventory,false);
        Assert.Equal(reader,created.Username);Assert.DoesNotContain(password,created.Instructions);
        await using(var login=new NpgsqlConnection(controller.Connection(reader,password))){
            await login.OpenAsync();
            Assert.Equal(reader,await new NpgsqlCommand("SELECT current_user",login).ExecuteScalarAsync());
            Assert.Equal("on",await new NpgsqlCommand("SHOW default_transaction_read_only",login).ExecuteScalarAsync());
            Assert.True(Convert.ToInt64(await new NpgsqlCommand("SELECT count(*) FROM ddhi_it.inventory",login).ExecuteScalarAsync())>=0);
            await new NpgsqlCommand("SET default_transaction_read_only=off",login).ExecuteNonQueryAsync();
            foreach(var forbidden in new[]{"SELECT * FROM public.\"AspNetUsers\"","UPDATE public.\"InventoryBalances\" SET \"Quantity\"=0 WHERE false","INSERT INTO ddhi_it.locations(\"Name\") VALUES ('blocked')","SET ROLE ddhi_migration","SET ROLE ddhi_owner","SELECT * FROM pg_authid","CREATE ROLE ddhi_it_unwanted"}) {
                var error=await Assert.ThrowsAsync<PostgresException>(()=>new NpgsqlCommand(forbidden,login).ExecuteNonQueryAsync());Assert.Equal("42501",error.SqlState);
            }
        }
        await using(var wrong=new NpgsqlConnection(controller.Connection(reader,otherPassword))) {var error=await Assert.ThrowsAsync<PostgresException>(()=>wrong.OpenAsync());Assert.Equal("28P01",error.SqlState);}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.CreateItDatabaseLogin(reader,otherPassword,ItDatabasePermission.DatabaseMaintenance,true));
        await using(var original=new NpgsqlConnection(controller.Connection(reader,password))){await original.OpenAsync();}
        Assert.False((bool)(await Scalar($"SELECT pg_has_role('{reader}','ddhi_migration','MEMBER')"))!);

        await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.CreateItDatabaseLogin(maint,password,ItDatabasePermission.DatabaseMaintenance,false));Assert.False(await RoleExists(maint));
        await controller.CreateItDatabaseLogin(maint,password,ItDatabasePermission.DatabaseMaintenance,true);
        Assert.True((bool)(await Scalar($"SELECT pg_has_role('{maint}','ddhi_migration','MEMBER')"))!);
        Assert.False((bool)(await Scalar($"SELECT rolsuper OR rolcreatedb OR rolcreaterole OR rolreplication OR rolbypassrls OR rolinherit FROM pg_roles WHERE rolname='{maint}'"))!);
        await using(var login=new NpgsqlConnection(controller.Connection(maint,password))){
            await login.OpenAsync();
            await new NpgsqlCommand("BEGIN READ WRITE; SET LOCAL ROLE ddhi_migration; CREATE TABLE public.ddhi_it_qa_rollback(id integer); ROLLBACK",login).ExecuteNonQueryAsync();
            Assert.Equal(DBNull.Value,await Scalar("SELECT to_regclass('public.ddhi_it_qa_rollback')::text"));
            var error=await Assert.ThrowsAsync<PostgresException>(()=>new NpgsqlCommand("SET ROLE ddhi_owner",login).ExecuteNonQueryAsync());Assert.Equal("42501",error.SqlState);
        }
        controller.TestFailItLoginBeforeCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.CreateItDatabaseLogin(failed,password,ItDatabasePermission.ReadOnlyInventory,false));
        controller.TestFailItLoginBeforeCommit=false;Assert.False(await RoleExists(failed));
        await Execute("GRANT SELECT ON public.\"AspNetUsers\" TO PUBLIC");
        try {await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.CreateItDatabaseLogin(failed,password,ItDatabasePermission.ReadOnlyInventory,false));Assert.False(await RoleExists(failed));}
        finally {await Execute("REVOKE SELECT ON public.\"AspNetUsers\" FROM PUBLIC");}
        Assert.Equal(originalPasswords,await Scalar("SELECT string_agg(rolname||rolpassword,';' ORDER BY rolname) FROM pg_authid WHERE rolname IN ('ddhi_owner','ddhi_runtime','ddhi_migration')"));
        Assert.Equal(originalStock,await Scalar("SELECT (SELECT count(*)::text FROM \"StockMovements\")||':'||(SELECT coalesce(sum(\"Quantity\"),0)::text FROM \"InventoryBalances\")"));
        Assert.Equal(originalCredentials,await File.ReadAllBytesAsync(Path.Combine(profile.DataRoot,"credentials.dpapi")));
        foreach(var path in Directory.EnumerateFiles(Path.Combine(profile.DataRoot,"logs"))) {
            await using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
            using var readerLog=new StreamReader(stream);var text=await readerLog.ReadToEndAsync();Assert.DoesNotContain(password,text);Assert.DoesNotContain(otherPassword,text);
        }
        Assert.Contains("ready",await controller.Health());
        await File.WriteAllTextAsync(Path.Combine(root,"release","Tests","ItDatabaseLoginSmoke.json"),JsonSerializer.Serialize(new {
            version="0.6.0",scramAuthentication=true,wrongPasswordRejected=true,readonlyViewsWork=true,sensitiveTablesDenied=true,
            writesDenied=true,maintenanceExplicit=true,noSuperuser=true,duplicatePreserved=true,rollbackAtomic=true,
            namespaceCollisionRejected=true,excessivePublicGrantsRejected=true,applicationCredentialsUnchanged=true,
            stockAndLedgerUnchanged=true,plaintextPasswordsAbsentFromLogs=true,realServerProvisioned=false
        },new JsonSerializerOptions{WriteIndented=true}));
    }
}
