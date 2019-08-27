using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class Addedmomoandbankdetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransferId",
                table: "Scholarships",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferId",
                table: "LuckyMes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferId",
                table: "Businesses",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BankDetails",
                columns: table => new
                {
                    BankDetailsId = table.Column<string>(nullable: false),
                    BankName = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    SwiftCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDetails", x => x.BankDetailsId);
                    table.ForeignKey(
                        name: "FK_BankDetails_AspNetUsers_BankDetailsId",
                        column: x => x.BankDetailsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MobileMoneyDetails",
                columns: table => new
                {
                    MobileMoneyDetailsId = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    Network = table.Column<string>(nullable: true),
                    Voucher = table.Column<string>(nullable: true),
                    MomoNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileMoneyDetails", x => x.MobileMoneyDetailsId);
                    table.ForeignKey(
                        name: "FK_MobileMoneyDetails_AspNetUsers_MobileMoneyDetailsId",
                        column: x => x.MobileMoneyDetailsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankDetails");

            migrationBuilder.DropTable(
                name: "MobileMoneyDetails");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "LuckyMes");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "Businesses");
        }
    }
}
