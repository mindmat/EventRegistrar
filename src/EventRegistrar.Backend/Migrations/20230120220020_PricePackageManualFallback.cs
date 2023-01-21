using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class PricePackageManualFallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAsManualFallback",
                table: "PricePackages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAsManualFallback",
                table: "PricePackages");
        }
    }
}
