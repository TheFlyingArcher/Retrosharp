using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ballpark",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ParkName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    City = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StateProvinceCountry = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FirstGame = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastGame = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ballpark", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LeagueName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_League", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetroSheetId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UseName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthCity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BirthStateProvince = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BirthCountry = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeathCity = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DeathStateProvince = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DeathCountry = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Cemetery = table.Column<string>(type: "nvarchar(72)", maxLength: 72, nullable: false),
                    CemeteryCity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CemeteryStateProv = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CemeteryCountry = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CemeteryNote = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BirthName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AlternateName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PlayerDebutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlayerLastDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoachDebutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoachLastDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerDebutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerLastDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UmpireDebutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UmpireLastDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Bats = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Throws = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Height = table.Column<float>(type: "real", nullable: true),
                    Weight = table.Column<float>(type: "real", nullable: true),
                    IsHof = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Franchise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    FranchiseIdentifier = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    FranchiseCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    DivisionCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FranchiseLocation = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AlternateNickname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FranchiseStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FranchiseEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlayingCity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PlayingState = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Franchise_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Batting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: true),
                    PlateAppearances = table.Column<short>(type: "smallint", nullable: false),
                    AtBats = table.Column<short>(type: "smallint", nullable: false),
                    Hit = table.Column<short>(type: "smallint", nullable: false),
                    Doubles = table.Column<short>(type: "smallint", nullable: false),
                    Triples = table.Column<short>(type: "smallint", nullable: false),
                    Homeruns = table.Column<short>(type: "smallint", nullable: false),
                    BaseOnBalls = table.Column<short>(type: "smallint", nullable: false),
                    Strikeouts = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeFlies = table.Column<short>(type: "smallint", nullable: false),
                    SacrificeBunts = table.Column<short>(type: "smallint", nullable: false),
                    IntentionalBb = table.Column<short>(type: "smallint", nullable: false),
                    HitByPitches = table.Column<short>(type: "smallint", nullable: false),
                    StolenBases = table.Column<short>(type: "smallint", nullable: false),
                    TimesCaughtStealing = table.Column<short>(type: "smallint", nullable: false),
                    Runs = table.Column<short>(type: "smallint", nullable: false),
                    Positions = table.Column<short>(type: "smallint", nullable: false),
                    GroundedIntoDoublePlay = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batting_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batting_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fielding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: true),
                    Putouts = table.Column<int>(type: "int", nullable: true),
                    Assists = table.Column<int>(type: "int", nullable: true),
                    Errors = table.Column<int>(type: "int", nullable: true),
                    PassedBalls = table.Column<int>(type: "int", nullable: true),
                    DoublePlays = table.Column<int>(type: "int", nullable: true),
                    TriplePlays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fielding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fielding_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fielding_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    GameWeekDay = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    GameDayNight = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    VisitorFranchiseId = table.Column<int>(type: "int", nullable: false),
                    VisitorGameNumber = table.Column<int>(type: "int", nullable: false),
                    VisitorRuns = table.Column<byte>(type: "tinyint", nullable: false),
                    VisitorHits = table.Column<byte>(type: "tinyint", nullable: true),
                    VisitorErrors = table.Column<byte>(type: "tinyint", nullable: true),
                    VisitorLineScore = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    VisitorManagerId = table.Column<int>(type: "int", nullable: false),
                    HomeFranchiseId = table.Column<int>(type: "int", nullable: false),
                    HomeGameNumber = table.Column<int>(type: "int", nullable: false),
                    HomeTeamRuns = table.Column<byte>(type: "tinyint", nullable: false),
                    HomeHits = table.Column<byte>(type: "tinyint", nullable: true),
                    HomeErrors = table.Column<byte>(type: "tinyint", nullable: true),
                    HomeLineScore = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HomeManagerId = table.Column<int>(type: "int", nullable: false),
                    BallparkId = table.Column<int>(type: "int", nullable: false),
                    GameLengthMinutes = table.Column<short>(type: "smallint", nullable: true),
                    ParkAttendance = table.Column<int>(type: "int", nullable: true),
                    UmpireHomeId = table.Column<int>(type: "int", nullable: true),
                    UmpireFirstId = table.Column<int>(type: "int", nullable: true),
                    UmpireSecondId = table.Column<int>(type: "int", nullable: true),
                    UmpireThirdId = table.Column<int>(type: "int", nullable: true),
                    UmpireLeftId = table.Column<int>(type: "int", nullable: true),
                    UmpireRightId = table.Column<int>(type: "int", nullable: true),
                    WinningPitcherId = table.Column<int>(type: "int", nullable: true),
                    LosingPitcherId = table.Column<int>(type: "int", nullable: true),
                    SavingPitcherId = table.Column<int>(type: "int", nullable: true),
                    GameWinningBatterId = table.Column<int>(type: "int", nullable: true),
                    GameNotes = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game_Ballpark_BallparkId",
                        column: x => x.BallparkId,
                        principalTable: "Ballpark",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Franchise_HomeFranchiseId",
                        column: x => x.HomeFranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Franchise_VisitorFranchiseId",
                        column: x => x.VisitorFranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_GameWinningBatterId",
                        column: x => x.GameWinningBatterId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_HomeManagerId",
                        column: x => x.HomeManagerId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_LosingPitcherId",
                        column: x => x.LosingPitcherId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_SavingPitcherId",
                        column: x => x.SavingPitcherId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireFirstId",
                        column: x => x.UmpireFirstId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireHomeId",
                        column: x => x.UmpireHomeId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireLeftId",
                        column: x => x.UmpireLeftId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireRightId",
                        column: x => x.UmpireRightId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireSecondId",
                        column: x => x.UmpireSecondId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_UmpireThirdId",
                        column: x => x.UmpireThirdId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_VisitorManagerId",
                        column: x => x.VisitorManagerId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Person_WinningPitcherId",
                        column: x => x.WinningPitcherId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PitchingModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    GamesPitched = table.Column<short>(type: "smallint", nullable: false),
                    GamesStarted = table.Column<short>(type: "smallint", nullable: false),
                    GamesFinished = table.Column<short>(type: "smallint", nullable: false),
                    CompleteGames = table.Column<short>(type: "smallint", nullable: false),
                    Shutouts = table.Column<short>(type: "smallint", nullable: false),
                    Saves = table.Column<short>(type: "smallint", nullable: false),
                    InningsPitched = table.Column<short>(type: "smallint", nullable: false),
                    Hits = table.Column<short>(type: "smallint", nullable: false),
                    Runs = table.Column<short>(type: "smallint", nullable: false),
                    EarnedRuns = table.Column<short>(type: "smallint", nullable: false),
                    BaseOnBalls = table.Column<short>(type: "smallint", nullable: false),
                    Strikeouts = table.Column<short>(type: "smallint", nullable: false),
                    IntentionalBb = table.Column<short>(type: "smallint", nullable: false),
                    HitBatsmen = table.Column<short>(type: "smallint", nullable: false),
                    Balks = table.Column<short>(type: "smallint", nullable: false),
                    WildPitches = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PitchingModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PitchingModel_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PitchingModel_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameLineup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    HomeVisitor = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    LineupOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    BatterId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLineup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameLineup_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLineup_Person_BatterId",
                        column: x => x.BatterId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameStatistics",
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
                name: "IX_Ballpark_SiteCode",
                table: "Ballpark",
                column: "SiteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batting_FranchiseId",
                table: "Batting",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Batting_PersonId_FranchiseId_SeasonYear",
                table: "Batting",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_FranchiseId",
                table: "Fielding",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise",
                column: "FranchiseCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseIdentifier",
                table: "Franchise",
                column: "FranchiseIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_LeagueId",
                table: "Franchise",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_BallparkId",
                table: "Game",
                column: "BallparkId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameDate",
                table: "Game",
                column: "GameDate");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameDate_HomeFranchiseId_VisitorFranchiseId",
                table: "Game",
                columns: new[] { "GameDate", "HomeFranchiseId", "VisitorFranchiseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameWinningBatterId",
                table: "Game",
                column: "GameWinningBatterId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_HomeFranchiseId",
                table: "Game",
                column: "HomeFranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_HomeManagerId",
                table: "Game",
                column: "HomeManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_LosingPitcherId",
                table: "Game",
                column: "LosingPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_SavingPitcherId",
                table: "Game",
                column: "SavingPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireFirstId",
                table: "Game",
                column: "UmpireFirstId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireHomeId",
                table: "Game",
                column: "UmpireHomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireLeftId",
                table: "Game",
                column: "UmpireLeftId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireRightId",
                table: "Game",
                column: "UmpireRightId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireSecondId",
                table: "Game",
                column: "UmpireSecondId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_UmpireThirdId",
                table: "Game",
                column: "UmpireThirdId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_VisitorFranchiseId",
                table: "Game",
                column: "VisitorFranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_VisitorManagerId",
                table: "Game",
                column: "VisitorManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_WinningPitcherId",
                table: "Game",
                column: "WinningPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineup_BatterId",
                table: "GameLineup",
                column: "BatterId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineup_GameId_BatterId",
                table: "GameLineup",
                columns: new[] { "GameId", "BatterId" });

            migrationBuilder.CreateIndex(
                name: "IX_GameStatistics_FranchiseId",
                table: "GameStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStatistics_GameId_FranchiseId_HomeVisitor",
                table: "GameStatistics",
                columns: new[] { "GameId", "FranchiseId", "HomeVisitor" });

            migrationBuilder.CreateIndex(
                name: "IX_League_LeagueCode",
                table: "League",
                column: "LeagueCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_RetroSheetId",
                table: "Person",
                column: "RetroSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_Surname",
                table: "Person",
                column: "Surname");

            migrationBuilder.CreateIndex(
                name: "IX_Person_UseName",
                table: "Person",
                column: "UseName");

            migrationBuilder.CreateIndex(
                name: "IX_PitchingModel_FranchiseId",
                table: "PitchingModel",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_PitchingModel_PersonId_FranchiseId_SeasonYear",
                table: "PitchingModel",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Batting");

            migrationBuilder.DropTable(
                name: "Fielding");

            migrationBuilder.DropTable(
                name: "GameLineup");

            migrationBuilder.DropTable(
                name: "GameStatistics");

            migrationBuilder.DropTable(
                name: "PitchingModel");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "Ballpark");

            migrationBuilder.DropTable(
                name: "Franchise");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "League");
        }
    }
}
