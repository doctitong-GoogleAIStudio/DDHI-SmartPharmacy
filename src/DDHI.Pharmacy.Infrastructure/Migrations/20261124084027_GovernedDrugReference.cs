using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GovernedDrugReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SafetyAssessmentId",
                table: "DispenseTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicalContentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    State = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Digest = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reviewer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Approver = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApprovalReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalContentVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferencePreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Favorite = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferencePreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalDrugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Strength = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Form = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Route = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SearchText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalDrugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalDrugs_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalPublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalPublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalPublications_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalReviewRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Action = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalReviewRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalReviewRecords_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalRuleRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PairKey = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Kind = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RuleJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalRuleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalRuleRecords_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalSourceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Organization = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Document = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LicenseBasis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RevisionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RetrievedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalSourceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalSourceRecords_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SearchText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugIngredients_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalMonographRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionsJson = table.Column<string>(type: "text", nullable: false),
                    ReviewDue = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalMonographRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalMonographRecords_ClinicalDrugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "ClinicalDrugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SearchText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalProducts_ClinicalContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ClinicalContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalProducts_ClinicalDrugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "ClinicalDrugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalScreenings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Workstation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EncounterReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MappingDigest = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    InputJson = table.Column<string>(type: "text", nullable: false),
                    ResolutionJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    Digest = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalScreenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalScreenings_ClinicalPublications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "ClinicalPublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalDrugIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    Unit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalDrugIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalDrugIngredients_ClinicalDrugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "ClinicalDrugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalDrugIngredients_DrugIngredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "DrugIngredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalPackageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Packaging = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UnitsPerPackage = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalPackageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalPackageRecords_ClinicalProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ClinicalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugClinicalMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClinicalMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugClinicalMappings_ClinicalProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ClinicalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrugClinicalMappings_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacistInterventions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    ScreenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Role = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Decision = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Management = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PrescriberClarification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApprovalReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReviewedIncomplete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacistInterventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacistInterventions_ClinicalScreenings_ScreenId",
                        column: x => x.ScreenId,
                        principalTable: "ClinicalScreenings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispenseTransactions_SafetyAssessmentId",
                table: "DispenseTransactions",
                column: "SafetyAssessmentId",
                unique: true,
                filter: "\"SafetyAssessmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalContentVersions_Version",
                table: "ClinicalContentVersions",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDrugIngredients_DrugId_IngredientId",
                table: "ClinicalDrugIngredients",
                columns: new[] { "DrugId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDrugIngredients_IngredientId",
                table: "ClinicalDrugIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDrugs_VersionId_Key",
                table: "ClinicalDrugs",
                columns: new[] { "VersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDrugs_VersionId_SearchText",
                table: "ClinicalDrugs",
                columns: new[] { "VersionId", "SearchText" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalMonographRecords_DrugId",
                table: "ClinicalMonographRecords",
                column: "DrugId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalPackageRecords_Barcode",
                table: "ClinicalPackageRecords",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalPackageRecords_ProductId_Key",
                table: "ClinicalPackageRecords",
                columns: new[] { "ProductId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProducts_DrugId",
                table: "ClinicalProducts",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProducts_VersionId_Key",
                table: "ClinicalProducts",
                columns: new[] { "VersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProducts_VersionId_SearchText",
                table: "ClinicalProducts",
                columns: new[] { "VersionId", "SearchText" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalPublications_Revision",
                table: "ClinicalPublications",
                column: "Revision",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalPublications_VersionId",
                table: "ClinicalPublications",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalReviewRecords_VersionId",
                table: "ClinicalReviewRecords",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalRuleRecords_VersionId_Key",
                table: "ClinicalRuleRecords",
                columns: new[] { "VersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalRuleRecords_VersionId_PairKey",
                table: "ClinicalRuleRecords",
                columns: new[] { "VersionId", "PairKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalScreenings_Actor_CreatedAt",
                table: "ClinicalScreenings",
                columns: new[] { "Actor", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalScreenings_PublicationId",
                table: "ClinicalScreenings",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalSourceRecords_VersionId_Key",
                table: "ClinicalSourceRecords",
                columns: new[] { "VersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugClinicalMappings_MedicineId",
                table: "DrugClinicalMappings",
                column: "MedicineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugClinicalMappings_ProductId",
                table: "DrugClinicalMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugIngredients_VersionId_Key",
                table: "DrugIngredients",
                columns: new[] { "VersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugIngredients_VersionId_SearchText",
                table: "DrugIngredients",
                columns: new[] { "VersionId", "SearchText" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacistInterventions_ScreenId_Revision",
                table: "PharmacistInterventions",
                columns: new[] { "ScreenId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferencePreferences_Actor_MedicineId",
                table: "ReferencePreferences",
                columns: new[] { "Actor", "MedicineId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DispenseTransactions_ClinicalScreenings_SafetyAssessmentId",
                table: "DispenseTransactions",
                column: "SafetyAssessmentId",
                principalTable: "ClinicalScreenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("""
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

""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)=>throw new InvalidOperationException("Clinical safety history downgrade refused. Use approved verified backup recovery.");
    }
}
