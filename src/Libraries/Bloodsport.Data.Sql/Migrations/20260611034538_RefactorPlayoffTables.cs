using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPlayoffTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_Seasons_SeasonId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_Teams_TeamOneId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_Teams_TeamTwoId",
                table: "PlayoffMatchups");

            migrationBuilder.DropTable(
                name: "PlayoffMatchupResults");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                table: "PlayoffMatchups",
                newName: "PlayoffId");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "PlayoffMatchups",
                newName: "MatchNumber");

            migrationBuilder.RenameIndex(
                name: "IX_PlayoffMatchups_SeasonId",
                table: "PlayoffMatchups",
                newName: "IX_PlayoffMatchups_PlayoffId");

            migrationBuilder.AddColumn<long>(
                name: "WinningTeamId",
                table: "PlayoffMatchups",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Playoffs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playoffs_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayoffTeams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    PlayoffId = table.Column<long>(type: "bigint", nullable: false),
                    Seed = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffTeams_Playoffs_PlayoffId",
                        column: x => x.PlayoffId,
                        principalTable: "Playoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayoffTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_WinningTeamId",
                table: "PlayoffMatchups",
                column: "WinningTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Playoffs_SeasonId",
                table: "Playoffs",
                column: "SeasonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffTeams_PlayoffId",
                table: "PlayoffTeams",
                column: "PlayoffId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffTeams_TeamId",
                table: "PlayoffTeams",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups",
                column: "NextMatchupId",
                principalTable: "PlayoffMatchups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_TeamOneId",
                table: "PlayoffMatchups",
                column: "TeamOneId",
                principalTable: "PlayoffTeams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_TeamTwoId",
                table: "PlayoffMatchups",
                column: "TeamTwoId",
                principalTable: "PlayoffTeams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_WinningTeamId",
                table: "PlayoffMatchups",
                column: "WinningTeamId",
                principalTable: "PlayoffTeams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_Playoffs_PlayoffId",
                table: "PlayoffMatchups",
                column: "PlayoffId",
                principalTable: "Playoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_TeamOneId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_TeamTwoId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffTeams_WinningTeamId",
                table: "PlayoffMatchups");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_Playoffs_PlayoffId",
                table: "PlayoffMatchups");

            migrationBuilder.DropTable(
                name: "PlayoffTeams");

            migrationBuilder.DropTable(
                name: "Playoffs");

            migrationBuilder.DropIndex(
                name: "IX_PlayoffMatchups_WinningTeamId",
                table: "PlayoffMatchups");

            migrationBuilder.DropColumn(
                name: "WinningTeamId",
                table: "PlayoffMatchups");

            migrationBuilder.RenameColumn(
                name: "PlayoffId",
                table: "PlayoffMatchups",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "MatchNumber",
                table: "PlayoffMatchups",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_PlayoffMatchups_PlayoffId",
                table: "PlayoffMatchups",
                newName: "IX_PlayoffMatchups_SeasonId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffMatchups_NextMatchupId",
                table: "PlayoffMatchups",
                column: "NextMatchupId",
                principalTable: "PlayoffMatchups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_Seasons_SeasonId",
                table: "PlayoffMatchups",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_Teams_TeamOneId",
                table: "PlayoffMatchups",
                column: "TeamOneId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_Teams_TeamTwoId",
                table: "PlayoffMatchups",
                column: "TeamTwoId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
