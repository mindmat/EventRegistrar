using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class BulkMailTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mails_MailTemplates_MailTemplateId",
                table: "Mails");

            migrationBuilder.DropTable(
                name: "MailTemplates");

            migrationBuilder.RenameColumn(
                name: "MailTemplateId",
                table: "Mails",
                newName: "BulkMailTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Mails_MailTemplateId",
                table: "Mails",
                newName: "IX_Mails_BulkMailTemplateId");

            migrationBuilder.CreateTable(
                name: "BulkMailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BulkMailKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MailingAudience = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkMailTemplates", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BulkMailTemplates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BulkMailTemplates_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkMailTemplates_EventId",
                table: "BulkMailTemplates",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkMailTemplates_RegistrableId",
                table: "BulkMailTemplates",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkMailTemplates_Sequence",
                table: "BulkMailTemplates",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mails_BulkMailTemplates_BulkMailTemplateId",
                table: "Mails",
                column: "BulkMailTemplateId",
                principalTable: "BulkMailTemplates",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mails_BulkMailTemplates_BulkMailTemplateId",
                table: "Mails");

            migrationBuilder.DropTable(
                name: "BulkMailTemplates");

            migrationBuilder.RenameColumn(
                name: "BulkMailTemplateId",
                table: "Mails",
                newName: "MailTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Mails_BulkMailTemplateId",
                table: "Mails",
                newName: "IX_Mails_MailTemplateId");

            migrationBuilder.CreateTable(
                name: "MailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BulkMailKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MailingAudience = table.Column<int>(type: "int", nullable: true),
                    ReleaseImmediately = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SenderMail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailTemplates", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MailTemplates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailTemplates_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_EventId",
                table: "MailTemplates",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_RegistrableId",
                table: "MailTemplates",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_Sequence",
                table: "MailTemplates",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mails_MailTemplates_MailTemplateId",
                table: "Mails",
                column: "MailTemplateId",
                principalTable: "MailTemplates",
                principalColumn: "Id");
        }
    }
}
