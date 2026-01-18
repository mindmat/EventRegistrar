using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStatSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatisticSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticSnapshots", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_StatisticSnapshots_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticSnapshots_EventId",
                table: "StatisticSnapshots",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticSnapshots_Key_EventId_CreatedAt",
                table: "StatisticSnapshots",
                columns: new[] { "Key", "EventId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticSnapshots_Sequence",
                table: "StatisticSnapshots",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatisticSnapshots");
        }
    }
}
