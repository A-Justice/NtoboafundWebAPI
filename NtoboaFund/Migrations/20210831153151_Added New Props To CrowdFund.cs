using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class AddedNewPropsToCrowdFund : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DateCreated",
                table: "CrowdFunds",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndDate",
                table: "CrowdFunds",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CrowdFunds",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "CrowdFunds");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CrowdFunds");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CrowdFunds");
        }
    }
}
