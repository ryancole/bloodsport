using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class TeamPlayoffRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamPlayoffRosters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    PlayoffId = table.Column<long>(type: "bigint", nullable: false),
                    RosterJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPlayoffRosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamPlayoffRosters_Playoffs_PlayoffId",
                        column: x => x.PlayoffId,
                        principalTable: "Playoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamPlayoffRosters_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayoffRosters_PlayoffId",
                table: "TeamPlayoffRosters",
                column: "PlayoffId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayoffRosters_TeamId_PlayoffId",
                table: "TeamPlayoffRosters",
                columns: new[] { "TeamId", "PlayoffId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamPlayoffRosters");
        }
    }
}
