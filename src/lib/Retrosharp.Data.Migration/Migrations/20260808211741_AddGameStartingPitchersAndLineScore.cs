using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddGameStartingPitchersAndLineScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomeStartingPitcherId",
                table: "Game",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitorStartingPitcherId",
                table: "Game",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_HomeStartingPitcherId",
                table: "Game",
                column: "HomeStartingPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_VisitorStartingPitcherId",
                table: "Game",
                column: "VisitorStartingPitcherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Person_HomeStartingPitcherId",
                table: "Game",
                column: "HomeStartingPitcherId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Person_VisitorStartingPitcherId",
                table: "Game",
                column: "VisitorStartingPitcherId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Person_HomeStartingPitcherId",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_Person_VisitorStartingPitcherId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_HomeStartingPitcherId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_VisitorStartingPitcherId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "HomeStartingPitcherId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "VisitorStartingPitcherId",
                table: "Game");
        }
    }
}
