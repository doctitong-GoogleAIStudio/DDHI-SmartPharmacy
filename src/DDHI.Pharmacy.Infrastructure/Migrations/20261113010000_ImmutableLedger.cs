
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace DDHI.Pharmacy.Infrastructure.Migrations;
[DbContext(typeof(PharmacyDb))]
[Migration("20261113010000_ImmutableLedger")]
public sealed class ImmutableLedger:Migration {
 protected override void Up(MigrationBuilder migrationBuilder)=>migrationBuilder.Sql("""
 CREATE FUNCTION ddhi_reject_immutable_change() RETURNS trigger LANGUAGE plpgsql AS $$
 BEGIN RAISE EXCEPTION 'DDHI immutable records cannot be changed or deleted'; END; $$;
 CREATE TRIGGER immutable_stock BEFORE UPDATE OR DELETE OR TRUNCATE ON "StockMovements" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
 CREATE TRIGGER immutable_audit BEFORE UPDATE OR DELETE OR TRUNCATE ON "AuditEvents" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
 CREATE TRIGGER immutable_operations BEFORE UPDATE OR DELETE OR TRUNCATE ON "OperationKeys" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
 """);
 protected override void Down(MigrationBuilder m)=>throw new InvalidOperationException("Immutable-ledger rollback requires hospital-authorized recovery, never automatic downgrade.");
}
