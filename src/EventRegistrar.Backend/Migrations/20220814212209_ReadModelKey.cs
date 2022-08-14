using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class ReadModelKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReadModels",
                table: "ReadModels");

            migrationBuilder.DropIndex(
                name: "IX_ReadModels_Sequence",
                table: "ReadModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "RowId",
                table: "ReadModels",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReadModels",
                table: "ReadModels",
                column: "Sequence")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadModels_QueryName_EventId_RowId",
                table: "ReadModels",
                columns: new[] { "QueryName", "EventId", "RowId" },
                unique: true,
                filter: "[RowId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReadModels",
                table: "ReadModels");

            migrationBuilder.DropIndex(
                name: "IX_ReadModels_QueryName_EventId_RowId",
                table: "ReadModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "RowId",
                table: "ReadModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReadModels",
                table: "ReadModels",
                columns: new[] { "QueryName", "EventId", "RowId" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ReadModels_Sequence",
                table: "ReadModels",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
