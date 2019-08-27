using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class databaseadjustments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "Scholarships",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "TxRef",
                table: "Scholarships",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "MobileMoneyDetails",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "LuckyMes",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "TxRef",
                table: "LuckyMes",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "Businesses",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "TxRef",
                table: "Businesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TxRef",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "MobileMoneyDetails");

            migrationBuilder.DropColumn(
                name: "TxRef",
                table: "LuckyMes");

            migrationBuilder.DropColumn(
                name: "TxRef",
                table: "Businesses");

            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "Scholarships",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "LuckyMes",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TransferId",
                table: "Businesses",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
