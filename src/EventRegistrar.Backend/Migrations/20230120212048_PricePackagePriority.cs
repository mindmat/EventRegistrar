using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class PricePackagePriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAsAutomaticFallback",
                table: "PricePackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FallbackPriority",
                table: "PricePackages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAsAutomaticFallback",
                table: "PricePackages");

            migrationBuilder.DropColumn(
                name: "FallbackPriority",
                table: "PricePackages");
        }
    }
}
