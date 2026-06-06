using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class WeekyMatchupResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonWeekMatchupResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WinnerTeamId = table.Column<long>(type: "bigint", nullable: false),
                    SeasonWeekMatchupId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonWeekMatchupResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonWeekMatchupResults_SeasonWeekMatchups_SeasonWeekMatchupId",
                        column: x => x.SeasonWeekMatchupId,
                        principalTable: "SeasonWeekMatchups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonWeekMatchupResults_Teams_WinnerTeamId",
                        column: x => x.WinnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeekMatchupResults_SeasonWeekMatchupId",
                table: "SeasonWeekMatchupResults",
                column: "SeasonWeekMatchupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeekMatchupResults_WinnerTeamId",
                table: "SeasonWeekMatchupResults",
                column: "WinnerTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonWeekMatchupResults");
        }
    }
}
