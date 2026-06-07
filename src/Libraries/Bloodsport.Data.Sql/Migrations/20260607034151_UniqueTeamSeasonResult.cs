using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTeamSeasonResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamSeasonResults_TeamId",
                table: "TeamSeasonResults");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonResults_TeamId_SeasonId",
                table: "TeamSeasonResults",
                columns: new[] { "TeamId", "SeasonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamSeasonResults_TeamId_SeasonId",
                table: "TeamSeasonResults");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonResults_TeamId",
                table: "TeamSeasonResults",
                column: "TeamId");
        }
    }
}
