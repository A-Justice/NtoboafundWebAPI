using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class changedisWinnertodateDeclared : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWinner",
                table: "LuckyMes");

            migrationBuilder.AddColumn<string>(
                name: "DateDeclared",
                table: "LuckyMes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDeclared",
                table: "LuckyMes");

            migrationBuilder.AddColumn<bool>(
                name: "IsWinner",
                table: "LuckyMes",
                nullable: false,
                defaultValue: false);
        }
    }
}
