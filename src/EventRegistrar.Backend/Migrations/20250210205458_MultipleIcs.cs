using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class MultipleIcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrablesIcs_Registrables_Id",
                table: "RegistrablesIcs");

            migrationBuilder.AddColumn<Guid>(
                name: "RegistrableId",
                table: "RegistrablesIcs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RegistrablesIcs_RegistrableId",
                table: "RegistrablesIcs",
                column: "RegistrableId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrablesIcs_Registrables_RegistrableId",
                table: "RegistrablesIcs",
                column: "RegistrableId",
                principalTable: "Registrables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrablesIcs_Registrables_RegistrableId",
                table: "RegistrablesIcs");

            migrationBuilder.DropIndex(
                name: "IX_RegistrablesIcs_RegistrableId",
                table: "RegistrablesIcs");

            migrationBuilder.DropColumn(
                name: "RegistrableId",
                table: "RegistrablesIcs");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrablesIcs_Registrables_Id",
                table: "RegistrablesIcs",
                column: "Id",
                principalTable: "Registrables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
