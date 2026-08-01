using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class GameNaturalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Game_GameDate_HomeFranchiseId_VisitorFranchiseId",
                table: "Game");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameDate_GameNumber_HomeFranchiseId_VisitorFranchiseId",
                table: "Game",
                columns: new[] { "GameDate", "GameNumber", "HomeFranchiseId", "VisitorFranchiseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Game_GameDate_GameNumber_HomeFranchiseId_VisitorFranchiseId",
                table: "Game");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameDate_HomeFranchiseId_VisitorFranchiseId",
                table: "Game",
                columns: new[] { "GameDate", "HomeFranchiseId", "VisitorFranchiseId" });
        }
    }
}
