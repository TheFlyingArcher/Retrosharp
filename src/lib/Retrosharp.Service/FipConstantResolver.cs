using Retrosharp.Data;
using Retrosharp.Format.PlayByPlay;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class FipConstantResolver : IFipConstantResolver
    {
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IGamePitchingStatisticsRepository _gamePitchingStatisticsRepository;
        private readonly IGameEventRepository _gameEventRepository;

        public FipConstantResolver(
            IFranchiseRepository franchiseRepository,
            IPitchingRepository pitchingRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            IGameEventRepository gameEventRepository)
        {
            _franchiseRepository = franchiseRepository;
            _pitchingRepository = pitchingRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _gameEventRepository = gameEventRepository;
        }

        public async Task<FipConstantResult> ResolveAsync(int leagueId, short season)
        {
            var franchiseIdsInLeague = (await _franchiseRepository.GetByLeagueIdAsync(leagueId)).Select(f => f.Id).ToList();

            var (baseOnBalls, hitBatsmen, strikeouts, inningsPitchedOuts) = await _pitchingRepository.GetLeagueTotalsAsync(franchiseIdsInLeague, season);
            var teamEarnedRuns = await _gamePitchingStatisticsRepository.GetLeagueTeamEarnedRunsAsync(franchiseIdsInLeague, season);
            var homerunsAllowed = await _gameEventRepository.GetLeagueHomerunsAllowedAsync(franchiseIdsInLeague, season);

            return FipConstantCalculator.Calculate(teamEarnedRuns, homerunsAllowed, baseOnBalls, hitBatsmen, strikeouts, inningsPitchedOuts);
        }
    }
}
