using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodsport.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class Recruitment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiotAccountRecruitments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiotAccountId = table.Column<long>(type: "bigint", nullable: false),
                    IsLookingForTeam = table.Column<bool>(type: "bit", nullable: false),
                    Lanes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiotAccountRecruitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiotAccountRecruitments_RiotAccounts_RiotAccountId",
                        column: x => x.RiotAccountId,
                        principalTable: "RiotAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamRecruitments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    IsLookingForUser = table.Column<bool>(type: "bit", nullable: false),
                    Lanes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRecruitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamRecruitments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiotAccountRecruitments_RiotAccountId",
                table: "RiotAccountRecruitments",
                column: "RiotAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamRecruitments_TeamId",
                table: "TeamRecruitments",
                column: "TeamId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiotAccountRecruitments");

            migrationBuilder.DropTable(
                name: "TeamRecruitments");
        }
    }
}
