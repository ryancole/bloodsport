using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayoffRoundEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PlayoffRoundId",
                table: "PlayoffMatchups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "PlayoffRound",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayoffId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayoffRound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayoffRound_Playoffs_PlayoffId",
                        column: x => x.PlayoffId,
                        principalTable: "Playoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffMatchups_PlayoffRoundId",
                table: "PlayoffMatchups",
                column: "PlayoffRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayoffRound_PlayoffId",
                table: "PlayoffRound",
                column: "PlayoffId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayoffMatchups_PlayoffRound_PlayoffRoundId",
                table: "PlayoffMatchups",
                column: "PlayoffRoundId",
                principalTable: "PlayoffRound",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayoffMatchups_PlayoffRound_PlayoffRoundId",
                table: "PlayoffMatchups");

            migrationBuilder.DropTable(
                name: "PlayoffRound");

            migrationBuilder.DropIndex(
                name: "IX_PlayoffMatchups_PlayoffRoundId",
                table: "PlayoffMatchups");

            migrationBuilder.DropColumn(
                name: "PlayoffRoundId",
                table: "PlayoffMatchups");
        }
    }
}
