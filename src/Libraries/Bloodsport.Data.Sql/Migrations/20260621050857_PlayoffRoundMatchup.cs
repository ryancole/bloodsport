using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class PlayoffRoundMatchup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayoffMatchups");

            migrationBuilder.CreateTable(
                name: "PlayoffRoundMatchups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayoffRoundId = table.Column<long>(type: "bigint", nullable: false),
                    MatchNumber = table.Column<int>(type: "int", nullable: false),
                    TeamOneId = table.Column<long>(type: "bigint", nullable: true),
                    TeamTwoId = table.Column<long>(type: "bigint", nullable: true),
                    WinningTeamId = table.Column<long>(type: "bigint", nullable: true),
                    NextMatchupId = table.Column<long>(type: "bigint", nullable: true),
                    TournamentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffRoundMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchups_PlayoffRoundMatchups_NextMatchupId",
                        column: x => x.NextMatchupId,
                        principalTable: "PlayoffRoundMatchups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchups_PlayoffRounds_PlayoffRoundId",
                        column: x => x.PlayoffRoundId,
                        principalTable: "PlayoffRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchups_PlayoffTeams_TeamOneId",
                        column: x => x.TeamOneId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchups_PlayoffTeams_TeamTwoId",
                        column: x => x.TeamTwoId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffRoundMatchups_PlayoffTeams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_NextMatchupId",
                table: "PlayoffRoundMatchups",
                column: "NextMatchupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_PlayoffRoundId",
                table: "PlayoffRoundMatchups",
                column: "PlayoffRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_TeamOneId",
                table: "PlayoffRoundMatchups",
                column: "TeamOneId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_TeamTwoId",
                table: "PlayoffRoundMatchups",
                column: "TeamTwoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRoundMatchups_WinningTeamId",
                table: "PlayoffRoundMatchups",
                column: "WinningTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayoffRoundMatchups");

            migrationBuilder.CreateTable(
                name: "PlayoffMatchups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NextMatchupId = table.Column<long>(type: "bigint", nullable: true),
                    PlayoffRoundId = table.Column<long>(type: "bigint", nullable: false),
                    TeamOneId = table.Column<long>(type: "bigint", nullable: true),
                    TeamTwoId = table.Column<long>(type: "bigint", nullable: true),
                    WinningTeamId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchNumber = table.Column<int>(type: "int", nullable: false),
                    TournamentCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                        column: x => x.NextMatchupId,
                        principalTable: "PlayoffMatchups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffRounds_PlayoffRoundId",
                        column: x => x.PlayoffRoundId,
                        principalTable: "PlayoffRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffTeams_TeamOneId",
                        column: x => x.TeamOneId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffTeams_TeamTwoId",
                        column: x => x.TeamTwoId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayoffMatchups_PlayoffTeams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalTable: "PlayoffTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups",
                column: "NextMatchupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_PlayoffRoundId",
                table: "PlayoffMatchups",
                column: "PlayoffRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_TeamOneId",
                table: "PlayoffMatchups",
                column: "TeamOneId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_TeamTwoId",
                table: "PlayoffMatchups",
                column: "TeamTwoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_WinningTeamId",
                table: "PlayoffMatchups",
                column: "WinningTeamId");
        }
    }
}
