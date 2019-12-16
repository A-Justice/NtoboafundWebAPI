using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class madeAnotherChangeToPaymentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelcoTransactionId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "Payments",
                newName: "TransactionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePayed",
                table: "Payments",
                nullable: true,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payments",
                newName: "Reference");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePayed",
                table: "Payments",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelcoTransactionId",
                table: "Payments",
                nullable: true);
        }
    }
}
