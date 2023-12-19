using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleManualFallbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_PricePackages_PricePackageId_ManualFallback",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_PricePackageId_ManualFallback",
                table: "Registrations");

            migrationBuilder.AlterColumn<string>(
                name: "PricePackageIds_Admitted",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricePackageIds_ManualFallback",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePackageIds_ManualFallback",
                table: "Registrations");

            migrationBuilder.AlterColumn<string>(
                name: "PricePackageIds_Admitted",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_PricePackageId_ManualFallback",
                table: "Registrations",
                column: "PricePackageId_ManualFallback");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_PricePackages_PricePackageId_ManualFallback",
                table: "Registrations",
                column: "PricePackageId_ManualFallback",
                principalTable: "PricePackages",
                principalColumn: "Id");
        }
    }
}
