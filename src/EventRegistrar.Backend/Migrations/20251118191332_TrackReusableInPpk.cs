using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class TrackReusableInPpk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Registrables");

            migrationBuilder.DropColumn(
                name: "ReducedPrice",
                table: "Registrables");

            migrationBuilder.AddColumn<bool>(
                name: "IsReusableInPricePackages",
                table: "Registrables",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReusableInPricePackages",
                table: "Registrables");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Registrables",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReducedPrice",
                table: "Registrables",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
