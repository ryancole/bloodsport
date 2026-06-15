using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class PlayoffTournamentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RiotProviderId",
                table: "Playoffs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RiotTournamentId",
                table: "Playoffs",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiotProviderId",
                table: "Playoffs");

            migrationBuilder.DropColumn(
                name: "RiotTournamentId",
                table: "Playoffs");
        }
    }
}
