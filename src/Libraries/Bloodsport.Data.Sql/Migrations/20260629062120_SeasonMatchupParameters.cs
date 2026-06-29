using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class SeasonMatchupParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonMatchupParameters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<long>(type: "bigint", nullable: false),
                    TeamSize = table.Column<int>(type: "int", nullable: false),
                    PickType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpectatorType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnoughPlayers = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonMatchupParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonMatchupParameters_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonMatchupParameters_SeasonId",
                table: "SeasonMatchupParameters",
                column: "SeasonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonMatchupParameters");
        }
    }
}
