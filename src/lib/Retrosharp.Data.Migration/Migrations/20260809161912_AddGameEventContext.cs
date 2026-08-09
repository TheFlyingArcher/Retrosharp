using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddGameEventContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameEventContext",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    StartTimeLocal = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEventContext", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_GameEventContext_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameEventContext");
        }
    }
}
