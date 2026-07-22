using Microsoft.Extensions.Logging;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Data;
using Retrosharp.Format.PlayByPlay;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class PlayerGameLogService : IPlayerGameLogService
    {
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly ILogger<PlayerGameLogService> _logger;

        public PlayerGameLogService(
            IGameEventRepository gameEventRepository,
            IFranchiseRepository franchiseRepository,
            ILogger<PlayerGameLogService> logger)
        {
            _gameEventRepository = gameEventRepository;
            _franchiseRepository = franchiseRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PlayerGameBattingLine>> GetBattingGameLogAsync(int personId, short? season)
        {
            var gameIds = await _gameEventRepository.GetGameIdsAsBatterAsync(personId, season);
            var playByPlayByGame = await _gameEventRepository.GetGamesPlayByPlayAsync(gameIds);

            var franchiseCodesById = new Dictionary<int, string>();
            var lines = new List<PlayerGameBattingLine>();

            foreach (var (gameId, game) in playByPlayByGame)
            {
                var statistics = ResolveGameStatistics(game);
                var battingDelta = statistics.Battings.FirstOrDefault(b => b.PersonId == personId);
                if (battingDelta == null)
                    continue;

                var isHome = battingDelta.FranchiseId == game.HomeFranchiseId;
                var opponentFranchiseId = isHome ? game.VisitorFranchiseId : game.HomeFranchiseId;

                lines.Add(new PlayerGameBattingLine
                {
                    GameId = gameId,
                    GameDate = game.GameDate,
                    IsHome = isHome,
                    FranchiseCode = await ResolveFranchiseCodeAsync(battingDelta.FranchiseId, franchiseCodesById),
                    OpponentFranchiseCode = await ResolveFranchiseCodeAsync(opponentFranchiseId, franchiseCodesById),
                    Stats = battingDelta
                });
            }

            _logger.LogInformation("Resolved batting game log for person {PersonId}, season {Season}: {GameCount} game(s)",
                personId, season, lines.Count);

            return lines.OrderBy(l => l.GameDate).ToList();
        }

        public async Task<IEnumerable<PlayerGamePitchingLine>> GetPitchingGameLogAsync(int personId, short? season)
        {
            var gameIds = await _gameEventRepository.GetGameIdsAsPitcherAsync(personId, season);
            var playByPlayByGame = await _gameEventRepository.GetGamesPlayByPlayAsync(gameIds);

            var franchiseCodesById = new Dictionary<int, string>();
            var lines = new List<PlayerGamePitchingLine>();

            foreach (var (gameId, game) in playByPlayByGame)
            {
                var statistics = ResolveGameStatistics(game);
                var pitchingDelta = statistics.Pitchings.FirstOrDefault(p => p.PersonId == personId);
                if (pitchingDelta == null)
                    continue;

                var isHome = pitchingDelta.FranchiseId == game.HomeFranchiseId;
                var opponentFranchiseId = isHome ? game.VisitorFranchiseId : game.HomeFranchiseId;

                lines.Add(new PlayerGamePitchingLine
                {
                    GameId = gameId,
                    GameDate = game.GameDate,
                    IsHome = isHome,
                    FranchiseCode = await ResolveFranchiseCodeAsync(pitchingDelta.FranchiseId, franchiseCodesById),
                    OpponentFranchiseCode = await ResolveFranchiseCodeAsync(opponentFranchiseId, franchiseCodesById),
                    Stats = pitchingDelta
                });
            }

            return lines.OrderBy(l => l.GameDate).ToList();
        }

        private static GameStatisticsDelta ResolveGameStatistics(GamePlayByPlay game)
        {
            var earnedRunsByPitcherId = GameReconciliationResolver.ResolveIndependentEarnedRuns(game.Plays);

            return GameStatisticsResolver.Resolve(
                game.HomeFranchiseId,
                game.VisitorFranchiseId,
                (short)game.GameDate.Year,
                game.Plays,
                earnedRunsByPitcherId);
        }

        private async Task<string> ResolveFranchiseCodeAsync(int franchiseId, Dictionary<int, string> cache)
        {
            if (cache.TryGetValue(franchiseId, out var code))
                return code;

            var franchise = await _franchiseRepository.GetByIdAsync(franchiseId);
            code = franchise?.FranchiseCode ?? string.Empty;
            cache[franchiseId] = code;
            return code;
        }
    }
}
