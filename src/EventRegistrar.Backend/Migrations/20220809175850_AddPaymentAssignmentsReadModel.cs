using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class AddPaymentAssignmentsReadModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentAssignmentsReadModel",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAssignmentsReadModel", x => new { x.EventId, x.PaymentId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignmentsReadModel_Sequence",
                table: "PaymentAssignmentsReadModel",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentAssignmentsReadModel");
        }
    }
}
