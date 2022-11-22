using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class RegistrationPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Registrations");

            migrationBuilder.RenameColumn(
                name: "IsWaitingList",
                table: "Registrations",
                newName: "IsOnWaitingList");

            migrationBuilder.AddColumn<decimal>(
                name: "Price_Admitted",
                table: "Registrations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price_AdmittedAndReduced",
                table: "Registrations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price_Original",
                table: "Registrations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "IndividualReductions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_Admitted",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Price_AdmittedAndReduced",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Price_Original",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "IndividualReductions");

            migrationBuilder.RenameColumn(
                name: "IsOnWaitingList",
                table: "Registrations",
                newName: "IsWaitingList");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Registrations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Registrations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
