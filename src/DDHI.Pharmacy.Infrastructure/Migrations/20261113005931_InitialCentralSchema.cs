using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCentralSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Role = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Workstation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Action = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PreviousHash = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Hash = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationInbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationInbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CerebroMedicineId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GenericName = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Strength = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DosageForm = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Route = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BaseUnit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PackagingUnit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    ActiveStatus = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Result = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecallNotices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Closed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallNotices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workstations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workstations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CerebroMedicineMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CerebroMedicineMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CerebroMedicineMappings_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispenseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EncounterReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Dispensed = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseOrders", x => x.Id);
                    table.CheckConstraint("CK_Order_Quantity", "\"Quantity\" > 0 AND \"Dispensed\" >= 0 AND \"Dispensed\" <= \"Quantity\"");
                    table.ForeignKey(
                        name: "FK_DispenseOrders_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Lot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Expiration = table.Column<DateOnly>(type: "date", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBatches_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicineBarcodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Packaging = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineBarcodes", x => x.Id);
                    table.CheckConstraint("CK_Barcode_Factor", "\"ConversionFactor\" > 0");
                    table.ForeignKey(
                        name: "FK_MedicineBarcodes_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagingUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Factor = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingUnits_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScannerConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkstationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Port = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Suffix = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LastSuccessfulScan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScannerConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScannerConfigurations_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScannerEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkstationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Symbology = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScannerEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScannerEvents_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    WorkstationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CerebroOrderMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CerebroOrderMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CerebroOrderMappings_DispenseOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DispenseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispenseTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Verification = table.Column<int>(type: "integer", nullable: false),
                    Pharmacist = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseTransactions_DispenseOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DispenseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBalances", x => x.Id);
                    table.CheckConstraint("CK_Balance_Nonnegative", "\"Quantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryBalances_InventoryBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "InventoryBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryBalances_InventoryLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecallAffectedBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallAffectedBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecallAffectedBatches_InventoryBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "InventoryBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecallAffectedBatches_RecallNotices_RecallId",
                        column: x => x.RecallId,
                        principalTable: "RecallNotices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationOutbox_DispenseTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "DispenseTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispenseTransactionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseTransactionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseTransactionLines_DispenseTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "DispenseTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispenseTransactionLines_InventoryBalances_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "InventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationDiscrepancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expected = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Actual = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationDiscrepancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationDiscrepancies_InventoryBalances_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "InventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationDiscrepancies_ReconciliationRuns_RunId",
                        column: x => x.RunId,
                        principalTable: "ReconciliationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Delta = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Kind = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_InventoryBalances_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "InventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Released = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReservations_DispenseOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DispenseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_InventoryBalances_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "InventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationAcknowledgments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationAcknowledgments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationAcknowledgments_IntegrationOutbox_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "IntegrationOutbox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DispenseLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Returns_DispenseTransactionLines_DispenseLineId",
                        column: x => x.DispenseLineId,
                        principalTable: "DispenseTransactionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_StockMovements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "StockMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Counted = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_StockMovements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "StockMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CreatedAt",
                table: "AuditEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CerebroMedicineMappings_ExternalId",
                table: "CerebroMedicineMappings",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CerebroMedicineMappings_MedicineId",
                table: "CerebroMedicineMappings",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_CerebroOrderMappings_ExternalId",
                table: "CerebroOrderMappings",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CerebroOrderMappings_OrderId",
                table: "CerebroOrderMappings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseOrders_MedicineId",
                table: "DispenseOrders",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseOrders_Reference",
                table: "DispenseOrders",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispenseTransactionLines_BalanceId",
                table: "DispenseTransactionLines",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseTransactionLines_TransactionId",
                table: "DispenseTransactionLines",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseTransactions_OrderId",
                table: "DispenseTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseTransactions_Reference",
                table: "DispenseTransactions",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_MovementId",
                table: "GoodsReceipts",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_PurchaseOrderId",
                table: "GoodsReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_SupplierId_Reference",
                table: "GoodsReceipts",
                columns: new[] { "SupplierId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationAcknowledgments_OutboxId",
                table: "IntegrationAcknowledgments",
                column: "OutboxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationInbox_ExternalId",
                table: "IntegrationInbox",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutbox_TransactionId",
                table: "IntegrationOutbox",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_BatchId_LocationId_Status",
                table: "InventoryBalances",
                columns: new[] { "BatchId", "LocationId", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_LocationId",
                table: "InventoryBalances",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_Expiration",
                table: "InventoryBatches",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_MedicineId_Lot_Expiration",
                table: "InventoryBatches",
                columns: new[] { "MedicineId", "Lot", "Expiration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_Name",
                table: "InventoryLocations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBarcodes_Barcode",
                table: "MedicineBarcodes",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBarcodes_MedicineId",
                table: "MedicineBarcodes",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_CerebroMedicineId",
                table: "Medicines",
                column: "CerebroMedicineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationKeys_Key",
                table: "OperationKeys",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagingUnits_MedicineId",
                table: "PackagingUnits",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId_Reference",
                table: "PurchaseOrders",
                columns: new[] { "SupplierId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecallAffectedBatches_BatchId",
                table: "RecallAffectedBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RecallAffectedBatches_RecallId_BatchId",
                table: "RecallAffectedBatches",
                columns: new[] { "RecallId", "BatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecallNotices_Reference",
                table: "RecallNotices",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationDiscrepancies_BalanceId",
                table: "ReconciliationDiscrepancies",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationDiscrepancies_RunId",
                table: "ReconciliationDiscrepancies",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_DispenseLineId",
                table: "Returns",
                column: "DispenseLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScannerConfigurations_WorkstationId_Name",
                table: "ScannerConfigurations",
                columns: new[] { "WorkstationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScannerEvents_WorkstationId",
                table: "ScannerEvents",
                column: "WorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_MovementId",
                table: "StockAdjustments",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BalanceId",
                table: "StockMovements",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_Reference",
                table: "StockMovements",
                column: "Reference");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_BalanceId",
                table: "StockReservations",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId",
                table: "StockReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Name",
                table: "SystemSettings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_WorkstationId",
                table: "UserSessions",
                column: "WorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_Name",
                table: "Workstations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "CerebroMedicineMappings");

            migrationBuilder.DropTable(
                name: "CerebroOrderMappings");

            migrationBuilder.DropTable(
                name: "GoodsReceipts");

            migrationBuilder.DropTable(
                name: "IntegrationAcknowledgments");

            migrationBuilder.DropTable(
                name: "IntegrationInbox");

            migrationBuilder.DropTable(
                name: "MedicineBarcodes");

            migrationBuilder.DropTable(
                name: "OperationKeys");

            migrationBuilder.DropTable(
                name: "PackagingUnits");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RecallAffectedBatches");

            migrationBuilder.DropTable(
                name: "ReconciliationDiscrepancies");

            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.DropTable(
                name: "ScannerConfigurations");

            migrationBuilder.DropTable(
                name: "ScannerEvents");

            migrationBuilder.DropTable(
                name: "StockAdjustments");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "IntegrationOutbox");

            migrationBuilder.DropTable(
                name: "RecallNotices");

            migrationBuilder.DropTable(
                name: "ReconciliationRuns");

            migrationBuilder.DropTable(
                name: "DispenseTransactionLines");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Workstations");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "DispenseTransactions");

            migrationBuilder.DropTable(
                name: "InventoryBalances");

            migrationBuilder.DropTable(
                name: "DispenseOrders");

            migrationBuilder.DropTable(
                name: "InventoryBatches");

            migrationBuilder.DropTable(
                name: "InventoryLocations");

            migrationBuilder.DropTable(
                name: "Medicines");
        }
    }
}
