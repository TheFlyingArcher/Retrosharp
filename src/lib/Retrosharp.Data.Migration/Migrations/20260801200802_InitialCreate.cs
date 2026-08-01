using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ParkName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    City = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StateProvinceCountry = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FirstGame = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastGame = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ballpark", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    LeagueName = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_League", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RetroSheetId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Surname = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    UseName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FullName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BirthCity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BirthStateProvince = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BirthCountry = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DeathDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeathCity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DeathStateProvince = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DeathCountry = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Cemetery = table.Column<string>(type: "character varying(72)", maxLength: 72, nullable: true),
                    CemeteryCity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CemeteryStateProv = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CemeteryCountry = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CemeteryNote = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    BirthName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AlternateName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PlayerDebutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PlayerLastDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CoachDebutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CoachLastDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ManagerDebutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ManagerLastDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UmpireDebutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UmpireLastDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Bats = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Throws = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Height = table.Column<float>(type: "real", nullable: true),
                    Weight = table.Column<float>(type: "real", nullable: true),
                    IsHof = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Franchise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: true),
                    FranchiseIdentifier = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    FranchiseCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    DivisionCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    FranchiseLocation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AlternateNickname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FranchiseStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FranchiseEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PlayingCity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PlayingState = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: true),
                    PlateAppearances = table.Column<short>(type: "smallint", nullable: false),
                    AtBats = table.Column<short>(type: "smallint", nullable: false),
                    Hits = table.Column<short>(type: "smallint", nullable: false),
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: true),
                    Position = table.Column<byte>(type: "smallint", nullable: false),
                    Putouts = table.Column<int>(type: "integer", nullable: true),
                    Assists = table.Column<int>(type: "integer", nullable: true),
                    Errors = table.Column<int>(type: "integer", nullable: true),
                    PassedBalls = table.Column<int>(type: "integer", nullable: true),
                    DoublePlays = table.Column<int>(type: "integer", nullable: true),
                    TriplePlays = table.Column<int>(type: "integer", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GameNumber = table.Column<byte>(type: "smallint", nullable: false),
                    GameWeekDay = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    GameDayNight = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    VisitorFranchiseId = table.Column<int>(type: "integer", nullable: false),
                    VisitorGameNumber = table.Column<int>(type: "integer", nullable: false),
                    VisitorRuns = table.Column<byte>(type: "smallint", nullable: false),
                    VisitorHits = table.Column<byte>(type: "smallint", nullable: true),
                    VisitorErrors = table.Column<byte>(type: "smallint", nullable: true),
                    VisitorLineScore = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VisitorManagerId = table.Column<int>(type: "integer", nullable: false),
                    HomeFranchiseId = table.Column<int>(type: "integer", nullable: false),
                    HomeGameNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamRuns = table.Column<byte>(type: "smallint", nullable: false),
                    HomeHits = table.Column<byte>(type: "smallint", nullable: true),
                    HomeErrors = table.Column<byte>(type: "smallint", nullable: true),
                    HomeLineScore = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HomeManagerId = table.Column<int>(type: "integer", nullable: false),
                    BallparkId = table.Column<int>(type: "integer", nullable: false),
                    GameLengthMinutes = table.Column<short>(type: "smallint", nullable: true),
                    ParkAttendance = table.Column<int>(type: "integer", nullable: true),
                    UmpireHomeId = table.Column<int>(type: "integer", nullable: true),
                    UmpireFirstId = table.Column<int>(type: "integer", nullable: true),
                    UmpireSecondId = table.Column<int>(type: "integer", nullable: true),
                    UmpireThirdId = table.Column<int>(type: "integer", nullable: true),
                    UmpireLeftId = table.Column<int>(type: "integer", nullable: true),
                    UmpireRightId = table.Column<int>(type: "integer", nullable: true),
                    WinningPitcherId = table.Column<int>(type: "integer", nullable: true),
                    LosingPitcherId = table.Column<int>(type: "integer", nullable: true),
                    SavingPitcherId = table.Column<int>(type: "integer", nullable: true),
                    GameWinningBatterId = table.Column<int>(type: "integer", nullable: true),
                    GameNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
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
                name: "Pitching",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    SeasonYear = table.Column<short>(type: "smallint", nullable: false),
                    Position = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
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
                    table.PrimaryKey("PK_Pitching", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pitching_Franchise_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pitching_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameAdjustment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RecordIndex = table.Column<int>(type: "integer", nullable: false),
                    AdjustmentType = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    HomeVisitor = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RecordIndex = table.Column<int>(type: "integer", nullable: false),
                    CommentText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RecordIndex = table.Column<int>(type: "integer", nullable: false),
                    Inning = table.Column<byte>(type: "smallint", nullable: false),
                    TeamAtBat = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    BatterId = table.Column<int>(type: "integer", nullable: false),
                    PitcherId = table.Column<int>(type: "integer", nullable: false),
                    Balls = table.Column<byte>(type: "smallint", nullable: false),
                    Strikes = table.Column<byte>(type: "smallint", nullable: false),
                    FoulBallsWithTwoStrikes = table.Column<byte>(type: "smallint", nullable: false),
                    PitchSequence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RawEventText = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    SecondaryEventType = table.Column<int>(type: "integer", nullable: true),
                    BattedBallType = table.Column<int>(type: "integer", nullable: true),
                    IsSacHit = table.Column<bool>(type: "boolean", nullable: false),
                    IsSacFly = table.Column<bool>(type: "boolean", nullable: false)
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
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    HomeVisitor = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Putouts = table.Column<short>(type: "smallint", nullable: false),
                    Assists = table.Column<short>(type: "smallint", nullable: false),
                    Errors = table.Column<short>(type: "smallint", nullable: false),
                    PassedBalls = table.Column<byte>(type: "smallint", nullable: false),
                    DoublePlays = table.Column<byte>(type: "smallint", nullable: false),
                    TriplePlays = table.Column<byte>(type: "smallint", nullable: false)
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
                name: "GameLineup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    HomeVisitor = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    LineupOrder = table.Column<byte>(type: "smallint", nullable: false),
                    BatterId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
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
                name: "GamePitchingStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    FranchiseId = table.Column<int>(type: "integer", nullable: false),
                    HomeVisitor = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    PitchersUsed = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualEarnedRuns = table.Column<short>(type: "smallint", nullable: false),
                    TeamEarnedRuns = table.Column<short>(type: "smallint", nullable: false),
                    WildPitches = table.Column<byte>(type: "smallint", nullable: false),
                    Balks = table.Column<byte>(type: "smallint", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RecordIndex = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    TeamAtBat = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    BattingOrderPosition = table.Column<byte>(type: "smallint", nullable: false),
                    FieldingPosition = table.Column<byte>(type: "smallint", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameEventId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    StartBase = table.Column<int>(type: "integer", nullable: false),
                    EndBase = table.Column<int>(type: "integer", nullable: false),
                    IsOut = table.Column<bool>(type: "boolean", nullable: false),
                    IsRBI = table.Column<bool>(type: "boolean", nullable: false),
                    IsEarnedRun = table.Column<bool>(type: "boolean", nullable: false),
                    ResponsiblePitcherId = table.Column<int>(type: "integer", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameEventId = table.Column<int>(type: "integer", nullable: false),
                    GameEventRunnerId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    CreditType = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<byte>(type: "smallint", nullable: false)
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

            migrationBuilder.InsertData(
                table: "League",
                columns: new[] { "Id", "LeagueCode", "LeagueName" },
                values: new object[,]
                {
                    { 1, "AA", "American Association" },
                    { 2, "AL", "American League" },
                    { 3, "FL", "Federal League" },
                    { 4, "NA", "National Association" },
                    { 5, "NL", "National League" },
                    { 6, "PL", "Players League" },
                    { 7, "UA", "Union Association" }
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
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_FranchiseId",
                table: "Fielding",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Fielding_PersonId_FranchiseId_SeasonYear_Position",
                table: "Fielding",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise",
                column: "FranchiseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseIdentifier",
                table: "Franchise",
                column: "FranchiseIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseIdentifier_FranchiseStart",
                table: "Franchise",
                columns: new[] { "FranchiseIdentifier", "FranchiseStart" },
                unique: true);

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
                name: "IX_Game_GameDate_GameNumber_HomeFranchiseId_VisitorFranchiseId",
                table: "Game",
                columns: new[] { "GameDate", "GameNumber", "HomeFranchiseId", "VisitorFranchiseId" },
                unique: true);

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
                name: "IX_GameLineup_BatterId",
                table: "GameLineup",
                column: "BatterId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineup_GameId_BatterId",
                table: "GameLineup",
                columns: new[] { "GameId", "BatterId" });

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
                name: "IX_Pitching_FranchiseId",
                table: "Pitching",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "Pitching",
                columns: new[] { "PersonId", "FranchiseId", "SeasonYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Batting");

            migrationBuilder.DropTable(
                name: "Fielding");

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
                name: "GameLineup");

            migrationBuilder.DropTable(
                name: "GamePitchingStatistics");

            migrationBuilder.DropTable(
                name: "GameSubstitution");

            migrationBuilder.DropTable(
                name: "Pitching");

            migrationBuilder.DropTable(
                name: "GameEventRunner");

            migrationBuilder.DropTable(
                name: "GameEvent");

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
