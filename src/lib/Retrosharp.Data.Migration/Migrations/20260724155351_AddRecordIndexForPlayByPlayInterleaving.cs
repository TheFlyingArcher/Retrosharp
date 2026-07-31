using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordIndexForPlayByPlayInterleaving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordIndex",
                table: "GameSubstitution",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordIndex",
                table: "GameEvent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordIndex",
                table: "GameComment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordIndex",
                table: "GameAdjustment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordIndex",
                table: "GameSubstitution");

            migrationBuilder.DropColumn(
                name: "RecordIndex",
                table: "GameEvent");

            migrationBuilder.DropColumn(
                name: "RecordIndex",
                table: "GameComment");

            migrationBuilder.DropColumn(
                name: "RecordIndex",
                table: "GameAdjustment");
        }
    }
}
