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

