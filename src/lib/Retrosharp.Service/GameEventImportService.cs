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
    /// and GameEventFieldingCredit for every game in it, as a single atomic batch. See
    /// spec/game-event.md and spec/phase-1-build-plan.md Step 6b.
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

            var (inserted, skipped) = await _gameEventRepository.BulkInsertAsync(records);

            var result = new GameEventImportResult
            {
                GamesInserted = inserted,
                GamesSkipped = skipped
            };

            _logger.LogInformation(
                "Game event import for '{FilePath}': {GamesInserted} games inserted, {GamesSkipped} games skipped.",
                filePath, result.GamesInserted, result.GamesSkipped);

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

            return new GameEventRecord { GameId = resolvedGame.Id, Plays = plays };
        }

        private async Task<IReadOnlyDictionary<string, int>> ResolvePersonIdsAsync(EventFileGame game)
        {
            var retrosheetIds = game.Records
                .SelectMany(record => record switch
                {
                    LineupRecord lineup => new[] { lineup.RetrosheetId },
                    PlayRecord play => new[] { play.RetrosheetId },
                    AdjustmentRecord { AdjustmentTypeCode: "radj" } radj => new[] { radj.RetrosheetId },
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
