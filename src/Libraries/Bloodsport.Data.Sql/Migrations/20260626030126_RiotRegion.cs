using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class RiotRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RiotRegion",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiotRegion",
                table: "Playoffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiotRegion",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "RiotRegion",
                table: "Playoffs");
        }
    }
}
