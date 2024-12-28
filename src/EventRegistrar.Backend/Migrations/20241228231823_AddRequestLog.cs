using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    RequestType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    When = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserDisplayText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionTimeInMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLog", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLog_Sequence",
                table: "RequestLog",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLog");
        }
    }
}
