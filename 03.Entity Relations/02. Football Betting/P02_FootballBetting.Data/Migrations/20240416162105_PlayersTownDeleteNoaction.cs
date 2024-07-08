using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P02_FootballBetting.Data.Migrations
{
    public partial class PlayersTownDeleteNoaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Towns_TownId",
                table: "Players");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Towns_TownId",
                table: "Players",
                column: "TownId",
                principalTable: "Towns",
                principalColumn: "TownId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Towns_TownId",
                table: "Players");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Towns_TownId",
                table: "Players",
                column: "TownId",
                principalTable: "Towns",
                principalColumn: "TownId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
