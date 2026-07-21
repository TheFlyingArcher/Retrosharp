using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UniqueStatisticsNaturalKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "Pitching");

            migrationBuilder.DropIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding");

            migrationBuilder.DropIndex(
                name: "IX_Batting_PersonId_FranchiseId_SeasonYear",
                table: "Batting");

            migrationBuilder.CreateIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "Pitching",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear", "Position" },
                unique: true,
                filter: "[SeasonYear] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Batting_PersonId_FranchiseId_SeasonYear",
                table: "Batting",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" },
                unique: true,
                filter: "[SeasonYear] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "Pitching");

            migrationBuilder.DropIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding");

            migrationBuilder.DropIndex(
                name: "IX_Batting_PersonId_FranchiseId_SeasonYear",
                table: "Batting");

            migrationBuilder.CreateIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "Pitching",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Batting_PersonId_FranchiseId_SeasonYear",
                table: "Batting",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });
        }
    }
}
