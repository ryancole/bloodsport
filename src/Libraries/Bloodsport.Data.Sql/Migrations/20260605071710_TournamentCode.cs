using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class TournamentCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TournamentCode",
                table: "SeasonWeekMatchups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RiotProviderId",
                table: "Seasons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RiotTournamentId",
                table: "Seasons",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TournamentCode",
                table: "SeasonWeekMatchups");

            migrationBuilder.DropColumn(
                name: "RiotProviderId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "RiotTournamentId",
                table: "Seasons");
        }
    }
}
