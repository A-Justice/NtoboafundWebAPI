using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NtoboaFund.Migrations
{
    public partial class AddedCrowdFunding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrowdFundTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrowdFundTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrowdFunds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    MainImageUrl = table.Column<string>(nullable: true),
                    SecondImageUrl = table.Column<string>(nullable: true),
                    ThirdImageUrl = table.Column<string>(nullable: true),
                    videoUrl = table.Column<string>(nullable: true),
                    TotalAmount = table.Column<decimal>(nullable: false),
                    TotalAmountRecieved = table.Column<decimal>(nullable: false),
                    TypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrowdFunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrowdFunds_CrowdFundTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "CrowdFundTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    AmountDonated = table.Column<decimal>(nullable: false),
                    CrowdFundId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donors_CrowdFunds_CrowdFundId",
                        column: x => x.CrowdFundId,
                        principalTable: "CrowdFunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdFunds_TypeId",
                table: "CrowdFunds",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_CrowdFundId",
                table: "Donors",
                column: "CrowdFundId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "CrowdFunds");

            migrationBuilder.DropTable(
                name: "CrowdFundTypes");
        }
    }
}
