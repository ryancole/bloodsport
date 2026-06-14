using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class DropPlayoffMatchupRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_Playoffs_PlayoffId",
                table: "PlayoffMatchups");

            migrationBuilder.DropIndex(
                name: "IX_PlayoffMatchups_PlayoffId",
                table: "PlayoffMatchups");

            migrationBuilder.DropColumn(
                name: "PlayoffId",
                table: "PlayoffMatchups");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "PlayoffMatchups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PlayoffId",
                table: "PlayoffMatchups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "PlayoffMatchups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_PlayoffId",
                table: "PlayoffMatchups",
                column: "PlayoffId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_Playoffs_PlayoffId",
                table: "PlayoffMatchups",
                column: "PlayoffId",
                principalTable: "Playoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
