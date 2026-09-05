using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkImport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: false),
                    SourceZipPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    WorkingDirectory = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BulkImportFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BulkImportId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    GamesInserted = table.Column<int>(type: "integer", nullable: true),
                    GamesSkipped = table.Column<int>(type: "integer", nullable: true),
                    StartedUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImportFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkImportFile_BulkImport_BulkImportId",
                        column: x => x.BulkImportId,
                        principalTable: "BulkImport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkImport_SeasonYear",
                table: "BulkImport",
                column: "SeasonYear");

            migrationBuilder.CreateIndex(
                name: "IX_BulkImport_TrackingId",
                table: "BulkImport",
                column: "TrackingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BulkImportFile_BulkImportId_FileName",
                table: "BulkImportFile",
                columns: new[] { "BulkImportId", "FileName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BulkImportFile_FileName",
                table: "BulkImportFile",
                column: "FileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkImportFile");

            migrationBuilder.DropTable(
                name: "BulkImport");
        }
    }
}
