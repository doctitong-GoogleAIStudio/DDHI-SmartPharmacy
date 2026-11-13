using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockMovementReversalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReversesMovementId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReversesMovementId",
                table: "StockMovements",
                column: "ReversesMovementId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockMovements_ReversesMovementId",
                table: "StockMovements",
                column: "ReversesMovementId",
                principalTable: "StockMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockMovements_ReversesMovementId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ReversesMovementId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReversesMovementId",
                table: "StockMovements");
        }
    }
}
