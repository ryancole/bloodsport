using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class AverageGPM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WinnerTeamAverageGPM",
                table: "TeamSeasonResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "WinnerTeamAverageGPM",
                table: "SeasonWeekMatchupResults",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinnerTeamAverageGPM",
                table: "TeamSeasonResults");

            migrationBuilder.DropColumn(
                name: "WinnerTeamAverageGPM",
                table: "SeasonWeekMatchupResults");
        }
    }
}
