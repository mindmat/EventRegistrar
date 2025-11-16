using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDebitorDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DebitorBuildingNr",
                table: "IncomingPayments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebitorCountry",
                table: "IncomingPayments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebitorStreet",
                table: "IncomingPayments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebitorTown",
                table: "IncomingPayments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebitorZip",
                table: "IncomingPayments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebitorBuildingNr",
                table: "IncomingPayments");

            migrationBuilder.DropColumn(
                name: "DebitorCountry",
                table: "IncomingPayments");

            migrationBuilder.DropColumn(
                name: "DebitorStreet",
                table: "IncomingPayments");

            migrationBuilder.DropColumn(
                name: "DebitorTown",
                table: "IncomingPayments");

            migrationBuilder.DropColumn(
                name: "DebitorZip",
                table: "IncomingPayments");
        }
    }
}
