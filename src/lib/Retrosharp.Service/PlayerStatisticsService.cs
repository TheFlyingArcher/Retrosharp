using Microsoft.Extensions.Logging;
using Retrosharp.Contract;
using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.League;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data;
using Retrosharp.Format.PlayByPlay;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class PlayerStatisticsService : IPlayerStatisticsService
    {
        private readonly IBattingRepository _battingRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IFieldingRepository _fieldingRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IGamePitchingStatisticsRepository _gamePitchingStatisticsRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly ILogger<PlayerStatisticsService> _logger;

        public PlayerStatisticsService(
            IBattingRepository battingRepository,
            IPitchingRepository pitchingRepository,
            IFieldingRepository fieldingRepository,
            IFranchiseRepository franchiseRepository,
            IGameEventRepository gameEventRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            ILeagueRepository leagueRepository,
            ILogger<PlayerStatisticsService> logger)
        {
            _battingRepository = battingRepository;
            _pitchingRepository = pitchingRepository;
            _fieldingRepository = fieldingRepository;
            _franchiseRepository = franchiseRepository;
            _gameEventRepository = gameEventRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _leagueRepository = leagueRepository;
            _logger = logger;
        }

        public async Task<PlayerStatLines<BattingStatistics>> GetBattingAsync(int personId, short? season)
        {
            var allBatting = await _battingRepository.GetByPersonIdAsync(personId);
            var scoped = (season.HasValue ? allBatting.Where(b => b.SeasonYear == season.Value) : allBatting).ToList();

            var franchisesById = new Dictionary<int, Franchise>();
            var rows = new List<PlayerStatLine<BattingStatistics>>();
            foreach (var source in scoped)
            {
                var franchise = await ResolveFranchiseAsync(source.FranchiseId, franchisesById);
                rows.Add(new PlayerStatLine<BattingStatistics>
                {
                    Stats = ToBattingStatistics(source),
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty
                });
            }

            var combinedTotal = scoped.Count > 1 ? SumBatting(scoped) : null;

            _logger.LogInformation("Resolved batting stats for person {PersonId}, season {Season}: {RowCount} row(s)",
                personId, season, rows.Count);

            return new PlayerStatLines<BattingStatistics> { Rows = rows, CombinedTotal = combinedTotal };
        }

        public async Task<PlayerStatLines<PitchingStatistics>> GetPitchingAsync(int personId, short? season)
        {
            var allPitching = await _pitchingRepository.GetByPersonIdAsync(personId);
            var scoped = (season.HasValue ? allPitching.Where(p => p.SeasonYear == season.Value) : allPitching).ToList();

            var pitcherEvents = await _gameEventRepository.GetPitcherGameEventsAsync(personId, season);
            var aggregatesByFranchiseSeason = PitcherEventAggregateResolver.Resolve(personId, pitcherEvents)
                .ToDictionary(a => (a.FranchiseId, a.SeasonYear));

            var franchisesById = new Dictionary<int, Franchise>();
            var leagueCodesById = new Dictionary<int, string?>();
            var fipConstantsByLeagueSeason = new Dictionary<(int LeagueId, short SeasonYear), FipConstantResult>();

            var rows = new List<PlayerStatLine<PitchingStatistics>>();
            foreach (var source in scoped)
            {
                var stats = ToPitchingStatistics(source);
                ApplyPitcherEventAggregate(stats, aggregatesByFranchiseSeason);

                var franchise = await ResolveFranchiseAsync(source.FranchiseId, franchisesById);
                if (franchise?.LeagueId is { } leagueId)
                {
                    var fip = await ResolveFipConstantAsync(leagueId, source.SeasonYear, fipConstantsByLeagueSeason);
                    stats.FipConstant = fip.FipConstant;
                    stats.FipConstantSeasonYear = source.SeasonYear;
                    stats.FipConstantLeagueCode = await ResolveLeagueCodeAsync(leagueId, leagueCodesById);
                }

                rows.Add(new PlayerStatLine<PitchingStatistics>
                {
                    Stats = stats,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty
                });
            }

            var combinedTotal = rows.Count > 1 ? SumPitching(rows.Select(r => r.Stats)) : null;

            return new PlayerStatLines<PitchingStatistics> { Rows = rows, CombinedTotal = combinedTotal };
        }

        private static void ApplyPitcherEventAggregate(
            PitchingStatistics stats,
            Dictionary<(int FranchiseId, short SeasonYear), PitcherEventAggregate> aggregatesByFranchiseSeason)
        {
            if (!aggregatesByFranchiseSeason.TryGetValue((stats.FranchiseId, stats.SeasonYear), out var aggregate))
                return;

            stats.HomerunsAllowed = aggregate.HomerunsAllowed;
            stats.FlyBallsAllowed = aggregate.FlyBallsAllowed;
            stats.AtBatsAgainst = aggregate.AtBatsAgainst;
            stats.SacrificeFliesAgainst = aggregate.SacrificeFliesAgainst;
        }

        private async Task<FipConstantResult> ResolveFipConstantAsync(
            int leagueId,
            short season,
            Dictionary<(int LeagueId, short SeasonYear), FipConstantResult> cache)
        {
            if (cache.TryGetValue((leagueId, season), out var cached))
                return cached;

            var franchiseIdsInLeague = (await _franchiseRepository.GetByLeagueIdAsync(leagueId)).Select(f => f.Id).ToList();

            var (baseOnBalls, hitBatsmen, strikeouts, inningsPitchedOuts) = await _pitchingRepository.GetLeagueTotalsAsync(franchiseIdsInLeague, season);
            var teamEarnedRuns = await _gamePitchingStatisticsRepository.GetLeagueTeamEarnedRunsAsync(franchiseIdsInLeague, season);
            var homerunsAllowed = await _gameEventRepository.GetLeagueHomerunsAllowedAsync(franchiseIdsInLeague, season);

            var result = FipConstantCalculator.Calculate(teamEarnedRuns, homerunsAllowed, baseOnBalls, hitBatsmen, strikeouts, inningsPitchedOuts);
            cache[(leagueId, season)] = result;
            return result;
        }

        private async Task<string?> ResolveLeagueCodeAsync(int leagueId, Dictionary<int, string?> cache)
        {
            if (cache.TryGetValue(leagueId, out var code))
                return code;

            var league = await _leagueRepository.GetByIdAsync(leagueId);
            code = league?.LeagueCode;
            cache[leagueId] = code;
            return code;
        }

        public async Task<PlayerStatLines<FieldingStatistics>> GetFieldingAsync(int personId, short? season)
        {
            var allFielding = await _fieldingRepository.GetByPersonIdAsync(personId);
            var scoped = (season.HasValue ? allFielding.Where(f => f.SeasonYear == season.Value) : allFielding).ToList();

            var franchisesById = new Dictionary<int, Franchise>();
            var rows = new List<PlayerStatLine<FieldingStatistics>>();
            foreach (var source in scoped)
            {
                var franchise = await ResolveFranchiseAsync(source.FranchiseId, franchisesById);
                rows.Add(new PlayerStatLine<FieldingStatistics>
                {
                    Stats = ToFieldingStatistics(source),
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty
                });
            }

            var combinedTotal = scoped.Count > 1 ? SumFielding(scoped) : null;

            return new PlayerStatLines<FieldingStatistics> { Rows = rows, CombinedTotal = combinedTotal };
        }

        private async Task<Franchise?> ResolveFranchiseAsync(int franchiseId, Dictionary<int, Franchise> cache)
        {
            if (!cache.TryGetValue(franchiseId, out var franchise))
            {
                franchise = await _franchiseRepository.GetByIdAsync(franchiseId);
                cache[franchiseId] = franchise;
            }

            return franchise;
        }

        private static BattingStatistics ToBattingStatistics(Batting b) => new()
        {
            Id = b.Id,
            PersonId = b.PersonId,
            FranchiseId = b.FranchiseId,
            SeasonYear = b.SeasonYear,
            PlateAppearances = b.PlateAppearances,
            AtBats = b.AtBats,
            Hits = b.Hits,
            Doubles = b.Doubles,
            Triples = b.Triples,
            Homeruns = b.Homeruns,
            BaseOnBalls = b.BaseOnBalls,
            Strikeouts = b.Strikeouts,
            SacrificeFlies = b.SacrificeFlies,
            SacrificeBunts = b.SacrificeBunts,
            IntentionalBb = b.IntentionalBb,
            HitByPitches = b.HitByPitches,
            StolenBases = b.StolenBases,
            TimesCaughtStealing = b.TimesCaughtStealing,
            Runs = b.Runs,
            Positions = b.Positions,
            GroundedIntoDoublePlay = b.GroundedIntoDoublePlay
        };

        private static BattingStatistics SumBatting(IEnumerable<Batting> rows) => new()
        {
            PlateAppearances = (short)rows.Sum(b => b.PlateAppearances),
            AtBats = (short)rows.Sum(b => b.AtBats),
            Hits = (short)rows.Sum(b => b.Hits),
            Doubles = (short)rows.Sum(b => b.Doubles),
            Triples = (short)rows.Sum(b => b.Triples),
            Homeruns = (short)rows.Sum(b => b.Homeruns),
            BaseOnBalls = (short)rows.Sum(b => b.BaseOnBalls),
            Strikeouts = (short)rows.Sum(b => b.Strikeouts),
            SacrificeFlies = (short)rows.Sum(b => b.SacrificeFlies),
            SacrificeBunts = (short)rows.Sum(b => b.SacrificeBunts),
            IntentionalBb = (short)rows.Sum(b => b.IntentionalBb),
            HitByPitches = (short)rows.Sum(b => b.HitByPitches),
            StolenBases = (short)rows.Sum(b => b.StolenBases),
            TimesCaughtStealing = (short)rows.Sum(b => b.TimesCaughtStealing),
            Runs = (short)rows.Sum(b => b.Runs),
            Positions = (short)rows.Sum(b => b.Positions),
            GroundedIntoDoublePlay = (short)rows.Sum(b => b.GroundedIntoDoublePlay)
        };

        private static PitchingStatistics ToPitchingStatistics(Pitching p) => new()
        {
            Id = p.Id,
            PersonId = p.PersonId,
            FranchiseId = p.FranchiseId,
            SeasonYear = p.SeasonYear,
            Position = p.Position,
            GamesPitched = p.GamesPitched,
            GamesStarted = p.GamesStarted,
            GamesFinished = p.GamesFinished,
            CompleteGames = p.CompleteGames,
            Shutouts = p.Shutouts,
            Saves = p.Saves,
            InningsPitched = p.InningsPitched,
            Hits = p.Hits,
            Runs = p.Runs,
            EarnedRuns = p.EarnedRuns,
            BaseOnBalls = p.BaseOnBalls,
            Strikeouts = p.Strikeouts,
            IntentionalBb = p.IntentionalBb,
            HitBatsmen = p.HitBatsmen,
            Balks = p.Balks,
            WildPitches = p.WildPitches
        };

        private static PitchingStatistics SumPitching(IEnumerable<PitchingStatistics> rows)
        {
            var rowList = rows.ToList();
            var totalOuts = rowList.Sum(p => p.InningsPitched);

            // Blend each row's own FIP constant, weighted by its innings pitched -- a season
            // split across two leagues (a mid-season AL/NL trade) shouldn't pick one league's
            // constant arbitrarily. See spec/api.md.
            var rowsWithConstant = rowList.Where(p => p.FipConstant.HasValue).ToList();
            var weightedOuts = rowsWithConstant.Sum(p => p.InningsPitched);
            double? blendedFipConstant = weightedOuts > 0
                ? rowsWithConstant.Sum(p => p.FipConstant!.Value * p.InningsPitched) / weightedOuts
                : null;

            var leagueCodes = rowList.Select(p => p.FipConstantLeagueCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var seasons = rowList.Select(p => p.FipConstantSeasonYear).Distinct().ToList();

            return new PitchingStatistics
            {
                GamesPitched = (short)rowList.Sum(p => p.GamesPitched),
                GamesStarted = (short)rowList.Sum(p => p.GamesStarted),
                GamesFinished = (short)rowList.Sum(p => p.GamesFinished),
                CompleteGames = (short)rowList.Sum(p => p.CompleteGames),
                Shutouts = (short)rowList.Sum(p => p.Shutouts),
                Saves = (short)rowList.Sum(p => p.Saves),
                InningsPitched = (short)totalOuts,
                Hits = (short)rowList.Sum(p => p.Hits),
                Runs = (short)rowList.Sum(p => p.Runs),
                EarnedRuns = (short)rowList.Sum(p => p.EarnedRuns),
                BaseOnBalls = (short)rowList.Sum(p => p.BaseOnBalls),
                Strikeouts = (short)rowList.Sum(p => p.Strikeouts),
                IntentionalBb = (short)rowList.Sum(p => p.IntentionalBb),
                HitBatsmen = (short)rowList.Sum(p => p.HitBatsmen),
                Balks = (short)rowList.Sum(p => p.Balks),
                WildPitches = (short)rowList.Sum(p => p.WildPitches),
                HomerunsAllowed = rowList.Sum(p => p.HomerunsAllowed),
                FlyBallsAllowed = rowList.Sum(p => p.FlyBallsAllowed),
                AtBatsAgainst = rowList.Sum(p => p.AtBatsAgainst),
                SacrificeFliesAgainst = rowList.Sum(p => p.SacrificeFliesAgainst),
                FipConstant = blendedFipConstant,
                FipConstantLeagueCode = leagueCodes.Count switch { 0 => null, 1 => leagueCodes[0], _ => string.Join("/", leagueCodes) },
                FipConstantSeasonYear = seasons.Count == 1 ? seasons[0] : null
            };
        }

        private static FieldingStatistics ToFieldingStatistics(Fielding f) => new()
        {
            Id = f.Id,
            PersonId = f.PersonId,
            FranchiseId = f.FranchiseId,
            SeasonYear = f.SeasonYear,
            Position = f.Position,
            Putouts = f.Putouts,
            Assists = f.Assists,
            Errors = f.Errors,
            PassedBalls = f.PassedBalls,
            DoublePlays = f.DoublePlays,
            TriplePlays = f.TriplePlays
        };

        private static FieldingStatistics SumFielding(IEnumerable<Fielding> rows) => new()
        {
            Putouts = rows.Sum(f => f.Putouts ?? 0),
            Assists = rows.Sum(f => f.Assists ?? 0),
            Errors = rows.Sum(f => f.Errors ?? 0),
            PassedBalls = rows.Sum(f => f.PassedBalls ?? 0),
            DoublePlays = rows.Sum(f => f.DoublePlays ?? 0),
            TriplePlays = rows.Sum(f => f.TriplePlays ?? 0)
        };
    }
}
