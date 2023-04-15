using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ManualPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentsFiles_PaymentsFileId",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "ReadableIdentifier",
                table: "Registrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentsFileId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EventId",
                table: "Payments",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Events_EventId",
                table: "Payments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentsFiles_PaymentsFileId",
                table: "Payments",
                column: "PaymentsFileId",
                principalTable: "PaymentsFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Events_EventId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentsFiles_PaymentsFileId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EventId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReadableIdentifier",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Payments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentsFileId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentsFiles_PaymentsFileId",
                table: "Payments",
                column: "PaymentsFileId",
                principalTable: "PaymentsFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
