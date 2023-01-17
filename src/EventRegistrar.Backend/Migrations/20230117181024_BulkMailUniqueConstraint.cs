using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class BulkMailUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BulkMailKey",
                table: "BulkMailTemplates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BulkMailTemplates_EventId_BulkMailKey_Language",
                table: "BulkMailTemplates",
                columns: new[] { "EventId", "BulkMailKey", "Language" },
                unique: true,
                filter: "[Language] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BulkMailTemplates_EventId_BulkMailKey_Language",
                table: "BulkMailTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "BulkMailKey",
                table: "BulkMailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
