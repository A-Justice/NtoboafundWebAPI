using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class addedPlayerTypeToScholarship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayerType",
                table: "Scholarships",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayerType",
                table: "Scholarships");
        }
    }
}
