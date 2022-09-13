using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class AddedUsertoCrowdFund : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CrowdFunds",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrowdFunds_UserId",
                table: "CrowdFunds",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdFunds_AspNetUsers_UserId",
                table: "CrowdFunds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrowdFunds_AspNetUsers_UserId",
                table: "CrowdFunds");

            migrationBuilder.DropIndex(
                name: "IX_CrowdFunds_UserId",
                table: "CrowdFunds");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CrowdFunds");
        }
    }
}
