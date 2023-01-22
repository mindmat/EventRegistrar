using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class RegistrationManualFallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PricePackageId_ManualFallback",
                table: "Registrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_PricePackageId_ManualFallback",
                table: "Registrations",
                column: "PricePackageId_ManualFallback");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_PricePackages_PricePackageId_ManualFallback",
                table: "Registrations",
                column: "PricePackageId_ManualFallback",
                principalTable: "PricePackages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_PricePackages_PricePackageId_ManualFallback",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_PricePackageId_ManualFallback",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PricePackageId_ManualFallback",
                table: "Registrations");
        }
    }
}
