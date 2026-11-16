using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Npgsql;

namespace DDHI.Pharmacy.ServerManager;

public enum ItDatabasePermission { ReadOnlyInventory, DatabaseMaintenance }
public sealed record ItDatabaseLoginResult(string Username, string Access, int Port, string Instructions);

public static class ItDatabaseLoginRules
{
    public static void Validate(string username, string password, ItDatabasePermission permission, bool maintenanceAcknowledged)
    {
        if (!Regex.IsMatch(username, "^ddhi_it_[a-z][a-z0-9_]{0,54}$", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Use a unique lowercase username starting ddhi_it_, followed by a letter, letters/numbers/underscores, up to 63 characters total.");
        if (password.Length is < 16 or > 128 || password.Any(c => c < 33 || c > 126) ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || !password.Any(c => !char.IsLetterOrDigit(c)))
            throw new InvalidOperationException("Choose 16–128 printable ASCII characters with uppercase, lowercase, a number and a symbol; no spaces.");
        if (!Enum.IsDefined(permission)) throw new InvalidOperationException("Select an IT access level.");
        if (permission == ItDatabasePermission.DatabaseMaintenance && !maintenanceAcknowledged)
            throw new InvalidOperationException("Acknowledge that database maintenance can alter or delete pharmacy data.");
    }
    // The cleartext password never enters a SQL command, command line or password file.
    public static string ScramVerifier(string password)
    {
        const int iterations = 32768;
        var salt = RandomNumberGenerator.GetBytes(16);
        var bytes = Encoding.UTF8.GetBytes(password);
        byte[]? salted = null, clientKey = null, storedKey = null, serverKey = null;
        try {
            salted = Rfc2898DeriveBytes.Pbkdf2(bytes, salt, iterations, HashAlgorithmName.SHA256, 32);
            clientKey = HMACSHA256.HashData(salted, Encoding.ASCII.GetBytes("Client Key"));
            storedKey = SHA256.HashData(clientKey);
            serverKey = HMACSHA256.HashData(salted, Encoding.ASCII.GetBytes("Server Key"));
            return $"SCRAM-SHA-256${iterations}:{Convert.ToBase64String(salt)}${Convert.ToBase64String(storedKey)}:{Convert.ToBase64String(serverKey)}";
        } finally {
            CryptographicOperations.ZeroMemory(bytes);
            foreach(var secret in new[]{salted,clientKey,storedKey,serverKey}) if(secret != null) CryptographicOperations.ZeroMemory(secret);
        }
    }
}

public sealed partial class ServerController
{
    internal bool TestSkipItLoginElevation { get; set; }
    internal bool TestFailItLoginBeforeCommit { get; set; }
    const string ItSchemaMarker = "DDHI Server Control IT reporting v1";
    public async Task<ItDatabaseLoginResult> CreateItDatabaseLogin(string username, string password, ItDatabasePermission permission, bool maintenanceAcknowledged)
    {
        ItDatabaseLoginRules.Validate(username, password, permission, maintenanceAcknowledged);
        if (!TestSkipItLoginElevation) RequireAdministrator();
        if (!profile.Prepared) throw new InvalidOperationException("Open and start the existing prepared server instance first.");
        profile.Validate();
        FileStream changeLock;
        try { changeLock = new FileStream(Path.Combine(profile.DataRoot,"it-login-provision.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None); }
        catch (IOException) { throw new InvalidOperationException("Another IT login is being created for this instance. Wait for it to finish."); }
        using (changeLock) {
            var secrets = ServerSecrets.Load(profile.DataRoot);
            await using var owner = new NpgsqlConnection(Connection("ddhi_owner", secrets.OwnerPassword));
            await owner.OpenAsync();
            var actual = Convert.ToString(await new NpgsqlCommand("SHOW data_directory", owner).ExecuteScalarAsync());
            if (!string.Equals(Path.GetFullPath(actual!),Path.GetFullPath(PgData),StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The PostgreSQL port belongs to a different instance. No login was created.");
            await using var transaction = await owner.BeginTransactionAsync();
            async Task Execute(string sql) { await using var command = new NpgsqlCommand(sql, owner, transaction) { CommandTimeout=30 }; await command.ExecuteNonQueryAsync(); }
            try {
                await Execute("SELECT pg_advisory_xact_lock(1145325636, 18756)");
                await using(var exists = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_roles WHERE rolname=@name)",owner,transaction)) {
                    exists.Parameters.AddWithValue("name",username);
                    if ((bool)(await exists.ExecuteScalarAsync())!) throw new InvalidOperationException("That PostgreSQL login already exists. Choose another name; its password and permissions were not changed.");
                }
                await EnsureItReportingViews(owner,transaction);
                var role = new NpgsqlCommandBuilder().QuoteIdentifier(username);
                var verifier = ItDatabaseLoginRules.ScramVerifier(password);
                // The verifier alphabet has no quote characters; it is not the cleartext password.
                await Execute($"CREATE ROLE {role} LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOREPLICATION NOBYPASSRLS CONNECTION LIMIT 3 PASSWORD '{verifier}'");
                await Execute($"GRANT CONNECT ON DATABASE ddhi_pharmacy TO {role}; GRANT USAGE ON SCHEMA ddhi_it TO {role}; GRANT SELECT ON ddhi_it.medicines,ddhi_it.inventory,ddhi_it.suppliers,ddhi_it.locations TO {role}");
                await Execute($"ALTER ROLE {role} IN DATABASE ddhi_pharmacy SET search_path TO ddhi_it,pg_catalog; ALTER ROLE {role} IN DATABASE ddhi_pharmacy SET default_transaction_read_only TO on; ALTER ROLE {role} IN DATABASE ddhi_pharmacy SET statement_timeout TO '30s'; ALTER ROLE {role} IN DATABASE ddhi_pharmacy SET idle_in_transaction_session_timeout TO '60s'");
                if (permission == ItDatabasePermission.DatabaseMaintenance)
                    await Execute($"GRANT ddhi_migration TO {role} WITH INHERIT FALSE, SET TRUE, ADMIN FALSE");
                if (permission == ItDatabasePermission.ReadOnlyInventory) {
                    await using var excessive=new NpgsqlCommand("""
                        SELECT has_schema_privilege(@name,'public','CREATE') OR EXISTS(
                          SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
                          WHERE n.nspname='public' AND c.relkind IN ('r','p','v','m','f') AND
                          (has_table_privilege(@name,c.oid,'INSERT,UPDATE,DELETE,TRUNCATE,TRIGGER,REFERENCES')
                           OR has_any_column_privilege(@name,c.oid,'SELECT,INSERT,UPDATE,REFERENCES')))
                        """,owner,transaction);
                    excessive.Parameters.AddWithValue("name",username);
                    if ((bool)(await excessive.ExecuteScalarAsync())!)
                        throw new InvalidOperationException("Existing PUBLIC permissions would give this read-only login access to underlying application tables. No login was created. IT must review those database permissions first.");
                }
                if (TestFailItLoginBeforeCommit) throw new InvalidOperationException("Isolated provisioning rollback test.");
                await transaction.CommitAsync();
            } catch (PostgresException ex) {
                // Do not propagate SQL text, password verifiers, or detailed server diagnostics to the UI/log.
                throw new InvalidOperationException(ex.SqlState=="42710" ? "That login already exists; it was not changed." : $"PostgreSQL could not create this IT login (code {ex.SqlState}). The provisioning transaction was rolled back. Check server ownership and permissions.");
            }
            var access = permission == ItDatabasePermission.ReadOnlyInventory ? "Read-only inventory views" : "Database maintenance (explicit schema-owner role)";
            var instructions = $"Username: {username}\nHost: 127.0.0.1\nPort: {profile.PostgresPort}\nDatabase: ddhi_pharmacy\nAccess: {access}\nUse the password you just entered in SQL Shell or pgAdmin. It was not saved by Server Control.\nExample: SELECT * FROM ddhi_it.inventory LIMIT 100;";
            if (permission == ItDatabasePermission.DatabaseMaintenance)
                instructions += "\nFor approved maintenance, explicitly begin a READ WRITE transaction and SET LOCAL ROLE ddhi_migration. This allows changes to pharmacy data/schema, not server-level superuser administration.";
            try {
                Directory.CreateDirectory(Logs);
                var audit=JsonSerializer.Serialize(new { At=DateTimeOffset.UtcNow, Action="Create IT database login", Username=username, Access=access, Operator=WindowsIdentity.GetCurrent().Name });
                await File.AppendAllTextAsync(Path.Combine(Logs,"it-login-audit.jsonl"),audit+"\n",new UTF8Encoding(false));
            } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { instructions += "\nAccount created, but the local audit file could not be written. Review the log folder permissions."; }
            log("Created separate IT PostgreSQL login " + username + " — " + access + ". No application account password was changed.");
            return new(username, access, profile.PostgresPort, instructions);
        }
    }

    static async Task EnsureItReportingViews(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        async Task Execute(string sql) { await using var command=new NpgsqlCommand(sql,connection,transaction);await command.ExecuteNonQueryAsync(); }
        await using(var schema=new NpgsqlCommand("SELECT pg_get_userbyid(nspowner),obj_description(oid,'pg_namespace') FROM pg_namespace WHERE nspname='ddhi_it'",connection,transaction)) {
            await using var reader=await schema.ExecuteReaderAsync();
            if(await reader.ReadAsync()) {
                if(reader.GetString(0)!="ddhi_migration"||reader.IsDBNull(1)||reader.GetString(1)!=ItSchemaMarker)
                    throw new InvalidOperationException("An existing ddhi_it schema is not owned by this feature. It was not overwritten; no login was created.");
            }
        }
        await Execute("CREATE SCHEMA IF NOT EXISTS ddhi_it AUTHORIZATION ddhi_migration; COMMENT ON SCHEMA ddhi_it IS '"+ItSchemaMarker+"'");
        var views=new Dictionary<string,string> {
            ["medicines"]="SELECT \"Id\",\"GenericName\",\"BrandName\",\"Strength\",\"DosageForm\",\"BaseUnit\",\"ActiveStatus\" FROM public.\"Medicines\"",
            ["inventory"]="SELECT s.\"Id\",s.\"BatchId\",m.\"GenericName\",m.\"Strength\",m.\"DosageForm\",b.\"Lot\",b.\"Expiration\",l.\"Name\" AS \"Location\",s.\"Status\",s.\"Quantity\" FROM public.\"InventoryBalances\" s JOIN public.\"InventoryBatches\" b ON b.\"Id\"=s.\"BatchId\" JOIN public.\"Medicines\" m ON m.\"Id\"=b.\"MedicineId\" JOIN public.\"InventoryLocations\" l ON l.\"Id\"=s.\"LocationId\"",
            ["suppliers"]="SELECT \"Id\",\"Name\" FROM public.\"Suppliers\"",
            ["locations"]="SELECT \"Id\",\"Name\" FROM public.\"InventoryLocations\""
        };
        foreach(var (name,query) in views) {
            await using(var existing=new NpgsqlCommand("SELECT c.relkind::text,pg_get_userbyid(c.relowner),obj_description(c.oid,'pg_class') FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname='ddhi_it' AND c.relname=@name",connection,transaction)) {
                existing.Parameters.AddWithValue("name",name);
                await using var reader=await existing.ExecuteReaderAsync();
                if(await reader.ReadAsync()) if(reader.GetString(0)!="v"||reader.GetString(1)!="ddhi_migration"||reader.IsDBNull(2)||reader.GetString(2)!=ItSchemaMarker)
                    throw new InvalidOperationException("An existing IT reporting object is not owned by this feature. It was not overwritten; no login was created.");
            }
            await Execute($"CREATE OR REPLACE VIEW ddhi_it.{name} WITH(security_barrier=true,security_invoker=false) AS {query}; ALTER VIEW ddhi_it.{name} OWNER TO ddhi_migration; COMMENT ON VIEW ddhi_it.{name} IS '{ItSchemaMarker}'; REVOKE ALL ON ddhi_it.{name} FROM PUBLIC");
        }
        await Execute("REVOKE CREATE ON SCHEMA ddhi_it FROM PUBLIC");
    }
}
