using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class PlayoffRoundMatchupResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffRoundMatchups_PlayoffTeams_WinningTeamId",
                table: "PlayoffRoundMatchups");

            migrationBuilder.DropIndex(
                name: "IX_PlayoffRoundMatchups_WinningTeamId",
                table: "PlayoffRoundMatchups");

            migrationBuilder.DropColumn(
                name: "WinningTeamId",
                table: "PlayoffRoundMatchups");

            migrationBuilder.CreateTable(
                name: "PlayoffRoundMatchupResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayoffRoundMatchupId = table.Column<long>(type: "bigint", nullable: false),
                    WinningTeamId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffRoundMatchupResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchupResults_PlayoffRoundMatchups_PlayoffRoundMatchupId",
                        column: x => x.PlayoffRoundMatchupId,
                        principalTable: "PlayoffRoundMatchups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchupResults_PlayoffTeams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchupResults_PlayoffRoundMatchupId",
                table: "PlayoffRoundMatchupResults",
                column: "PlayoffRoundMatchupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchupResults_WinningTeamId",
                table: "PlayoffRoundMatchupResults",
                column: "WinningTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayoffRoundMatchupResults");

            migrationBuilder.AddColumn<long>(
                name: "WinningTeamId",
                table: "PlayoffRoundMatchups",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_WinningTeamId",
                table: "PlayoffRoundMatchups",
                column: "WinningTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffRoundMatchups_PlayoffTeams_WinningTeamId",
                table: "PlayoffRoundMatchups",
                column: "WinningTeamId",
                principalTable: "PlayoffTeams",
                principalColumn: "Id");
        }
    }
}
