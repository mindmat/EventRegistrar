using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class AddPricePackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricePackages", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PricePackages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricePackagePart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricePackagePart", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PricePackagePart_PricePackages_PricePackageId",
                        column: x => x.PricePackageId,
                        principalTable: "PricePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrableInPricePackageParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricePackagePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrableInPricePackageParts", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrableInPricePackageParts_PricePackagePart_PricePackagePartId",
                        column: x => x.PricePackagePartId,
                        principalTable: "PricePackagePart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrableInPricePackageParts_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricePackagePart_PricePackageId",
                table: "PricePackagePart",
                column: "PricePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PricePackagePart_Sequence",
                table: "PricePackagePart",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PricePackages_EventId",
                table: "PricePackages",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PricePackages_Sequence",
                table: "PricePackages",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableInPricePackageParts_PricePackagePartId",
                table: "RegistrableInPricePackageParts",
                column: "PricePackagePartId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableInPricePackageParts_RegistrableId",
                table: "RegistrableInPricePackageParts",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableInPricePackageParts_Sequence",
                table: "RegistrableInPricePackageParts",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrableInPricePackageParts");

            migrationBuilder.DropTable(
                name: "PricePackagePart");

            migrationBuilder.DropTable(
                name: "PricePackages");
        }
    }
}
