using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class RiotAccountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameName",
                table: "RiotAccounts",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Puuid",
                table: "RiotAccounts",
                type: "nvarchar(78)",
                maxLength: 78,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagLine",
                table: "RiotAccounts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RiotAccounts_Puuid",
                table: "RiotAccounts",
                column: "Puuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiotAccounts_Puuid",
                table: "RiotAccounts");

            migrationBuilder.DropColumn(
                name: "GameName",
                table: "RiotAccounts");

            migrationBuilder.DropColumn(
                name: "Puuid",
                table: "RiotAccounts");

            migrationBuilder.DropColumn(
                name: "TagLine",
                table: "RiotAccounts");
        }
    }
}
