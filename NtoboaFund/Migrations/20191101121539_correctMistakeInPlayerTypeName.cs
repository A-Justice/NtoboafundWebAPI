using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class correctMistakeInPlayerTypeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayerType",
                table: "Scholarships",
                newName: "PlayerType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlayerType",
                table: "Scholarships",
                newName: "PayerType");
        }
    }
}
