using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class PlayoffBracket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayoffMatchups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<long>(type: "bigint", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    TeamOneId = table.Column<long>(type: "bigint", nullable: true),
                    TeamTwoId = table.Column<long>(type: "bigint", nullable: true),
                    NextMatchupId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                        column: x => x.NextMatchupId,
                        principalTable: "PlayoffMatchups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_Teams_TeamOneId",
                        column: x => x.TeamOneId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_Teams_TeamTwoId",
                        column: x => x.TeamTwoId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayoffMatchupResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayoffMatchupId = table.Column<long>(type: "bigint", nullable: false),
                    WinnerTeamId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffMatchupResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchupResults_PlayoffMatchups_PlayoffMatchupId",
                        column: x => x.PlayoffMatchupId,
                        principalTable: "PlayoffMatchups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchupResults_Teams_WinnerTeamId",
                        column: x => x.WinnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchupResults_PlayoffMatchupId",
                table: "PlayoffMatchupResults",
                column: "PlayoffMatchupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchupResults_WinnerTeamId",
                table: "PlayoffMatchupResults",
                column: "WinnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups",
                column: "NextMatchupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_SeasonId",
                table: "PlayoffMatchups",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_TeamOneId",
                table: "PlayoffMatchups",
                column: "TeamOneId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_TeamTwoId",
                table: "PlayoffMatchups",
                column: "TeamTwoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayoffMatchupResults");

            migrationBuilder.DropTable(
                name: "PlayoffMatchups");
        }
    }
}
