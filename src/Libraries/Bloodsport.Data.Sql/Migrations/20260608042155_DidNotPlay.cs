using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class DidNotPlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "WinnerTeamId",
                table: "SeasonWeekMatchupResults",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "DidNotPlay",
                table: "SeasonWeekMatchupResults",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DidNotPlay",
                table: "SeasonWeekMatchupResults");

            migrationBuilder.AlterColumn<long>(
                name: "WinnerTeamId",
                table: "SeasonWeekMatchupResults",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
