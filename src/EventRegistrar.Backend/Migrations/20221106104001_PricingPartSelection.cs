using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class PricingPartSelection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "PricePackagePart");

            migrationBuilder.RenameColumn(
                name: "Reduction",
                table: "PricePackagePart",
                newName: "PriceAdjustment");

            migrationBuilder.AddColumn<int>(
                name: "SelectionType",
                table: "PricePackagePart",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectionType",
                table: "PricePackagePart");

            migrationBuilder.RenameColumn(
                name: "PriceAdjustment",
                table: "PricePackagePart",
                newName: "Reduction");

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "PricePackagePart",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
