using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class SeasonWeekMatchup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonWeekMatchups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonWeekId = table.Column<long>(type: "bigint", nullable: false),
                    TeamOneId = table.Column<long>(type: "bigint", nullable: false),
                    TeamTwoId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonWeekMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonWeekMatchups_SeasonWeeks_SeasonWeekId",
                        column: x => x.SeasonWeekId,
                        principalTable: "SeasonWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonWeekMatchups_Teams_TeamOneId",
                        column: x => x.TeamOneId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeasonWeekMatchups_Teams_TeamTwoId",
                        column: x => x.TeamTwoId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeekMatchups_SeasonWeekId",
                table: "SeasonWeekMatchups",
                column: "SeasonWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeekMatchups_TeamOneId",
                table: "SeasonWeekMatchups",
                column: "TeamOneId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonWeekMatchups_TeamTwoId",
                table: "SeasonWeekMatchups",
                column: "TeamTwoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonWeekMatchups");
        }
    }
}
