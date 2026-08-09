using Retrosharp.Contract.Game;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Format.PlayByPlay;
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
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IGameSubstitutionRepository _gameSubstitutionRepository;
        private readonly IGameEventContextRepository _gameEventContextRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IBallparkRepository _ballparkRepository;
        private readonly IPersonRepository _personRepository;

        public GameSummaryService(
            IGameRepository gameRepository,
            IGameLineupRepository gameLineupRepository,
            IGameBattingStatisticsRepository gameBattingStatisticsRepository,
            IGamePitchingStatisticsRepository gamePitchingStatisticsRepository,
            IGameFieldingStatisticsRepository gameFieldingStatisticsRepository,
            IGameEventRepository gameEventRepository,
            IGameSubstitutionRepository gameSubstitutionRepository,
            IGameEventContextRepository gameEventContextRepository,
            IFranchiseRepository franchiseRepository,
            IBallparkRepository ballparkRepository,
            IPersonRepository personRepository)
        {
            _gameRepository = gameRepository;
            _gameLineupRepository = gameLineupRepository;
            _gameBattingStatisticsRepository = gameBattingStatisticsRepository;
            _gamePitchingStatisticsRepository = gamePitchingStatisticsRepository;
            _gameFieldingStatisticsRepository = gameFieldingStatisticsRepository;
            _gameEventRepository = gameEventRepository;
            _gameSubstitutionRepository = gameSubstitutionRepository;
            _gameEventContextRepository = gameEventContextRepository;
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

            var lineups = (await _gameLineupRepository.GetByGameIdAsync(gameId)).ToList();
            var substitutions = (await _gameSubstitutionRepository.GetByGameIdAsync(gameId)).ToList();

            // Full per-player box score lines are derived from play-by-play, same as a player's
            // own per-game log (see PlayerGameLogService), just grouped by game instead of by
            // player. A game with no imported event file has no entry in this dictionary --
            // Batters/Pitchers below then stay empty rather than erroring, the same
            // graceful-empty convention already established for play-by-play itself.
            var playByPlayByGame = await _gameEventRepository.GetGamesPlayByPlayAsync(new[] { gameId });
            GameStatisticsDelta statistics = null;
            if (playByPlayByGame.TryGetValue(gameId, out var playByPlay))
            {
                var earnedRunsByPitcherId = GameReconciliationResolver.ResolveIndependentEarnedRuns(playByPlay.Plays);
                statistics = GameStatisticsResolver.Resolve(
                    game.HomeFranchiseId,
                    game.VisitorFranchiseId,
                    (short)game.GameDate.Year,
                    playByPlay.Plays,
                    earnedRunsByPitcherId);
            }

            async Task<GameTeamBoxScore> BuildBoxScoreAsync(bool isHome)
            {
                var franchise = isHome ? homeFranchise : visitorFranchise;
                var franchiseId = isHome ? game.HomeFranchiseId : game.VisitorFranchiseId;
                var homeVisitor = isHome ? "H" : "V";

                var batters = new List<GameBoxScoreBattingParticipant>();
                var pitchers = new List<GameBoxScorePitchingParticipant>();

                if (statistics != null)
                {
                    foreach (var delta in statistics.Battings.Where(b => b.FranchiseId == franchiseId))
                    {
                        batters.Add(new GameBoxScoreBattingParticipant
                        {
                            Player = await ResolvePersonAsync(delta.PersonId),
                            Position = ResolvePosition(delta.PersonId, homeVisitor, lineups, substitutions),
                            Stats = delta
                        });
                    }

                    foreach (var delta in statistics.Pitchings.Where(p => p.FranchiseId == franchiseId))
                    {
                        pitchers.Add(new GameBoxScorePitchingParticipant
                        {
                            Player = await ResolvePersonAsync(delta.PersonId),
                            Stats = delta
                        });
                    }
                }

                return new GameTeamBoxScore
                {
                    FranchiseId = franchiseId,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty,
                    IsHome = isHome,
                    Runs = isHome ? game.HomeTeamRuns : game.VisitorRuns,
                    Hits = isHome ? game.HomeHits : game.VisitorHits,
                    Errors = isHome ? game.HomeErrors : game.VisitorErrors,
                    LineScore = isHome ? game.HomeLineScore : game.VisitorLineScore,
                    Batting = battingStats.FirstOrDefault(b => b.HomeVisitor == homeVisitor),
                    Pitching = pitchingStats.FirstOrDefault(p => p.HomeVisitor == homeVisitor),
                    Fielding = fieldingStats.FirstOrDefault(f => f.HomeVisitor == homeVisitor),
                    Batters = batters,
                    Pitchers = pitchers
                };
            }

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
                StartTimeLocal = (await _gameEventContextRepository.GetByGameIdAsync(gameId))?.StartTimeLocal,
                Ballpark = await _ballparkRepository.GetByIdAsync(game.BallparkId),
                HomeTeam = await BuildBoxScoreAsync(true),
                VisitorTeam = await BuildBoxScoreAsync(false),
                HomeLineup = await BuildLineupAsync("H"),
                VisitorLineup = await BuildLineupAsync("V"),
                VisitorStartingPitcher = await ResolvePersonAsync(game.VisitorStartingPitcherId),
                HomeStartingPitcher = await ResolvePersonAsync(game.HomeStartingPitcherId),
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

        /// <summary>
        /// Resolves the defensive position(s) a batter played in one game, combining their
        /// starting lineup slot (if any) with every subsequent position recorded via
        /// substitution (Retrosheet records a player changing position while remaining in the
        /// game as another <see cref="GameSubstitution"/> row for the same person, not just
        /// entries/exits). Returns positions in the order encountered -- starting position
        /// first, then substitution-recorded positions in the order given -- de-duplicated, and
        /// assumes <paramref name="substitutions"/> is already chronologically ordered (by
        /// RecordIndex, as <see cref="IGameSubstitutionRepository.GetByGameIdAsync"/> already
        /// returns them). A pure function -- no I/O -- so it's unit-testable without a database.
        /// </summary>
        public static string ResolvePosition(
            int personId,
            string homeVisitor,
            IEnumerable<GameLineup> lineups,
            IEnumerable<GameSubstitution> substitutions)
        {
            var positions = new List<string>();

            var startingPosition = lineups
                .FirstOrDefault(l => l.HomeVisitor == homeVisitor && l.BatterId == personId)
                ?.Position;
            if (!string.IsNullOrEmpty(startingPosition))
                positions.Add(startingPosition);

            foreach (var sub in substitutions.Where(s => s.TeamAtBat == homeVisitor && s.PersonId == personId))
            {
                var code = sub.FieldingPosition.ToString();
                if (!positions.Contains(code))
                    positions.Add(code);
            }

            return positions.Count == 0 ? null : string.Join(",", positions);
        }
    }
}
