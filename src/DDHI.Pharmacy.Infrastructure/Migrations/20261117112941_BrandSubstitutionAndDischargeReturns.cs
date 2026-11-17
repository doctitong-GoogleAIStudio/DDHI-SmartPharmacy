using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BrandSubstitutionAndDischargeReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DischargeReference",
                table: "Returns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DischargeReturn",
                table: "Returns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MovementId",
                table: "Returns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageCondition",
                table: "Returns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Returns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UnusedVerified",
                table: "Returns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BrandSubstituted",
                table: "DispenseTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DispensedBrandName",
                table: "DispenseTransactions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DispensedMedicineId",
                table: "DispenseTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedBrandName",
                table: "DispenseTransactions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedMedicineId",
                table: "DispenseTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubstitutionApprovalReference",
                table: "DispenseTransactions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubstitutionReason",
                table: "DispenseTransactions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_Reference",
                table: "Returns",
                column: "Reference",
                unique: true,
                filter: "\"Reference\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Returns_Reference",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "DischargeReference",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "DischargeReturn",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "MovementId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "PackageCondition",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "UnusedVerified",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "BrandSubstituted",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "DispensedBrandName",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "DispensedMedicineId",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "RequestedBrandName",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "RequestedMedicineId",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "SubstitutionApprovalReference",
                table: "DispenseTransactions");

            migrationBuilder.DropColumn(
                name: "SubstitutionReason",
                table: "DispenseTransactions");
        }
    }
}
