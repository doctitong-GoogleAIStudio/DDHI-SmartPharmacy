-- Run as DBA. Supply strong role passwords interactively using psql \password.
-- Create the database and roles only on a new, approved server; do not run against Cerebro.
-- CREATE ROLE ddhi_migration LOGIN;
-- CREATE ROLE ddhi_runtime LOGIN;
-- CREATE DATABASE ddhi_pharmacy OWNER ddhi_migration;
-- Connect to ddhi_pharmacy as schema owner before applying migration.
GRANT CONNECT ON DATABASE ddhi_pharmacy TO ddhi_runtime;
GRANT USAGE ON SCHEMA public TO ddhi_runtime;
GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA public TO ddhi_runtime;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO ddhi_runtime;
REVOKE UPDATE, DELETE, TRUNCATE ON "StockMovements", "AuditEvents", "OperationKeys" FROM ddhi_runtime;
-- Account management removes account-private identity records only; clinical history remains protected.
GRANT DELETE ON "AspNetUsers", "AspNetUserClaims", "AspNetUserRoles", "AspNetUserLogins", "AspNetUserTokens" TO ddhi_runtime;
-- Do not grant DELETE/TRUNCATE or schema ownership to the runtime account.

-- Outside/uncertain patient intake is append-only and never a stock credit.
DO $$ BEGIN
 IF to_regclass('public."ExternalReturnIntakes"') IS NOT NULL THEN
  GRANT SELECT, INSERT ON "ExternalReturnIntakes" TO ddhi_runtime;
  REVOKE UPDATE, DELETE, TRUNCATE ON "ExternalReturnIntakes" FROM ddhi_runtime;
 END IF;
END $$;
