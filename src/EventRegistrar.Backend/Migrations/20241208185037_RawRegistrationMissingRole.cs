using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RawRegistrationMissingRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "RawRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "RawRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mail",
                table: "RawRegistrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "RawRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RoleMissing",
                table: "RawRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoleOverride",
                table: "RawRegistrations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "RawRegistrations");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "RawRegistrations");

            migrationBuilder.DropColumn(
                name: "Mail",
                table: "RawRegistrations");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "RawRegistrations");

            migrationBuilder.DropColumn(
                name: "RoleMissing",
                table: "RawRegistrations");

            migrationBuilder.DropColumn(
                name: "RoleOverride",
                table: "RawRegistrations");
        }
    }
}
