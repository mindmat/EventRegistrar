using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReductionComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reductions");

            migrationBuilder.DropTable(
                name: "RegistrableCompositions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId1_ReductionActivatedIfCombinedWith = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrableId2_ReductionActivatedIfCombinedWith = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivatedByReduction = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OnlyForRole = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reductions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId1_ReductionActivatedIfCombinedWith",
                        column: x => x.RegistrableId1_ReductionActivatedIfCombinedWith,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId2_ReductionActivatedIfCombinedWith",
                        column: x => x.RegistrableId2_ReductionActivatedIfCombinedWith,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RegistrableCompositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId_Contains = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrableCompositions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrableCompositions_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrableCompositions_Registrables_RegistrableId_Contains",
                        column: x => x.RegistrableId_Contains,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId",
                table: "Reductions",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId1_ReductionActivatedIfCombinedWith",
                table: "Reductions",
                column: "RegistrableId1_ReductionActivatedIfCombinedWith");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId2_ReductionActivatedIfCombinedWith",
                table: "Reductions",
                column: "RegistrableId2_ReductionActivatedIfCombinedWith");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_Sequence",
                table: "Reductions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_RegistrableId",
                table: "RegistrableCompositions",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_RegistrableId_Contains",
                table: "RegistrableCompositions",
                column: "RegistrableId_Contains");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_Sequence",
                table: "RegistrableCompositions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
