using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTeamMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamMemberships_TeamId",
                table: "TeamMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_TeamId_RiotAccountId",
                table: "TeamMemberships",
                columns: new[] { "TeamId", "RiotAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamMemberships_TeamId_RiotAccountId",
                table: "TeamMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_TeamId",
                table: "TeamMemberships",
                column: "TeamId");
        }
    }
}
