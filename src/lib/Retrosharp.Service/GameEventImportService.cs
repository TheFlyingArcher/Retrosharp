using Microsoft.Extensions.Logging;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data;
using Retrosharp.Format.EventFile;
using Retrosharp.Format.PlayByPlay;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    /// <summary>
    /// Parses a Retrosheet play-by-play event file and populates GameEvent, GameEventRunner,
    /// GameEventFieldingCredit, GameSubstitution, GameAdjustment, and GameComment for every
    /// game in it, as a single atomic batch. See spec/game-event.md and
    /// spec/phase-1-build-plan.md Steps 6b/6c.
    /// </summary>
    public class GameEventImportService : IGameEventImportService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IGameEventRepository _gameEventRepository;
        private readonly ILogger<GameEventImportService> _logger;

        public GameEventImportService(
            IGameRepository gameRepository,
            IFranchiseRepository franchiseRepository,
            IPersonRepository personRepository,
            IGameEventRepository gameEventRepository,
            ILogger<GameEventImportService> logger)
        {
            _gameRepository = gameRepository;
            _franchiseRepository = franchiseRepository;
            _personRepository = personRepository;
            _gameEventRepository = gameEventRepository;
            _logger = logger;
        }

        public async Task<GameEventImportResult> ImportAsync(string filePath)
        {
            var records = new List<GameEventRecord>();

            foreach (var game in EventFileReader.ReadGames(filePath))
            {
                records.Add(await MapToGameEventRecordAsync(game));
            }

            var (inserted, skipped, statisticsApplied, statisticsSkipped) = await _gameEventRepository.BulkInsertAsync(records);

            var result = new GameEventImportResult
            {
                GamesInserted = inserted,
                GamesSkipped = skipped,
                StatisticsApplied = statisticsApplied,
                StatisticsSkipped = statisticsSkipped
            };

            _logger.LogInformation(
                "Game event import for '{FilePath}': {GamesInserted} games inserted, {GamesSkipped} games skipped, " +
                "{StatisticsApplied} games' statistics applied, {StatisticsSkipped} games' statistics already claimed.",
                filePath, result.GamesInserted, result.GamesSkipped, result.StatisticsApplied, result.StatisticsSkipped);

            return result;
        }

        private async Task<GameEventRecord> MapToGameEventRecordAsync(EventFileGame game)
        {
            var homeFranchise = await _franchiseRepository.GetByFranchiseCodeAndDateAsync(game.HomeTeamCode, game.GameDate)
                ?? throw new InvalidOperationException($"No franchise found for code '{game.HomeTeamCode}' as of {game.GameDate:d}.");
            var visitingFranchise = await _franchiseRepository.GetByFranchiseCodeAndDateAsync(game.VisitingTeamCode, game.GameDate)
                ?? throw new InvalidOperationException($"No franchise found for code '{game.VisitingTeamCode}' as of {game.GameDate:d}.");

            // A game event file referencing a game the Game Log Parser hasn't processed yet is
            // a retryable condition per spec/game-event.md's Prerequisites -- but the retry/
            // backoff wiring to act on that is Step 6f's job, not this step's. For now, this is
            // a clear, diagnosable failure rather than a silent skip or a guess.
            var resolvedGame = await _gameRepository.GetByNaturalKeyAsync(game.GameDate, game.GameNumber, homeFranchise.Id, visitingFranchise.Id)
                ?? throw new InvalidOperationException(
                    $"No Game found for {game.GameDate:d} game {game.GameNumber} ({game.VisitingTeamCode} @ {game.HomeTeamCode}) -- " +
                    "has the corresponding Game Log file been processed yet?");

            var personIds = await ResolvePersonIdsAsync(game);

            var plays = GameEventResolver.Resolve(resolvedGame.Id, game, personIds);
            var (substitutions, adjustments, comments) = GameContextResolver.Resolve(resolvedGame.Id, game, personIds);

            var earnedRunsByPitcherId = ResolveEarnedRunsByPitcherId(game, personIds);
            var seasonYear = (short)game.GameDate.Year;
            var statistics = GameStatisticsResolver.Resolve(homeFranchise.Id, visitingFranchise.Id, seasonYear, plays, earnedRunsByPitcherId);

            LogPitchersMissingEarnedRunData(statistics, earnedRunsByPitcherId, game);

            return new GameEventRecord
            {
                GameId = resolvedGame.Id,
                Plays = plays,
                Substitutions = substitutions,
                Adjustments = adjustments,
                Comments = comments,
                Statistics = statistics
            };
        }

        private static IReadOnlyDictionary<int, short> ResolveEarnedRunsByPitcherId(EventFileGame game, IReadOnlyDictionary<string, int> personIds)
        {
            var earnedRunsByPitcherId = new Dictionary<int, short>();

            foreach (var data in game.Records.OfType<DataRecord>())
            {
                if (data.DataType != "er")
                    continue;

                earnedRunsByPitcherId[personIds[data.RetrosheetId]] = short.Parse(data.Value);
            }

            return earnedRunsByPitcherId;
        }

        // A pitcher who appears in the play-by-play with no matching "data,er,..." record is a
        // real data gap, not something to guess at -- GameStatisticsResolver already defaults
        // their EarnedRuns to 0; this just makes the gap visible rather than silent.
        private void LogPitchersMissingEarnedRunData(GameStatisticsDelta statistics, IReadOnlyDictionary<int, short> earnedRunsByPitcherId, EventFileGame game)
        {
            foreach (var pitching in statistics.Pitchings)
            {
                if (!earnedRunsByPitcherId.ContainsKey(pitching.PersonId))
                {
                    _logger.LogWarning(
                        "Game '{GameId}': no 'data,er,...' record found for pitcher PersonId {PersonId}; EarnedRuns defaulted to 0.",
                        game.GameId, pitching.PersonId);
                }
            }
        }

        private async Task<IReadOnlyDictionary<string, int>> ResolvePersonIdsAsync(EventFileGame game)
        {
            var retrosheetIds = game.Records
                .SelectMany(record => record switch
                {
                    LineupRecord lineup => new[] { lineup.RetrosheetId },
                    PlayRecord play => new[] { play.RetrosheetId },
                    AdjustmentRecord adjustment => new[] { adjustment.RetrosheetId },
                    DataRecord data => new[] { data.RetrosheetId },
                    _ => Array.Empty<string>()
                })
                .Distinct();

            var personIds = new Dictionary<string, int>();
            foreach (var retrosheetId in retrosheetIds)
            {
                var person = await _personRepository.GetByRetrosheetIdAsync(retrosheetId)
                    ?? throw new InvalidOperationException($"No person found for Retrosheet ID '{retrosheetId}'.");

                personIds[retrosheetId] = person.Id;
            }

            return personIds;
        }
    }
}
