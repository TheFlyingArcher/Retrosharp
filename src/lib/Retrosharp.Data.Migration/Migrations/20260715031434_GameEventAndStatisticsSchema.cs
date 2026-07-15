using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class GameEventAndStatisticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear",
                table: "Fielding");

            migrationBuilder.AddColumn<byte>(
                name: "Position",
                table: "Fielding",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "GameAdjustment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameAdjustment_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameAdjustment_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameBattingStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    HomeVisitor = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    PlateAppearances = table.Column<short>(type: "smallint", nullable: false),
                    AtBats = table.Column<short>(type: "smallint", nullable: false),
                    Hit = table.Column<short>(type: "smallint", nullable: false),
                    Doubles = table.Column<short>(type: "smallint", nullable: false),
                    Triples = table.Column<short>(type: "smallint", nullable: false),
                    Homeruns = table.Column<short>(type: "smallint", nullable: false),
                    RunsBattedIn = table.Column<short>(type: "smallint", nullable: false),
                    BaseOnBalls = table.Column<short>(type: "smallint", nullable: false),
                    Strikeouts = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeFlies = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeBunts = table.Column<short>(type: "smallint", nullable: false),
                    IntentionalBb = table.Column<short>(type: "smallint", nullable: false),
                    HitByPitches = table.Column<short>(type: "smallint", nullable: false),
                    StolenBases = table.Column<short>(type: "smallint", nullable: false),
                    TimesCaughtStealing = table.Column<short>(type: "smallint", nullable: false),
                    Runs = table.Column<short>(type: "smallint", nullable: false),
                    GroundedIntoDoublePlay = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameBattingStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameBattingStatistics_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameBattingStatistics_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameComment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameComment_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Inning = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamAtBat = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    BatterId = table.Column<int>(type: "int", nullable: false),
                    PitcherId = table.Column<int>(type: "int", nullable: false),
                    Balls = table.Column<byte>(type: "tinyint", nullable: false),
                    Strikes = table.Column<byte>(type: "tinyint", nullable: false),
                    FoulBallsWithTwoStrikes = table.Column<byte>(type: "tinyint", nullable: false),
                    PitchSequence = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RawEventText = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    BattedBallType = table.Column<int>(type: "int", nullable: true),
                    IsSacHit = table.Column<bool>(type: "bit", nullable: false),
                    IsSacFly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEvent_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameEvent_Person_BatterId",
                        column: x => x.BatterId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEvent_Person_PitcherId",
                        column: x => x.PitcherId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameEventGameStatus",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEventGameStatus", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_GameEventGameStatus_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameFieldingStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    HomeVisitor = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Putouts = table.Column<short>(type: "smallint", nullable: false),
                    Assists = table.Column<short>(type: "smallint", nullable: false),
                    Errors = table.Column<short>(type: "smallint", nullable: false),
                    PassedBalls = table.Column<byte>(type: "tinyint", nullable: false),
                    DoublePlays = table.Column<byte>(type: "tinyint", nullable: false),
                    TriplePlays = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameFieldingStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameFieldingStatistics_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameFieldingStatistics_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePitchingStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    HomeVisitor = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    PitchersUsed = table.Column<byte>(type: "tinyint", nullable: false),
                    IndividualEarnedRuns = table.Column<short>(type: "smallint", nullable: false),
                    TeamEarnedRuns = table.Column<short>(type: "smallint", nullable: false),
                    WildPitches = table.Column<byte>(type: "tinyint", nullable: false),
                    Balks = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePitchingStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePitchingStatistics_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GamePitchingStatistics_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSubstitution",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    TeamAtBat = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    BattingOrderPosition = table.Column<byte>(type: "tinyint", nullable: false),
                    FieldingPosition = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSubstitution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSubstitution_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSubstitution_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameEventRunner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameEventId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    StartBase = table.Column<int>(type: "int", nullable: false),
                    EndBase = table.Column<int>(type: "int", nullable: false),
                    IsOut = table.Column<bool>(type: "bit", nullable: false),
                    IsRBI = table.Column<bool>(type: "bit", nullable: false),
                    IsEarnedRun = table.Column<bool>(type: "bit", nullable: false),
                    ResponsiblePitcherId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEventRunner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEventRunner_GameEvent_GameEventId",
                        column: x => x.GameEventId,
                        principalTable: "GameEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEventRunner_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEventRunner_Person_ResponsiblePitcherId",
                        column: x => x.ResponsiblePitcherId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameEventFieldingCredit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameEventId = table.Column<int>(type: "int", nullable: false),
                    GameEventRunnerId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    CreditType = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEventFieldingCredit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEventFieldingCredit_GameEventRunner_GameEventRunnerId",
                        column: x => x.GameEventRunnerId,
                        principalTable: "GameEventRunner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEventFieldingCredit_GameEvent_GameEventId",
                        column: x => x.GameEventId,
                        principalTable: "GameEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEventFieldingCredit_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_GameAdjustment_GameId_Sequence",
                table: "GameAdjustment",
                columns: new[] { "GameId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_GameAdjustment_PersonId",
                table: "GameAdjustment",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GameBattingStatistics_FranchiseId",
                table: "GameBattingStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameBattingStatistics_GameId_FranchiseId_HomeVisitor",
                table: "GameBattingStatistics",
                columns: new[] { "GameId", "FranchiseId", "HomeVisitor" });

            migrationBuilder.CreateIndex(
                name: "IX_GameComment_GameId_Sequence",
                table: "GameComment",
                columns: new[] { "GameId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvent_BatterId",
                table: "GameEvent",
                column: "BatterId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvent_GameId_Sequence",
                table: "GameEvent",
                columns: new[] { "GameId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvent_PitcherId",
                table: "GameEvent",
                column: "PitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventFieldingCredit_GameEventId",
                table: "GameEventFieldingCredit",
                column: "GameEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventFieldingCredit_GameEventRunnerId",
                table: "GameEventFieldingCredit",
                column: "GameEventRunnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventFieldingCredit_PersonId",
                table: "GameEventFieldingCredit",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventRunner_GameEventId",
                table: "GameEventRunner",
                column: "GameEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventRunner_PersonId",
                table: "GameEventRunner",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEventRunner_ResponsiblePitcherId",
                table: "GameEventRunner",
                column: "ResponsiblePitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_GameFieldingStatistics_FranchiseId",
                table: "GameFieldingStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameFieldingStatistics_GameId_FranchiseId_HomeVisitor",
                table: "GameFieldingStatistics",
                columns: new[] { "GameId", "FranchiseId", "HomeVisitor" });

            migrationBuilder.CreateIndex(
                name: "IX_GamePitchingStatistics_FranchiseId",
                table: "GamePitchingStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePitchingStatistics_GameId_FranchiseId_HomeVisitor",
                table: "GamePitchingStatistics",
                columns: new[] { "GameId", "FranchiseId", "HomeVisitor" });

            migrationBuilder.CreateIndex(
                name: "IX_GameSubstitution_GameId_Sequence",
                table: "GameSubstitution",
                columns: new[] { "GameId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_GameSubstitution_PersonId",
                table: "GameSubstitution",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameAdjustment");

            migrationBuilder.DropTable(
                name: "GameBattingStatistics");

            migrationBuilder.DropTable(
                name: "GameComment");

            migrationBuilder.DropTable(
                name: "GameEventFieldingCredit");

            migrationBuilder.DropTable(
                name: "GameEventGameStatus");

            migrationBuilder.DropTable(
                name: "GameFieldingStatistics");

            migrationBuilder.DropTable(
                name: "GamePitchingStatistics");

            migrationBuilder.DropTable(
                name: "GameSubstitution");

            migrationBuilder.DropTable(
                name: "GameEventRunner");

            migrationBuilder.DropTable(
                name: "GameEvent");

            migrationBuilder.DropIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Fielding");

            migrationBuilder.CreateTable(
                name: "GameStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    AtBats = table.Column<short>(type: "smallint", nullable: false),
                    BaseOnBalls = table.Column<short>(type: "smallint", nullable: false),
                    Doubles = table.Column<short>(type: "smallint", nullable: false),
                    GroundedIntoDoublePlay = table.Column<short>(type: "smallint", nullable: false),
                    Hit = table.Column<short>(type: "smallint", nullable: false),
                    HitByPitches = table.Column<short>(type: "smallint", nullable: false),
                    HomeVisitor = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Homeruns = table.Column<short>(type: "smallint", nullable: false),
                    IntentionalBb = table.Column<short>(type: "smallint", nullable: false),
                    PlateAppearances = table.Column<short>(type: "smallint", nullable: false),
                    Runs = table.Column<short>(type: "smallint", nullable: false),
                    RunsBattedIn = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeBunts = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeFlies = table.Column<short>(type: "smallint", nullable: false),
                    StolenBases = table.Column<short>(type: "smallint", nullable: false),
                    Strikeouts = table.Column<short>(type: "smallint", nullable: false),
                    TimesCaughtStealing = table.Column<short>(type: "smallint", nullable: false),
                    Triples = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameStatistics_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameStatistics_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });

            migrationBuilder.CreateIndex(
                name: "IX_GameStatistics_FranchiseId",
                table: "GameStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStatistics_GameId_FranchiseId_HomeVisitor",
                table: "GameStatistics",
                columns: new[] { "GameId", "FranchiseId", "HomeVisitor" });
        }
    }
}
