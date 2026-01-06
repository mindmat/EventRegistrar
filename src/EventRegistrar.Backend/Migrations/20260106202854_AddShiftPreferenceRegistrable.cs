using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftPreferenceRegistrable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RegistrableId_ShiftPreference",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_RegistrableId_ShiftPreference",
                table: "Shifts",
                column: "RegistrableId_ShiftPreference");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Registrables_RegistrableId_ShiftPreference",
                table: "Shifts",
                column: "RegistrableId_ShiftPreference",
                principalTable: "Registrables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Registrables_RegistrableId_ShiftPreference",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_RegistrableId_ShiftPreference",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "RegistrableId_ShiftPreference",
                table: "Shifts");
        }
    }
}
