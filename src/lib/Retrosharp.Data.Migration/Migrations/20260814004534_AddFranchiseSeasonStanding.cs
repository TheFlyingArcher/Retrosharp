using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchiseSeasonStanding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FranchiseSeasonStanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: false),
                    Wins = table.Column<short>(type: "smallint", nullable: false),
                    Losses = table.Column<short>(type: "smallint", nullable: false),
                    Ties = table.Column<short>(type: "smallint", nullable: false),
                    Rank = table.Column<byte>(type: "smallint", nullable: false),
                    GamesBehind = table.Column<decimal>(type: "numeric(5,1)", nullable: false),
                    DivisionChampion = table.Column<bool>(type: "boolean", nullable: false),
                    LeagueBestRecord = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseSeasonStanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseSeasonStanding_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseSeasonStanding_FranchiseId_SeasonYear",
                table: "FranchiseSeasonStanding",
                columns: new[] { "FranchiseId", "SeasonYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FranchiseSeasonStanding");
        }
    }
}
