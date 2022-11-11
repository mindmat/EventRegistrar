using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class RawRegistrationLastProcessingError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastProcessingError",
                table: "RawRegistrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastProcessingError",
                table: "RawRegistrations");
        }
    }
}
