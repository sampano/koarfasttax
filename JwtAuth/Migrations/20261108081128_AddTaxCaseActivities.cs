using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxCaseActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxCaseActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCaseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCaseActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxCaseActivities_TaxCases_TaxCaseId",
                        column: x => x.TaxCaseId,
                        principalTable: "TaxCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxCaseActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCaseActivities_TaxCaseId",
                table: "TaxCaseActivities",
                column: "TaxCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCaseActivities_UserId",
                table: "TaxCaseActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxCaseActivities");
        }
    }
}
