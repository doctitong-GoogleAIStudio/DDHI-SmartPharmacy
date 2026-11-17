using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MixedSourceReturnIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HospitalEvidenceReference",
                table: "Returns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HospitalSourceVerified",
                table: "Returns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExternalReturnIntakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EncounterReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PackageCondition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Lot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Expiration = table.Column<DateOnly>(type: "date", nullable: true),
                    Barcode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReceiptReference = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CustodyDisposition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecordedBy = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalReturnIntakes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReturnIntakes_Reference",
                table: "ExternalReturnIntakes",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalReturnIntakes");

            migrationBuilder.DropColumn(
                name: "HospitalEvidenceReference",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "HospitalSourceVerified",
                table: "Returns");
        }
    }
}
