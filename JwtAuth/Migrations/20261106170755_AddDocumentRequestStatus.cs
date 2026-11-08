using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentRequestStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DocumentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
        UPDATE DocumentRequests
        SET Status =
            CASE
                WHEN IsCompleted = 1 THEN 2
                ELSE 0
            END
        """);

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "DocumentRequests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "DocumentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
        UPDATE DocumentRequests
        SET IsCompleted =
            CASE
                WHEN Status = 2 THEN 1
                ELSE 0
            END
        """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocumentRequests");
        }
    }
}
