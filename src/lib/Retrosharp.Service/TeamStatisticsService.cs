using Retrosharp.Contract.Game;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data;
using Retrosharp.Format.PlayByPlay;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class TeamStatisticsService : ITeamStatisticsService
    {
        private readonly IGameBattingStatisticsRepository _gameBattingStatisticsRepository;
        private readonly IGameFieldingStatisticsRepository _gameFieldingStatisticsRepository;
        private readonly IGamePitchingStatisticsRepository _gamePitchingStatisticsRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IFipConstantResolver _fipConstantResolver;

        public TeamStatisticsService(
            IGameBattingStatisticsRepository gameBattingStatisticsRepository,
            IGameFieldingStatisticsRepository gameFieldingStatisticsRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            IPitchingRepository pitchingRepository,
            IGameEventRepository gameEventRepository,
            IFranchiseRepository franchiseRepository,
            ILeagueRepository leagueRepository,
            IFipConstantResolver fipConstantResolver)
        {
            _gameBattingStatisticsRepository = gameBattingStatisticsRepository;
            _gameFieldingStatisticsRepository = gameFieldingStatisticsRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _pitchingRepository = pitchingRepository;
            _gameEventRepository = gameEventRepository;
            _franchiseRepository = franchiseRepository;
            _leagueRepository = leagueRepository;
            _fipConstantResolver = fipConstantResolver;
        }

        public async Task<TeamBattingStatistics> GetBattingAsync(int franchiseId, short season)
        {
            var rows = (await _gameBattingStatisticsRepository.GetByFranchiseSeasonAsync(franchiseId, season)).ToList();
            if (rows.Count == 0)
                return null;

            return new TeamBattingStatistics
            {
                GameId = 0,
                FranchiseId = franchiseId,
                PlateAppearances = (short)rows.Sum(r => r.PlateAppearances),
                AtBats = (short)rows.Sum(r => r.AtBats),
                Hit = (short)rows.Sum(r => r.Hit),
                Doubles = (short)rows.Sum(r => r.Doubles),
                Triples = (short)rows.Sum(r => r.Triples),
                Homeruns = (short)rows.Sum(r => r.Homeruns),
                RunsBattedIn = (short)rows.Sum(r => r.RunsBattedIn),
                BaseOnBalls = (short)rows.Sum(r => r.BaseOnBalls),
                Strikeouts = (short)rows.Sum(r => r.Strikeouts),
                SacrificeFlies = (short)rows.Sum(r => r.SacrificeFlies),
                SacrificeBunts = (short)rows.Sum(r => r.SacrificeBunts),
                IntentionalBb = (short)rows.Sum(r => r.IntentionalBb),
                HitByPitches = (short)rows.Sum(r => r.HitByPitches),
                StolenBases = (short)rows.Sum(r => r.StolenBases),
                TimesCaughtStealing = (short)rows.Sum(r => r.TimesCaughtStealing),
                Runs = (short)rows.Sum(r => r.Runs),
                GroundedIntoDoublePlay = (short)rows.Sum(r => r.GroundedIntoDoublePlay)
            };
        }

        public async Task<TeamFieldingStatistics> GetFieldingAsync(int franchiseId, short season)
        {
            var rows = (await _gameFieldingStatisticsRepository.GetByFranchiseSeasonAsync(franchiseId, season)).ToList();
            if (rows.Count == 0)
                return null;

            return new TeamFieldingStatistics
            {
                GameId = 0,
                FranchiseId = franchiseId,
                Putouts = (short)rows.Sum(r => r.Putouts),
                Assists = (short)rows.Sum(r => r.Assists),
                Errors = (short)rows.Sum(r => r.Errors),
                PassedBalls = (byte)rows.Sum(r => r.PassedBalls),
                DoublePlays = (byte)rows.Sum(r => r.DoublePlays),
                TriplePlays = (byte)rows.Sum(r => r.TriplePlays)
            };
        }

        public async Task<PitchingStatistics> GetPitchingAsync(int franchiseId, short season)
        {
            var pitchingRows = (await _pitchingRepository.GetByFranchiseAsync(franchiseId, season)).ToList();
            if (pitchingRows.Count == 0)
                return null;

            // Authoritative team-earned figure, substituted for the ERA numerator instead of
            // summing each pitcher's own individually-earned runs. See spec/api.md.
            var teamEarnedRuns = await _gamePitchingStatisticsRepository.GetLeagueTeamEarnedRunsAsync([franchiseId], season);

            var teamEvents = await _gameEventRepository.GetTeamPitchingEventsAsync(franchiseId, season);
            var aggregate = PitcherEventAggregateResolver.Resolve(0, teamEvents).FirstOrDefault();

            var stats = new PitchingStatistics
            {
                FranchiseId = franchiseId,
                SeasonYear = season,
                GamesPitched = (short)pitchingRows.Sum(p => p.GamesPitched),
                GamesStarted = (short)pitchingRows.Sum(p => p.GamesStarted),
                GamesFinished = (short)pitchingRows.Sum(p => p.GamesFinished),
                CompleteGames = (short)pitchingRows.Sum(p => p.CompleteGames),
                Shutouts = (short)pitchingRows.Sum(p => p.Shutouts),
                Saves = (short)pitchingRows.Sum(p => p.Saves),
                InningsPitched = (short)pitchingRows.Sum(p => p.InningsPitched),
                Hits = (short)pitchingRows.Sum(p => p.Hits),
                Runs = (short)pitchingRows.Sum(p => p.Runs),
                EarnedRuns = (short)teamEarnedRuns,
                BaseOnBalls = (short)pitchingRows.Sum(p => p.BaseOnBalls),
                Strikeouts = (short)pitchingRows.Sum(p => p.Strikeouts),
                IntentionalBb = (short)pitchingRows.Sum(p => p.IntentionalBb),
                HitBatsmen = (short)pitchingRows.Sum(p => p.HitBatsmen),
                Balks = (short)pitchingRows.Sum(p => p.Balks),
                WildPitches = (short)pitchingRows.Sum(p => p.WildPitches),
                HomerunsAllowed = aggregate?.HomerunsAllowed ?? 0,
                FlyBallsAllowed = aggregate?.FlyBallsAllowed ?? 0,
                AtBatsAgainst = aggregate?.AtBatsAgainst ?? 0,
                SacrificeFliesAgainst = aggregate?.SacrificeFliesAgainst ?? 0
            };

            var franchise = await _franchiseRepository.GetByIdAsync(franchiseId);
            if (franchise?.LeagueId is { } leagueId)
            {
                var fip = await _fipConstantResolver.ResolveAsync(leagueId, season);
                stats.FipConstant = fip.FipConstant;
                stats.FipConstantSeasonYear = season;

                var league = await _leagueRepository.GetByIdAsync(leagueId);
                stats.FipConstantLeagueCode = league?.LeagueCode;
            }

            return stats;
        }
    }
}
