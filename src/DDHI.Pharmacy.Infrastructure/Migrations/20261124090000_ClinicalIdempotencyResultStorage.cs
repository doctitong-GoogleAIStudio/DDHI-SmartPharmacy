using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDHI.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClinicalIdempotencyResultStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "OperationKeys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)=>throw new InvalidOperationException("Idempotency history cannot be truncated by downgrade. Use approved backup recovery.");
    }
}
