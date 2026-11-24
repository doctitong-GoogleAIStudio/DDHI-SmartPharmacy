CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetRoles" (
        "Id" text NOT NULL,
        "Name" character varying(256),
        "NormalizedName" character varying(256),
        "ConcurrencyStamp" text,
        CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetUsers" (
        "Id" text NOT NULL,
        "UserName" character varying(256),
        "NormalizedUserName" character varying(256),
        "Email" character varying(256),
        "NormalizedEmail" character varying(256),
        "EmailConfirmed" boolean NOT NULL,
        "PasswordHash" text,
        "SecurityStamp" text,
        "ConcurrencyStamp" text,
        "PhoneNumber" text,
        "PhoneNumberConfirmed" boolean NOT NULL,
        "TwoFactorEnabled" boolean NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "LockoutEnabled" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AuditEvents" (
        "Id" uuid NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "Role" character varying(2000) NOT NULL,
        "Workstation" character varying(2000) NOT NULL,
        "Action" character varying(2000) NOT NULL,
        "RecordId" character varying(2000) NOT NULL,
        "Detail" character varying(2000) NOT NULL,
        "PreviousHash" character varying(2000) NOT NULL,
        "Hash" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "IntegrationInbox" (
        "Id" uuid NOT NULL,
        "ExternalId" character varying(2000) NOT NULL,
        "PayloadHash" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_IntegrationInbox" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "InventoryLocations" (
        "Id" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InventoryLocations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "Medicines" (
        "Id" uuid NOT NULL,
        "CerebroMedicineId" character varying(2000),
        "GenericName" character varying(2000) NOT NULL,
        "BrandName" character varying(2000) NOT NULL,
        "Strength" character varying(2000) NOT NULL,
        "DosageForm" character varying(2000) NOT NULL,
        "Route" character varying(2000) NOT NULL,
        "Manufacturer" character varying(2000) NOT NULL,
        "Category" character varying(2000) NOT NULL,
        "BaseUnit" character varying(2000) NOT NULL,
        "PackagingUnit" character varying(2000) NOT NULL,
        "ConversionFactor" numeric(20,4) NOT NULL,
        "ReorderLevel" numeric(20,4) NOT NULL,
        "ActiveStatus" boolean NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Medicines" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "OperationKeys" (
        "Id" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "PayloadHash" character varying(2000) NOT NULL,
        "Result" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OperationKeys" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Role" character varying(2000) NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "RecallNotices" (
        "Id" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "Closed" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RecallNotices" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "ReconciliationRuns" (
        "Id" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ReconciliationRuns" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "Suppliers" (
        "Id" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Suppliers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "SystemSettings" (
        "Id" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "Value" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_SystemSettings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "Workstations" (
        "Id" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "LastSeen" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Workstations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetRoleClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" text NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetUserClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "UserId" text NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetUserLogins" (
        "LoginProvider" text NOT NULL,
        "ProviderKey" text NOT NULL,
        "ProviderDisplayName" text,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
        CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetUserRoles" (
        "UserId" text NOT NULL,
        "RoleId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "AspNetUserTokens" (
        "UserId" text NOT NULL,
        "LoginProvider" text NOT NULL,
        "Name" text NOT NULL,
        "Value" text,
        CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "CerebroMedicineMappings" (
        "Id" uuid NOT NULL,
        "MedicineId" uuid NOT NULL,
        "ExternalId" character varying(2000) NOT NULL,
        "Approved" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CerebroMedicineMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CerebroMedicineMappings_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "DispenseOrders" (
        "Id" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "EncounterReference" character varying(2000) NOT NULL,
        "MedicineId" uuid NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "Dispensed" numeric(20,4) NOT NULL,
        "Active" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DispenseOrders" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Order_Quantity" CHECK ("Quantity" > 0 AND "Dispensed" >= 0 AND "Dispensed" <= "Quantity"),
        CONSTRAINT "FK_DispenseOrders_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "InventoryBatches" (
        "Id" uuid NOT NULL,
        "MedicineId" uuid NOT NULL,
        "Lot" character varying(2000) NOT NULL,
        "Expiration" date NOT NULL,
        "UnitCost" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InventoryBatches" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InventoryBatches_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "MedicineBarcodes" (
        "Id" uuid NOT NULL,
        "MedicineId" uuid NOT NULL,
        "Barcode" character varying(2000) NOT NULL,
        "Packaging" character varying(2000) NOT NULL,
        "ConversionFactor" numeric(20,4) NOT NULL,
        "ApprovedBy" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_MedicineBarcodes" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Barcode_Factor" CHECK ("ConversionFactor" > 0),
        CONSTRAINT "FK_MedicineBarcodes_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "PackagingUnits" (
        "Id" uuid NOT NULL,
        "MedicineId" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "Factor" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PackagingUnits" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PackagingUnits_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "PurchaseOrders" (
        "Id" uuid NOT NULL,
        "SupplierId" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "OrderedQuantity" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PurchaseOrders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PurchaseOrders_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "ScannerConfigurations" (
        "Id" uuid NOT NULL,
        "WorkstationId" uuid NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "Enabled" boolean NOT NULL,
        "Type" character varying(2000) NOT NULL,
        "Port" character varying(2000) NOT NULL,
        "Suffix" character varying(2000) NOT NULL,
        "LastSuccessfulScan" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ScannerConfigurations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ScannerConfigurations_Workstations_WorkstationId" FOREIGN KEY ("WorkstationId") REFERENCES "Workstations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "ScannerEvents" (
        "Id" uuid NOT NULL,
        "WorkstationId" uuid NOT NULL,
        "Mode" character varying(2000) NOT NULL,
        "Symbology" character varying(2000) NOT NULL,
        "Success" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ScannerEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ScannerEvents_Workstations_WorkstationId" FOREIGN KEY ("WorkstationId") REFERENCES "Workstations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "UserSessions" (
        "Id" uuid NOT NULL,
        "UserId" character varying(2000) NOT NULL,
        "WorkstationId" uuid NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_UserSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserSessions_Workstations_WorkstationId" FOREIGN KEY ("WorkstationId") REFERENCES "Workstations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "CerebroOrderMappings" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "ExternalId" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CerebroOrderMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CerebroOrderMappings_DispenseOrders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "DispenseOrders" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "DispenseTransactions" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "Verification" integer NOT NULL,
        "Pharmacist" character varying(2000) NOT NULL,
        "OverrideReason" character varying(2000) NOT NULL,
        "State" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DispenseTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DispenseTransactions_DispenseOrders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "DispenseOrders" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "InventoryBalances" (
        "Id" uuid NOT NULL,
        "BatchId" uuid NOT NULL,
        "LocationId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InventoryBalances" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Balance_Nonnegative" CHECK ("Quantity" >= 0),
        CONSTRAINT "FK_InventoryBalances_InventoryBatches_BatchId" FOREIGN KEY ("BatchId") REFERENCES "InventoryBatches" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_InventoryBalances_InventoryLocations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "InventoryLocations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "RecallAffectedBatches" (
        "Id" uuid NOT NULL,
        "RecallId" uuid NOT NULL,
        "BatchId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RecallAffectedBatches" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RecallAffectedBatches_InventoryBatches_BatchId" FOREIGN KEY ("BatchId") REFERENCES "InventoryBatches" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RecallAffectedBatches_RecallNotices_RecallId" FOREIGN KEY ("RecallId") REFERENCES "RecallNotices" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "IntegrationOutbox" (
        "Id" uuid NOT NULL,
        "TransactionId" uuid NOT NULL,
        "State" integer NOT NULL,
        "Detail" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_IntegrationOutbox" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IntegrationOutbox_DispenseTransactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "DispenseTransactions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "DispenseTransactionLines" (
        "Id" uuid NOT NULL,
        "TransactionId" uuid NOT NULL,
        "BalanceId" uuid NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DispenseTransactionLines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DispenseTransactionLines_DispenseTransactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "DispenseTransactions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DispenseTransactionLines_InventoryBalances_BalanceId" FOREIGN KEY ("BalanceId") REFERENCES "InventoryBalances" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "ReconciliationDiscrepancies" (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "BalanceId" uuid NOT NULL,
        "Expected" numeric(20,4) NOT NULL,
        "Actual" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ReconciliationDiscrepancies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ReconciliationDiscrepancies_InventoryBalances_BalanceId" FOREIGN KEY ("BalanceId") REFERENCES "InventoryBalances" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ReconciliationDiscrepancies_ReconciliationRuns_RunId" FOREIGN KEY ("RunId") REFERENCES "ReconciliationRuns" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "StockMovements" (
        "Id" uuid NOT NULL,
        "BalanceId" uuid NOT NULL,
        "Delta" numeric(20,4) NOT NULL,
        "Kind" character varying(2000) NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_StockMovements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StockMovements_InventoryBalances_BalanceId" FOREIGN KEY ("BalanceId") REFERENCES "InventoryBalances" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "StockReservations" (
        "Id" uuid NOT NULL,
        "BalanceId" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "Released" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_StockReservations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StockReservations_DispenseOrders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "DispenseOrders" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_StockReservations_InventoryBalances_BalanceId" FOREIGN KEY ("BalanceId") REFERENCES "InventoryBalances" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "IntegrationAcknowledgments" (
        "Id" uuid NOT NULL,
        "OutboxId" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_IntegrationAcknowledgments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IntegrationAcknowledgments_IntegrationOutbox_OutboxId" FOREIGN KEY ("OutboxId") REFERENCES "IntegrationOutbox" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "Returns" (
        "Id" uuid NOT NULL,
        "DispenseLineId" uuid NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Returns" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Returns_DispenseTransactionLines_DispenseLineId" FOREIGN KEY ("DispenseLineId") REFERENCES "DispenseTransactionLines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "GoodsReceipts" (
        "Id" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "SupplierId" uuid NOT NULL,
        "PurchaseOrderId" uuid,
        "MovementId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_GoodsReceipts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_GoodsReceipts_StockMovements_MovementId" FOREIGN KEY ("MovementId") REFERENCES "StockMovements" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_GoodsReceipts_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE TABLE "StockAdjustments" (
        "Id" uuid NOT NULL,
        "MovementId" uuid NOT NULL,
        "Counted" numeric(20,4) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_StockAdjustments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StockAdjustments_StockMovements_MovementId" FOREIGN KEY ("MovementId") REFERENCES "StockMovements" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_AuditEvents_CreatedAt" ON "AuditEvents" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_CerebroMedicineMappings_ExternalId" ON "CerebroMedicineMappings" ("ExternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_CerebroMedicineMappings_MedicineId" ON "CerebroMedicineMappings" ("MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_CerebroOrderMappings_ExternalId" ON "CerebroOrderMappings" ("ExternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_CerebroOrderMappings_OrderId" ON "CerebroOrderMappings" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_DispenseOrders_MedicineId" ON "DispenseOrders" ("MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_DispenseOrders_Reference" ON "DispenseOrders" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_DispenseTransactionLines_BalanceId" ON "DispenseTransactionLines" ("BalanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_DispenseTransactionLines_TransactionId" ON "DispenseTransactionLines" ("TransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_DispenseTransactions_OrderId" ON "DispenseTransactions" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_DispenseTransactions_Reference" ON "DispenseTransactions" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_GoodsReceipts_MovementId" ON "GoodsReceipts" ("MovementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_GoodsReceipts_PurchaseOrderId" ON "GoodsReceipts" ("PurchaseOrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_GoodsReceipts_SupplierId_Reference" ON "GoodsReceipts" ("SupplierId", "Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_IntegrationAcknowledgments_OutboxId" ON "IntegrationAcknowledgments" ("OutboxId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_IntegrationInbox_ExternalId" ON "IntegrationInbox" ("ExternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_IntegrationOutbox_TransactionId" ON "IntegrationOutbox" ("TransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_InventoryBalances_BatchId_LocationId_Status" ON "InventoryBalances" ("BatchId", "LocationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_InventoryBalances_LocationId" ON "InventoryBalances" ("LocationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_InventoryBatches_Expiration" ON "InventoryBatches" ("Expiration");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_InventoryBatches_MedicineId_Lot_Expiration" ON "InventoryBatches" ("MedicineId", "Lot", "Expiration");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_InventoryLocations_Name" ON "InventoryLocations" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_MedicineBarcodes_Barcode" ON "MedicineBarcodes" ("Barcode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_MedicineBarcodes_MedicineId" ON "MedicineBarcodes" ("MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_Medicines_CerebroMedicineId" ON "Medicines" ("CerebroMedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_OperationKeys_Key" ON "OperationKeys" ("Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_PackagingUnits_MedicineId" ON "PackagingUnits" ("MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_PurchaseOrders_SupplierId_Reference" ON "PurchaseOrders" ("SupplierId", "Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_RecallAffectedBatches_BatchId" ON "RecallAffectedBatches" ("BatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_RecallAffectedBatches_RecallId_BatchId" ON "RecallAffectedBatches" ("RecallId", "BatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_RecallNotices_Reference" ON "RecallNotices" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_ReconciliationDiscrepancies_BalanceId" ON "ReconciliationDiscrepancies" ("BalanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_ReconciliationDiscrepancies_RunId" ON "ReconciliationDiscrepancies" ("RunId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_Returns_DispenseLineId" ON "Returns" ("DispenseLineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_ScannerConfigurations_WorkstationId_Name" ON "ScannerConfigurations" ("WorkstationId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_ScannerEvents_WorkstationId" ON "ScannerEvents" ("WorkstationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_StockAdjustments_MovementId" ON "StockAdjustments" ("MovementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_StockMovements_BalanceId" ON "StockMovements" ("BalanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_StockMovements_Reference" ON "StockMovements" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_StockReservations_BalanceId" ON "StockReservations" ("BalanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_StockReservations_OrderId" ON "StockReservations" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_Suppliers_Name" ON "Suppliers" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_SystemSettings_Name" ON "SystemSettings" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE INDEX "IX_UserSessions_WorkstationId" ON "UserSessions" ("WorkstationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    CREATE UNIQUE INDEX "IX_Workstations_Name" ON "Workstations" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113005931_InitialCentralSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261113005931_InitialCentralSchema', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113010000_ImmutableLedger') THEN
    CREATE FUNCTION ddhi_reject_immutable_change() RETURNS trigger LANGUAGE plpgsql AS $$
    BEGIN RAISE EXCEPTION 'DDHI immutable records cannot be changed or deleted'; END; $$;
    CREATE TRIGGER immutable_stock BEFORE UPDATE OR DELETE OR TRUNCATE ON "StockMovements" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_audit BEFORE UPDATE OR DELETE OR TRUNCATE ON "AuditEvents" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_operations BEFORE UPDATE OR DELETE OR TRUNCATE ON "OperationKeys" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113010000_ImmutableLedger') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261113010000_ImmutableLedger', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113012058_StockMovementReversalLinks') THEN
    ALTER TABLE "StockMovements" ADD "ReversesMovementId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113012058_StockMovementReversalLinks') THEN
    CREATE UNIQUE INDEX "IX_StockMovements_ReversesMovementId" ON "StockMovements" ("ReversesMovementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113012058_StockMovementReversalLinks') THEN
    ALTER TABLE "StockMovements" ADD CONSTRAINT "FK_StockMovements_StockMovements_ReversesMovementId" FOREIGN KEY ("ReversesMovementId") REFERENCES "StockMovements" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261113012058_StockMovementReversalLinks') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261113012058_StockMovementReversalLinks', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117110136_MedicineTradeParties') THEN
    ALTER TABLE "Medicines" ADD "DistributedBy" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117110136_MedicineTradeParties') THEN
    ALTER TABLE "Medicines" ADD "ImportedBy" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117110136_MedicineTradeParties') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261117110136_MedicineTradeParties', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "DischargeReference" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "DischargeReturn" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "MovementId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "PackageCondition" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "Reference" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "Returns" ADD "UnusedVerified" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "BrandSubstituted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "DispensedBrandName" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "DispensedMedicineId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "RequestedBrandName" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "RequestedMedicineId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "SubstitutionApprovalReference" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    ALTER TABLE "DispenseTransactions" ADD "SubstitutionReason" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    CREATE UNIQUE INDEX "IX_Returns_Reference" ON "Returns" ("Reference") WHERE "Reference" <> '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117112941_BrandSubstitutionAndDischargeReturns') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261117112941_BrandSubstitutionAndDischargeReturns', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117113616_MixedSourceReturnIntake') THEN
    ALTER TABLE "Returns" ADD "HospitalEvidenceReference" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117113616_MixedSourceReturnIntake') THEN
    ALTER TABLE "Returns" ADD "HospitalSourceVerified" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117113616_MixedSourceReturnIntake') THEN
    CREATE TABLE "ExternalReturnIntakes" (
        "Id" uuid NOT NULL,
        "Reference" character varying(2000) NOT NULL,
        "EncounterReference" character varying(2000) NOT NULL,
        "Source" character varying(2000) NOT NULL,
        "ItemDescription" character varying(2000) NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "Unit" character varying(2000) NOT NULL,
        "PackageCondition" character varying(2000) NOT NULL,
        "Lot" character varying(2000) NOT NULL,
        "Expiration" date,
        "Barcode" character varying(2000) NOT NULL,
        "ReceiptReference" character varying(2000) NOT NULL,
        "CustodyDisposition" character varying(2000) NOT NULL,
        "Notes" character varying(2000) NOT NULL,
        "RecordedBy" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ExternalReturnIntakes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117113616_MixedSourceReturnIntake') THEN
    CREATE UNIQUE INDEX "IX_ExternalReturnIntakes_Reference" ON "ExternalReturnIntakes" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261117113616_MixedSourceReturnIntake') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261117113616_MixedSourceReturnIntake', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "Computer" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "Email" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "EndReason" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "EndedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "LastSeenAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "UserSessions" ADD "Roles" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "Changes" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "Computer" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "FormatVersion" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "IpAddress" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "Outcome" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "SessionId" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "StaffEmail" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "StaffName" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    ALTER TABLE "AuditEvents" ADD "StaffRoles" character varying(2000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261119095027_AuditIdentityAndTimedSessions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261119095027_AuditIdentityAndTimedSessions', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    ALTER TABLE "DispenseTransactions" ADD "SafetyAssessmentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalContentVersions" (
        "Id" uuid NOT NULL,
        "Version" character varying(2000) NOT NULL,
        "State" character varying(2000) NOT NULL,
        "Digest" character varying(2000) NOT NULL,
        "PayloadJson" text NOT NULL,
        "Author" character varying(2000) NOT NULL,
        "Reviewer" character varying(2000) NOT NULL,
        "Approver" character varying(2000) NOT NULL,
        "ApprovalReference" character varying(2000) NOT NULL,
        "ReviewedAt" timestamp with time zone,
        "ApprovedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalContentVersions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ReferencePreferences" (
        "Id" uuid NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "MedicineId" uuid NOT NULL,
        "Favorite" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ReferencePreferences" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalDrugs" (
        "Id" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "Strength" character varying(2000) NOT NULL,
        "Form" character varying(2000) NOT NULL,
        "Route" character varying(2000) NOT NULL,
        "SearchText" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalDrugs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalDrugs_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalPublications" (
        "Id" uuid NOT NULL,
        "Revision" bigint NOT NULL,
        "VersionId" uuid NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalPublications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalPublications_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalReviewRecords" (
        "Id" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "Action" character varying(2000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalReviewRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalReviewRecords_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalRuleRecords" (
        "Id" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "PairKey" character varying(2000) NOT NULL,
        "Kind" character varying(2000) NOT NULL,
        "Severity" character varying(2000) NOT NULL,
        "RuleJson" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalRuleRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalRuleRecords_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalSourceRecords" (
        "Id" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Organization" character varying(2000) NOT NULL,
        "Document" character varying(2000) NOT NULL,
        "Url" character varying(2000) NOT NULL,
        "LicenseBasis" character varying(2000) NOT NULL,
        "RevisionDate" date NOT NULL,
        "RetrievedDate" date NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalSourceRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalSourceRecords_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "DrugIngredients" (
        "Id" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Name" character varying(2000) NOT NULL,
        "SearchText" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DrugIngredients" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DrugIngredients_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalMonographRecords" (
        "Id" uuid NOT NULL,
        "DrugId" uuid NOT NULL,
        "SectionsJson" text NOT NULL,
        "ReviewDue" date NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalMonographRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalMonographRecords_ClinicalDrugs_DrugId" FOREIGN KEY ("DrugId") REFERENCES "ClinicalDrugs" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalProducts" (
        "Id" uuid NOT NULL,
        "DrugId" uuid NOT NULL,
        "VersionId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Brand" character varying(2000) NOT NULL,
        "SearchText" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalProducts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalProducts_ClinicalContentVersions_VersionId" FOREIGN KEY ("VersionId") REFERENCES "ClinicalContentVersions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ClinicalProducts_ClinicalDrugs_DrugId" FOREIGN KEY ("DrugId") REFERENCES "ClinicalDrugs" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalScreenings" (
        "Id" uuid NOT NULL,
        "PublicationId" uuid,
        "Actor" character varying(2000) NOT NULL,
        "Workstation" character varying(2000) NOT NULL,
        "EncounterReference" character varying(2000) NOT NULL,
        "MappingDigest" character varying(2000) NOT NULL,
        "InputJson" text NOT NULL,
        "ResolutionJson" text NOT NULL,
        "ResultJson" text NOT NULL,
        "Digest" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalScreenings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalScreenings_ClinicalPublications_PublicationId" FOREIGN KEY ("PublicationId") REFERENCES "ClinicalPublications" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalDrugIngredients" (
        "Id" uuid NOT NULL,
        "DrugId" uuid NOT NULL,
        "IngredientId" uuid NOT NULL,
        "Amount" numeric(20,6),
        "Unit" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalDrugIngredients" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalDrugIngredients_ClinicalDrugs_DrugId" FOREIGN KEY ("DrugId") REFERENCES "ClinicalDrugs" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ClinicalDrugIngredients_DrugIngredients_IngredientId" FOREIGN KEY ("IngredientId") REFERENCES "DrugIngredients" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "ClinicalPackageRecords" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Key" character varying(2000) NOT NULL,
        "Barcode" character varying(2000) NOT NULL,
        "Packaging" character varying(2000) NOT NULL,
        "UnitsPerPackage" numeric(20,4) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClinicalPackageRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClinicalPackageRecords_ClinicalProducts_ProductId" FOREIGN KEY ("ProductId") REFERENCES "ClinicalProducts" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "DrugClinicalMappings" (
        "Id" uuid NOT NULL,
        "MedicineId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ApprovedBy" character varying(2000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DrugClinicalMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DrugClinicalMappings_ClinicalProducts_ProductId" FOREIGN KEY ("ProductId") REFERENCES "ClinicalProducts" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DrugClinicalMappings_Medicines_MedicineId" FOREIGN KEY ("MedicineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TABLE "PharmacistInterventions" (
        "Id" uuid NOT NULL,
        "Revision" bigint NOT NULL,
        "ScreenId" uuid NOT NULL,
        "Actor" character varying(2000) NOT NULL,
        "Role" character varying(2000) NOT NULL,
        "Decision" character varying(2000) NOT NULL,
        "Management" character varying(2000) NOT NULL,
        "PrescriberClarification" character varying(2000) NOT NULL,
        "ApprovalReference" character varying(2000) NOT NULL,
        "ReviewedIncomplete" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PharmacistInterventions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PharmacistInterventions_ClinicalScreenings_ScreenId" FOREIGN KEY ("ScreenId") REFERENCES "ClinicalScreenings" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_DispenseTransactions_SafetyAssessmentId" ON "DispenseTransactions" ("SafetyAssessmentId") WHERE "SafetyAssessmentId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalContentVersions_Version" ON "ClinicalContentVersions" ("Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalDrugIngredients_DrugId_IngredientId" ON "ClinicalDrugIngredients" ("DrugId", "IngredientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalDrugIngredients_IngredientId" ON "ClinicalDrugIngredients" ("IngredientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalDrugs_VersionId_Key" ON "ClinicalDrugs" ("VersionId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalDrugs_VersionId_SearchText" ON "ClinicalDrugs" ("VersionId", "SearchText");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalMonographRecords_DrugId" ON "ClinicalMonographRecords" ("DrugId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalPackageRecords_Barcode" ON "ClinicalPackageRecords" ("Barcode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalPackageRecords_ProductId_Key" ON "ClinicalPackageRecords" ("ProductId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalProducts_DrugId" ON "ClinicalProducts" ("DrugId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalProducts_VersionId_Key" ON "ClinicalProducts" ("VersionId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalProducts_VersionId_SearchText" ON "ClinicalProducts" ("VersionId", "SearchText");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalPublications_Revision" ON "ClinicalPublications" ("Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalPublications_VersionId" ON "ClinicalPublications" ("VersionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalReviewRecords_VersionId" ON "ClinicalReviewRecords" ("VersionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalRuleRecords_VersionId_Key" ON "ClinicalRuleRecords" ("VersionId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalRuleRecords_VersionId_PairKey" ON "ClinicalRuleRecords" ("VersionId", "PairKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalScreenings_Actor_CreatedAt" ON "ClinicalScreenings" ("Actor", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_ClinicalScreenings_PublicationId" ON "ClinicalScreenings" ("PublicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ClinicalSourceRecords_VersionId_Key" ON "ClinicalSourceRecords" ("VersionId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_DrugClinicalMappings_MedicineId" ON "DrugClinicalMappings" ("MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_DrugClinicalMappings_ProductId" ON "DrugClinicalMappings" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_DrugIngredients_VersionId_Key" ON "DrugIngredients" ("VersionId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE INDEX "IX_DrugIngredients_VersionId_SearchText" ON "DrugIngredients" ("VersionId", "SearchText");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_PharmacistInterventions_ScreenId_Revision" ON "PharmacistInterventions" ("ScreenId", "Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE UNIQUE INDEX "IX_ReferencePreferences_Actor_MedicineId" ON "ReferencePreferences" ("Actor", "MedicineId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    ALTER TABLE "DispenseTransactions" ADD CONSTRAINT "FK_DispenseTransactions_ClinicalScreenings_SafetyAssessmentId" FOREIGN KEY ("SafetyAssessmentId") REFERENCES "ClinicalScreenings" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    CREATE TRIGGER immutable_clinical_0 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalSourceRecords" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_1 BEFORE UPDATE OR DELETE OR TRUNCATE ON "DrugIngredients" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_2 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalDrugs" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_3 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalDrugIngredients" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_4 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalProducts" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_5 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalPackageRecords" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_6 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalRuleRecords" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_7 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalMonographRecords" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_8 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalPublications" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_9 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalReviewRecords" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_10 BEFORE UPDATE OR DELETE OR TRUNCATE ON "ClinicalScreenings" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE TRIGGER immutable_clinical_11 BEFORE UPDATE OR DELETE OR TRUNCATE ON "PharmacistInterventions" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();
    CREATE FUNCTION ddhi_guard_clinical_version() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
    IF TG_OP='DELETE' OR OLD."State"='Published' OR NEW."PayloadJson" IS DISTINCT FROM OLD."PayloadJson" OR NEW."Digest" IS DISTINCT FROM OLD."Digest" OR NEW."Author" IS DISTINCT FROM OLD."Author" OR NEW."Version" IS DISTINCT FROM OLD."Version" THEN RAISE EXCEPTION 'Clinical content history is immutable'; END IF;
    RETURN NEW; END; $$;
    CREATE TRIGGER clinical_version_guard BEFORE UPDATE OR DELETE ON "ClinicalContentVersions" FOR EACH ROW EXECUTE FUNCTION ddhi_guard_clinical_version();
    CREATE TRIGGER clinical_version_no_truncate BEFORE TRUNCATE ON "ClinicalContentVersions" FOR EACH STATEMENT EXECUTE FUNCTION ddhi_reject_immutable_change();

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124084027_GovernedDrugReference') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261124084027_GovernedDrugReference', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124090000_ClinicalIdempotencyResultStorage') THEN
    ALTER TABLE "OperationKeys" ALTER COLUMN "Result" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124090000_ClinicalIdempotencyResultStorage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261124090000_ClinicalIdempotencyResultStorage', '10.0.15');
    END IF;
END $EF$;
COMMIT;

