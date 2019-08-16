using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class addedluckymeattributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOrNumber",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountToWin",
                table: "LuckyMes",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsWinner",
                table: "LuckyMes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "LuckyMes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LuckyMes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountToWin",
                table: "LuckyMes");

            migrationBuilder.DropColumn(
                name: "IsWinner",
                table: "LuckyMes");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "LuckyMes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LuckyMes");

            migrationBuilder.AddColumn<string>(
                name: "EmailOrNumber",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
