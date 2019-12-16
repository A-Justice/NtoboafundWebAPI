using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class editedPaymentModelAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Payments",
                newName: "TelcoTransactionId");

            migrationBuilder.AlterColumn<long>(
                name: "Reference",
                table: "Payments",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelcoTransactionId",
                table: "Payments",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                table: "Payments",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Payments",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
