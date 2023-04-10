using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MultiQuestionOptionMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionOptionIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrableCombinedIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortKey = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiQuestionOptionMappings", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MultiQuestionOptionMappings_RegistrationForms_RegistrationFormId",
                        column: x => x.RegistrationFormId,
                        principalTable: "RegistrationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiQuestionOptionMappings_RegistrationFormId",
                table: "MultiQuestionOptionMappings",
                column: "RegistrationFormId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiQuestionOptionMappings_Sequence",
                table: "MultiQuestionOptionMappings",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiQuestionOptionMappings");
        }
    }
}
