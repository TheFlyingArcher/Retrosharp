using Retrosharp.Contract.Game;
using Retrosharp.Contract.Person;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data;
using Retrosharp.Format;
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
        private readonly IBattingRepository _battingRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IFipConstantResolver _fipConstantResolver;

        public TeamStatisticsService(
            IGameBattingStatisticsRepository gameBattingStatisticsRepository,
            IGameFieldingStatisticsRepository gameFieldingStatisticsRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            IPitchingRepository pitchingRepository,
            IBattingRepository battingRepository,
            IGameRepository gameRepository,
            IPersonRepository personRepository,
            IGameEventRepository gameEventRepository,
            IFranchiseRepository franchiseRepository,
            ILeagueRepository leagueRepository,
            IFipConstantResolver fipConstantResolver)
        {
            _gameBattingStatisticsRepository = gameBattingStatisticsRepository;
            _gameFieldingStatisticsRepository = gameFieldingStatisticsRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _pitchingRepository = pitchingRepository;
            _battingRepository = battingRepository;
            _gameRepository = gameRepository;
            _personRepository = personRepository;
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

        public async Task<(IReadOnlyList<TeamSeasonBattingSummary> Hitting, IReadOnlyList<TeamSeasonPitchingSummary> Pitching)> GetSeasonSummariesAsync(short season)
        {
            var games = (await _gameRepository.GetBySeasonAsync(season)).ToList();
            var gamesPlayedByFranchiseId = TallyGamesPlayed(games);

            // Shared across both loops below so a player who appears on multiple franchises'
            // rosters that season (a trade) or on both the batting and pitching side
            // (pre-DH-era pitcher) is only ever looked up once.
            var personCache = new Dictionary<int, Person>();

            var hitting = new List<TeamSeasonBattingSummary>();
            var pitching = new List<TeamSeasonPitchingSummary>();

            foreach (var (franchiseId, gamesPlayed) in gamesPlayedByFranchiseId)
            {
                var battingStats = await GetBattingAsync(franchiseId, season);
                if (battingStats != null)
                {
                    var batterPersonIds = (await _battingRepository.GetByFranchiseAsync(franchiseId, season))
                        .Select(b => b.PersonId)
                        .Distinct();

                    hitting.Add(new TeamSeasonBattingSummary
                    {
                        FranchiseId = franchiseId,
                        Batting = battingStats,
                        AverageAge = await ComputeAverageAgeAsync(batterPersonIds, season, personCache),
                        RunsPerGame = gamesPlayed > 0 ? (float)battingStats.Runs / gamesPlayed : 0f
                    });
                }

                var pitchingStats = await GetPitchingAsync(franchiseId, season);
                if (pitchingStats != null)
                {
                    var pitcherPersonIds = (await _pitchingRepository.GetByFranchiseAsync(franchiseId, season))
                        .Select(p => p.PersonId)
                        .Distinct();

                    pitching.Add(new TeamSeasonPitchingSummary
                    {
                        FranchiseId = franchiseId,
                        Pitching = pitchingStats,
                        AverageAge = await ComputeAverageAgeAsync(pitcherPersonIds, season, personCache),
                        RunsAllowedPerGame = gamesPlayed > 0 ? (float)pitchingStats.Runs / gamesPlayed : 0f
                    });
                }
            }

            return (hitting, pitching);
        }

        // Counts each franchise's game appearances (home or visitor) for the season -- computed
        // directly from Game, not read from FranchiseSeasonStanding, so this endpoint works
        // whether or not standings have been (re)computed for this season.
        private static Dictionary<int, int> TallyGamesPlayed(IEnumerable<Game> games)
        {
            var counts = new Dictionary<int, int>();

            foreach (var game in games)
            {
                counts[game.HomeFranchiseId] = counts.GetValueOrDefault(game.HomeFranchiseId) + 1;
                counts[game.VisitorFranchiseId] = counts.GetValueOrDefault(game.VisitorFranchiseId) + 1;
            }

            return counts;
        }

        private async Task<float> ComputeAverageAgeAsync(IEnumerable<int> personIds, short season, Dictionary<int, Person> personCache)
        {
            var ages = new List<int>();

            foreach (var personId in personIds)
            {
                if (!personCache.TryGetValue(personId, out var person))
                {
                    person = await _personRepository.GetByIdAsync(personId);
                    personCache[personId] = person;
                }

                if (BaseballAge.ComputeAge(person?.BirthDate, season) is { } age)
                    ages.Add(age);
            }

            return ages.Count > 0 ? (float)ages.Average() : 0f;
        }
    }
}
