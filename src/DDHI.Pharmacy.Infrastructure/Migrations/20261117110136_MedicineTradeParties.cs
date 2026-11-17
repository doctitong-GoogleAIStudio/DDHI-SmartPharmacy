using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MedicineTradeParties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistributedBy",
                table: "Medicines",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImportedBy",
                table: "Medicines",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistributedBy",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ImportedBy",
                table: "Medicines");
        }
    }
}
