using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class addedsomepropertiestodonations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "txRef",
                table: "Donations",
                newName: "TxRef");

            migrationBuilder.AlterColumn<string>(
                name: "TxRef",
                table: "Donations",
                nullable: true,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TxRef",
                table: "Donations",
                newName: "txRef");

            migrationBuilder.AlterColumn<decimal>(
                name: "txRef",
                table: "Donations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
