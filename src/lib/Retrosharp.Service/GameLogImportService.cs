using Microsoft.Extensions.Logging;

using Retrosharp.Contract.Game;
using Retrosharp.Data;
using Retrosharp.Format;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

using FormatGameLineup = Retrosharp.Format.GameLineup;
using GameLineup = Retrosharp.Contract.Game.GameLineup;

namespace Retrosharp.Service
{
    /// <summary>
    /// Parses Retrosheet's game log file and populates Game, GameLineup, and the three
    /// Game*Statistics tables as a single atomic batch. See spec/game-log.md.
    /// </summary>
    public class GameLogImportService : IGameLogImportService
    {
        private readonly IRetrosheetFileService<GameLog> _gameLogFileService;
        private readonly IGameRepository _gameRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IBallparkRepository _ballparkRepository;
        private readonly ILogger<GameLogImportService> _logger;

        public GameLogImportService(
            IRetrosheetFileService<GameLog> gameLogFileService,
            IGameRepository gameRepository,
            IFranchiseRepository franchiseRepository,
            IPersonRepository personRepository,
            IBallparkRepository ballparkRepository,
            ILogger<GameLogImportService> logger)
        {
            _gameLogFileService = gameLogFileService;
            _gameRepository = gameRepository;
            _franchiseRepository = franchiseRepository;
            _personRepository = personRepository;
            _ballparkRepository = ballparkRepository;
            _logger = logger;
        }

        public async Task<GameLogImportResult> ImportAsync(string filePath, int seasonYear)
        {
            var gameLogs = await _gameLogFileService.ParseFileAsync(filePath);

            var records = new List<GameLogRecord>();
            foreach (var gameLog in gameLogs)
            {
                records.Add(await MapToGameLogRecordAsync(gameLog));
            }

            var (added, skipped) = await _gameRepository.BulkInsertAsync(records);

            var result = new GameLogImportResult
            {
                GamesAdded = added,
                GamesSkipped = skipped
            };

            _logger.LogInformation(
                "Game log import for season {SeasonYear}: {GamesAdded} games added, {GamesSkipped} games skipped.",
                seasonYear, result.GamesAdded, result.GamesSkipped);

            return result;
        }

        private async Task<GameLogRecord> MapToGameLogRecordAsync(GameLog gameLog)
        {
            var visitorFranchise = await _franchiseRepository.GetByFranchiseCodeAndDateAsync(gameLog.VisitorTeamCode, gameLog.GameDate)
                ?? throw new InvalidOperationException($"No franchise found for code '{gameLog.VisitorTeamCode}' as of {gameLog.GameDate:d}.");
            var homeFranchise = await _franchiseRepository.GetByFranchiseCodeAndDateAsync(gameLog.HomeTeamCode, gameLog.GameDate)
                ?? throw new InvalidOperationException($"No franchise found for code '{gameLog.HomeTeamCode}' as of {gameLog.GameDate:d}.");

            var ballpark = await _ballparkRepository.GetBySiteCodeAsync(gameLog.ParkCode)
                ?? throw new InvalidOperationException($"No ballpark found for site code '{gameLog.ParkCode}'.");

            var visitorManagerId = await ResolveRequiredPersonIdAsync(gameLog.VisitorManagerId, "visiting manager");
            var homeManagerId = await ResolveRequiredPersonIdAsync(gameLog.HomeManagerId, "home manager");

            var game = new Game
            {
                GameDate = gameLog.GameDate,
                GameNumber = ParseGameNumber(gameLog.GameNumber),
                GameWeekDay = gameLog.DayOfWeek,
                GameDayNight = gameLog.DayOrNight.ToString(),
                VisitorFranchiseId = visitorFranchise.Id,
                VisitorGameNumber = gameLog.VisitorGameNumber,
                VisitorRuns = (byte)gameLog.VisitorScore,
                VisitorHits = (byte)gameLog.VisitorHitting.Hits,
                VisitorErrors = (byte)gameLog.VisitorFielding.Errors,
                VisitorLineScore = gameLog.VisitorScoreLine,
                VisitorManagerId = visitorManagerId,
                HomeFranchiseId = homeFranchise.Id,
                HomeGameNumber = gameLog.HomeGameNumber,
                HomeTeamRuns = (byte)gameLog.HomeScore,
                HomeHits = (byte)gameLog.HomeHitting.Hits,
                HomeErrors = (byte)gameLog.HomeFielding.Errors,
                HomeLineScore = gameLog.HomeScoreLine,
                HomeManagerId = homeManagerId,
                BallparkId = ballpark.Id,
                GameLengthMinutes = (short)gameLog.GameLengthMinutes,
                ParkAttendance = gameLog.GameAttendance,
                UmpireHomeId = await ResolveOptionalPersonIdAsync(gameLog.UmpireHomeId),
                UmpireFirstId = await ResolveOptionalPersonIdAsync(gameLog.UmpireFirstId),
                UmpireSecondId = await ResolveOptionalPersonIdAsync(gameLog.UmpireSecondId),
                UmpireThirdId = await ResolveOptionalPersonIdAsync(gameLog.UmpireThirdId),
                UmpireLeftId = await ResolveOptionalPersonIdAsync(gameLog.UmpireLeftId),
                UmpireRightId = await ResolveOptionalPersonIdAsync(gameLog.UmpireRightId),
                WinningPitcherId = await ResolveOptionalPersonIdAsync(gameLog.WinningPitcherId),
                LosingPitcherId = await ResolveOptionalPersonIdAsync(gameLog.LosingPitcherId),
                SavingPitcherId = await ResolveOptionalPersonIdAsync(gameLog.SavingPitcherId),
                GameWinningBatterId = await ResolveOptionalPersonIdAsync(gameLog.GameWinningPlayerId),
                GameNotes = gameLog.AdditionalInformation
            };

            var lineups = new List<GameLineup>();
            lineups.AddRange(await FlattenLineupAsync(gameLog.VisitorStartingLineup, "V"));
            lineups.AddRange(await FlattenLineupAsync(gameLog.HomeStartingLineup, "H"));

            var battingStatistics = new List<GameBattingStatistics>
            {
                MapBattingStatistics(gameLog.VisitorHitting, visitorFranchise.Id, "V", gameLog.VisitorScore),
                MapBattingStatistics(gameLog.HomeHitting, homeFranchise.Id, "H", gameLog.HomeScore)
            };

            var pitchingStatistics = new List<GamePitchingStatistics>
            {
                MapPitchingStatistics(gameLog.VisitorPitching, visitorFranchise.Id, "V"),
                MapPitchingStatistics(gameLog.HomePitching, homeFranchise.Id, "H")
            };

            var fieldingStatistics = new List<GameFieldingStatistics>
            {
                MapFieldingStatistics(gameLog.VisitorFielding, visitorFranchise.Id, "V"),
                MapFieldingStatistics(gameLog.HomeFielding, homeFranchise.Id, "H")
            };

            return new GameLogRecord
            {
                Game = game,
                Lineups = lineups,
                BattingStatistics = battingStatistics,
                PitchingStatistics = pitchingStatistics,
                FieldingStatistics = fieldingStatistics
            };
        }

        private async Task<List<GameLineup>> FlattenLineupAsync(FormatGameLineup lineup, string homeVisitor)
        {
            var batters = new[]
            {
                lineup.LeadoffBatter,
                lineup.SecondBatter,
                lineup.ThirdBatter,
                lineup.CleanupBatter,
                lineup.FifthBatter,
                lineup.SixthBatter,
                lineup.SeventhBatter,
                lineup.EighthBatter,
                lineup.NinthBatter
            };

            var result = new List<GameLineup>(batters.Length);
            for (var i = 0; i < batters.Length; i++)
            {
                var batter = batters[i];
                var batterId = await ResolveRequiredPersonIdAsync(batter.PlayerId, $"{homeVisitor} batting order slot {i + 1}");

                result.Add(new GameLineup
                {
                    HomeVisitor = homeVisitor,
                    LineupOrder = (byte)(i + 1),
                    BatterId = batterId,
                    Position = batter.PlayerPosition
                });
            }

            return result;
        }

        private static GameBattingStatistics MapBattingStatistics(GameLogHittingStatistics stats, int franchiseId, string homeVisitor, int runs)
        {
            return new GameBattingStatistics
            {
                FranchiseId = franchiseId,
                HomeVisitor = homeVisitor,
                // Retrosheet's game log doesn't supply plate appearances directly.
                PlateAppearances = (short)(stats.AtBats + stats.BasesOnBalls + stats.TimesHitByPitch
                    + stats.SacrificeHits + stats.SacrificeFlies + stats.TimesCatchersInterference),
                AtBats = (short)stats.AtBats,
                Hit = (short)stats.Hits,
                Doubles = (short)stats.Doubles,
                Triples = (short)stats.Triples,
                Homeruns = (short)stats.Homeruns,
                RunsBattedIn = (short)stats.RunsBattedIn,
                BaseOnBalls = (short)stats.BasesOnBalls,
                Strikeouts = (short)stats.Strikeouts,
                SacrificeFlies = (short)stats.SacrificeFlies,
                SacrificeBunts = (short)stats.SacrificeHits,
                IntentionalBb = (short)stats.IntentionalBasesOnBalls,
                HitByPitches = (short)stats.TimesHitByPitch,
                StolenBases = (short)stats.StolenBases,
                TimesCaughtStealing = (short)stats.TimesCaughtStealing,
                // Team runs scored comes from the game score, not the hitting-stats sub-block.
                Runs = (short)runs,
                GroundedIntoDoublePlay = (short)stats.TimesGidp
            };
        }

        private static GamePitchingStatistics MapPitchingStatistics(GameLogPitchingStatistics stats, int franchiseId, string homeVisitor)
        {
            return new GamePitchingStatistics
            {
                FranchiseId = franchiseId,
                HomeVisitor = homeVisitor,
                PitchersUsed = (byte)stats.PitchersUsed,
                IndividualEarnedRuns = (short)stats.EarnedRuns,
                TeamEarnedRuns = (short)stats.TeamEarnedRuns,
                WildPitches = (byte)stats.WildPitches,
                Balks = (byte)stats.Balks
            };
        }

        private static GameFieldingStatistics MapFieldingStatistics(GameLogFieldingStatistics stats, int franchiseId, string homeVisitor)
        {
            return new GameFieldingStatistics
            {
                FranchiseId = franchiseId,
                HomeVisitor = homeVisitor,
                Putouts = (short)stats.Putouts,
                Assists = (short)stats.Assists,
                Errors = (short)stats.Errors,
                PassedBalls = (byte)stats.PassedBalls,
                DoublePlays = (byte)stats.DoublePlays,
                TriplePlays = (byte)stats.TriplePlays
            };
        }

        private static byte ParseGameNumber(char gameNumber)
        {
            // "A"/"B" (three-team doubleheaders) aren't handled -- GameModel.GameNumber is a
            // byte, a pre-existing schema decision that predates this step. Not present in any
            // modern season; flagged rather than silently mis-parsed.
            if (!char.IsDigit(gameNumber))
                throw new NotSupportedException($"Game number '{gameNumber}' (three-team doubleheader style) is not supported.");

            return (byte)(gameNumber - '0');
        }

        private async Task<int> ResolveRequiredPersonIdAsync(string retrosheetId, string role)
        {
            var person = await _personRepository.GetByRetrosheetIdAsync(retrosheetId)
                ?? throw new InvalidOperationException($"No person found for Retrosheet ID '{retrosheetId}' ({role}).");

            return person.Id;
        }

        private async Task<int?> ResolveOptionalPersonIdAsync(string? retrosheetId)
        {
            if (string.IsNullOrWhiteSpace(retrosheetId) || retrosheetId == "(none)")
                return null;

            var person = await _personRepository.GetByRetrosheetIdAsync(retrosheetId)
                ?? throw new InvalidOperationException($"No person found for Retrosheet ID '{retrosheetId}'.");

            return person.Id;
        }
    }
}
