using Retrosharp.Contract.Game;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class GameSummaryService : IGameSummaryService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameLineupRepository _gameLineupRepository;
        private readonly IGameBattingStatisticsRepository _gameBattingStatisticsRepository;
        private readonly IGamePitchingStatisticsRepository _gamePitchingStatisticsRepository;
        private readonly IGameFieldingStatisticsRepository _gameFieldingStatisticsRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IBallparkRepository _ballparkRepository;
        private readonly IPersonRepository _personRepository;

        public GameSummaryService(
            IGameRepository gameRepository,
            IGameLineupRepository gameLineupRepository,
            IGameBattingStatisticsRepository gameBattingStatisticsRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            IGameFieldingStatisticsRepository gameFieldingStatisticsRepository,
            IFranchiseRepository franchiseRepository,
            IBallparkRepository ballparkRepository,
            IPersonRepository personRepository)
        {
            _gameRepository = gameRepository;
            _gameLineupRepository = gameLineupRepository;
            _gameBattingStatisticsRepository = gameBattingStatisticsRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _gameFieldingStatisticsRepository = gameFieldingStatisticsRepository;
            _franchiseRepository = franchiseRepository;
            _ballparkRepository = ballparkRepository;
            _personRepository = personRepository;
        }

        public async Task<GameSummary> GetSummaryAsync(int gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                return null;

            var personCache = new Dictionary<int, Person>();
            async Task<Person> ResolvePersonAsync(int? personId)
            {
                if (personId == null)
                    return null;

                if (!personCache.TryGetValue(personId.Value, out var person))
                {
                    person = await _personRepository.GetByIdAsync(personId.Value);
                    personCache[personId.Value] = person;
                }

                return person;
            }

            var homeFranchise = await _franchiseRepository.GetByIdAsync(game.HomeFranchiseId);
            var visitorFranchise = await _franchiseRepository.GetByIdAsync(game.VisitorFranchiseId);

            var battingStats = (await _gameBattingStatisticsRepository.GetByGameIdAsync(gameId)).ToList();
            var pitchingStats = (await _gamePitchingStatisticsRepository.GetByGameIdAsync(gameId)).ToList();
            var fieldingStats = (await _gameFieldingStatisticsRepository.GetByGameIdAsync(gameId)).ToList();

            GameTeamBoxScore BuildBoxScore(bool isHome)
            {
                var franchise = isHome ? homeFranchise : visitorFranchise;
                var homeVisitor = isHome ? "H" : "V";

                return new GameTeamBoxScore
                {
                    FranchiseId = isHome ? game.HomeFranchiseId : game.VisitorFranchiseId,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty,
                    IsHome = isHome,
                    Runs = isHome ? game.HomeTeamRuns : game.VisitorRuns,
                    Hits = isHome ? game.HomeHits : game.VisitorHits,
                    Errors = isHome ? game.HomeErrors : game.VisitorErrors,
                    Batting = battingStats.FirstOrDefault(b => b.HomeVisitor == homeVisitor),
                    Pitching = pitchingStats.FirstOrDefault(p => p.HomeVisitor == homeVisitor),
                    Fielding = fieldingStats.FirstOrDefault(f => f.HomeVisitor == homeVisitor)
                };
            }

            var lineups = (await _gameLineupRepository.GetByGameIdAsync(gameId)).ToList();
            async Task<IReadOnlyList<GameLineupEntry>> BuildLineupAsync(string homeVisitor)
            {
                var entries = new List<GameLineupEntry>();
                foreach (var lineup in lineups.Where(l => l.HomeVisitor == homeVisitor).OrderBy(l => l.LineupOrder))
                {
                    entries.Add(new GameLineupEntry
                    {
                        LineupOrder = lineup.LineupOrder,
                        Batter = await ResolvePersonAsync(lineup.BatterId),
                        Position = lineup.Position
                    });
                }

                return entries;
            }

            return new GameSummary
            {
                Id = game.Id,
                GameDate = game.GameDate,
                GameNumber = game.GameNumber,
                GameDayNight = game.GameDayNight,
                GameLengthMinutes = game.GameLengthMinutes,
                ParkAttendance = game.ParkAttendance,
                GameNotes = game.GameNotes,
                Ballpark = await _ballparkRepository.GetByIdAsync(game.BallparkId),
                HomeTeam = BuildBoxScore(true),
                VisitorTeam = BuildBoxScore(false),
                HomeLineup = await BuildLineupAsync("H"),
                VisitorLineup = await BuildLineupAsync("V"),
                WinningPitcher = await ResolvePersonAsync(game.WinningPitcherId),
                LosingPitcher = await ResolvePersonAsync(game.LosingPitcherId),
                SavingPitcher = await ResolvePersonAsync(game.SavingPitcherId),
                GameWinningBatter = await ResolvePersonAsync(game.GameWinningBatterId),
                UmpireHome = await ResolvePersonAsync(game.UmpireHomeId),
                UmpireFirst = await ResolvePersonAsync(game.UmpireFirstId),
                UmpireSecond = await ResolvePersonAsync(game.UmpireSecondId),
                UmpireThird = await ResolvePersonAsync(game.UmpireThirdId),
                UmpireLeft = await ResolvePersonAsync(game.UmpireLeftId),
                UmpireRight = await ResolvePersonAsync(game.UmpireRightId)
            };
        }
    }
}
