using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ProcessMailEventFromOtherMailSender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendGridMessageId",
                table: "Mails",
                newName: "MailSenderMessageId");

            migrationBuilder.AddColumn<int>(
                name: "MailSender",
                table: "RawMailEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "RawMailEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SentBy",
                table: "Mails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MailSender",
                table: "MailEvents",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailSender",
                table: "RawMailEvents");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "RawMailEvents");

            migrationBuilder.DropColumn(
                name: "SentBy",
                table: "Mails");

            migrationBuilder.DropColumn(
                name: "MailSender",
                table: "MailEvents");

            migrationBuilder.RenameColumn(
                name: "MailSenderMessageId",
                table: "Mails",
                newName: "SendGridMessageId");
        }
    }
}
