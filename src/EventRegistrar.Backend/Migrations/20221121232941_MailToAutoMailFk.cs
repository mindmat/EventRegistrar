using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class MailToAutoMailFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AutoMailTemplateId",
                table: "Mails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mails_AutoMailTemplateId",
                table: "Mails",
                column: "AutoMailTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mails_AutoMailTemplates_AutoMailTemplateId",
                table: "Mails",
                column: "AutoMailTemplateId",
                principalTable: "AutoMailTemplates",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mails_AutoMailTemplates_AutoMailTemplateId",
                table: "Mails");

            migrationBuilder.DropIndex(
                name: "IX_Mails_AutoMailTemplateId",
                table: "Mails");

            migrationBuilder.DropColumn(
                name: "AutoMailTemplateId",
                table: "Mails");
        }
    }
}
