using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class EditedPaymentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayerId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserPayedId",
                table: "Payments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payments",
                newName: "Reference");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Payments",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Payments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Payments",
                newName: "UserPayedId");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "Payments",
                newName: "TransactionId");

            migrationBuilder.AddColumn<string>(
                name: "PayerId",
                table: "Payments",
                nullable: true);
        }
    }
}
