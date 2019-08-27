using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class addedothersetupTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MomoNumber",
                table: "MobileMoneyDetails",
                newName: "Number");

            migrationBuilder.AddColumn<string>(
                name: "PreferedMoneyReceptionMethod",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Transfer",
                columns: table => new
                {
                    TransferId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    status = table.Column<string>(nullable: true),
                    message = table.Column<string>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    account_number = table.Column<string>(nullable: true),
                    bank_code = table.Column<string>(nullable: true),
                    fullname = table.Column<string>(nullable: true),
                    date_created = table.Column<DateTime>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    amount = table.Column<int>(nullable: false),
                    fee = table.Column<int>(nullable: false),
                    reference = table.Column<string>(nullable: true),
                    narration = table.Column<string>(nullable: true),
                    complete_message = table.Column<string>(nullable: true),
                    requires_approval = table.Column<int>(nullable: false),
                    is_approved = table.Column<int>(nullable: false),
                    bank_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer", x => x.TransferId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "Transfer");

            migrationBuilder.DropColumn(
                name: "PreferedMoneyReceptionMethod",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "MobileMoneyDetails",
                newName: "MomoNumber");
        }
    }
}
