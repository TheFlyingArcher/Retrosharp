using System;
using System.Collections.Generic;
using System.Linq;

using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameEventRepository : IGameEventRepository
    {
        private readonly RetrosharpContext _context;
        private readonly IMapper _mapper;
        private readonly IGameStatisticsRepository _gameStatisticsRepository;

        public GameEventRepository(RetrosharpContext context, IMapper mapper, IGameStatisticsRepository gameStatisticsRepository)
        {
            _context = context;
            _mapper = mapper;
            _gameStatisticsRepository = gameStatisticsRepository;
        }

        public async Task<(int GamesInserted, int GamesSkipped, int StatisticsApplied, int StatisticsSkipped)> BulkInsertAsync(IEnumerable<GameEventRecord> records)
        {
            var existingGameIds = (await _context.Set<GameEventModel>()
                    .Select(e => e.GameId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();

            var gamesInserted = 0;
            var gamesSkipped = 0;
            var statisticsApplied = 0;
            var statisticsSkipped = 0;

            foreach (var record in records)
            {
                // Per-game, not per-file: the statistics claim below (Step 6d) needs its own
                // transaction on this same DbContext, and EF Core doesn't support a nested
                // transaction on one connection. Committing each game's GameEvent-family graph
                // before moving to its statistics also means a crash mid-file leaves only
                // fully-completed games behind, not partially-inserted ones -- every already-
                // committed game is either fully done (event data + statistics) or untouched,
                // and a re-run resumes correctly via the existing per-game idempotency checks.
                if (!existingGameIds.Contains(record.GameId))
                {
                    try
                    {
                        await _context.Database.BeginTransactionAsync();

                        foreach (var play in record.Plays)
                        {
                            var eventModel = _mapper.Map<GameEventModel>(play.Event);

                            foreach (var runnerRecord in play.Runners)
                            {
                                var runnerModel = _mapper.Map<GameEventRunnerModel>(runnerRecord.Runner);

                                foreach (var credit in runnerRecord.FieldingCredits)
                                {
                                    var creditModel = _mapper.Map<GameEventFieldingCreditModel>(credit);

                                    // GameEventFieldingCreditModel has two independent required FKs
                                    // into the same ancestor tree: GameEventId (direct) and
                                    // GameEventRunnerId (via the runner). Placing the credit only in
                                    // runnerModel.FieldingCredits lets EF Core fix up
                                    // GameEventRunnerId -- the relationship it was reached through --
                                    // but NOT GameEventId, a separate relationship the object was
                                    // never attached through. Without this explicit assignment,
                                    // every credit would insert with GameEventId = 0.
                                    creditModel.GameEvent = eventModel;

                                    runnerModel.FieldingCredits.Add(creditModel);
                                }

                                eventModel.Runners.Add(runnerModel);
                            }

                            _context.Set<GameEventModel>().Add(eventModel);
                        }

                        // Substitutions, adjustments, and comments are direct children of Game
                        // with GameId already known -- unlike the GameEvent graph above, there's no
                        // generated-key fixup or shared-ancestor FK to worry about, so each model
                        // is simply mapped and added directly.
                        foreach (var substitution in record.Substitutions)
                            _context.Set<GameSubstitutionModel>().Add(_mapper.Map<GameSubstitutionModel>(substitution));

                        foreach (var adjustment in record.Adjustments)
                            _context.Set<GameAdjustmentModel>().Add(_mapper.Map<GameAdjustmentModel>(adjustment));

                        foreach (var comment in record.Comments)
                            _context.Set<GameCommentModel>().Add(_mapper.Map<GameCommentModel>(comment));

                        await _context.SaveChangesAsync();
                        await _context.Database.CommitTransactionAsync();
                    }
                    catch
                    {
                        await _context.Database.RollbackTransactionAsync();
                        throw;
                    }

                    existingGameIds.Add(record.GameId);
                    gamesInserted++;
                }
                else
                {
                    gamesSkipped++;
                }

                // Attempted for every game, regardless of whether its GameEvent-family rows
                // were just inserted or already existed -- statistics-application idempotency
                // (GameEventGameStatus) is deliberately independent of raw-event idempotency,
                // so a game imported before Step 6d existed still gets its statistics applied
                // the first time this runs against it, and a crash between the two steps above
                // self-heals on the next run.
                if (await _gameStatisticsRepository.TryApplyGameStatisticsAsync(record.GameId, record.Statistics))
                    statisticsApplied++;
                else
                    statisticsSkipped++;
            }

            return (gamesInserted, gamesSkipped, statisticsApplied, statisticsSkipped);
        }

        public async Task<IEnumerable<PitcherGameEventRecord>> GetPitcherGameEventsAsync(int personId, short? season)
        {
            var query = _context.GameEvents.Where(e => e.PitcherId == personId);

            if (season.HasValue)
                query = query.Where(e => e.Game.GameDate.Year == season.Value);

            var records = await query
                .Select(e => new PitcherGameEventRecord
                {
                    // The pitcher's own franchise is whichever team is NOT at bat -- mirrors
                    // GameStatisticsResolver's fieldingFranchiseId derivation exactly.
                    FranchiseId = e.TeamAtBat == "H" ? e.Game.VisitorFranchiseId : e.Game.HomeFranchiseId,
                    SeasonYear = (short)e.Game.GameDate.Year,
                    EventType = e.EventType,
                    BattedBallType = e.BattedBallType,
                    IsSacHit = e.IsSacHit,
                    IsSacFly = e.IsSacFly
                })
                .ToListAsync();

            return records;
        }

        public async Task<int> GetLeagueHomerunsAllowedAsync(IEnumerable<int> franchiseIds, short season)
        {
            var franchiseIdList = franchiseIds.ToList();

            return await _context.GameEvents
                .Where(e => e.Game.GameDate.Year == season && e.EventType == GameEventType.HomeRun)
                .Where(e => franchiseIdList.Contains(e.TeamAtBat == "H" ? e.Game.VisitorFranchiseId : e.Game.HomeFranchiseId))
                .CountAsync();
        }

        public async Task<IEnumerable<int>> GetGameIdsAsBatterAsync(int personId, short? season)
        {
            var query = _context.GameEvents.Where(e => e.BatterId == personId);

            if (season.HasValue)
                query = query.Where(e => e.Game.GameDate.Year == season.Value);

            return await query.Select(e => e.GameId).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<int>> GetGameIdsAsPitcherAsync(int personId, short? season)
        {
            var query = _context.GameEvents.Where(e => e.PitcherId == personId);

            if (season.HasValue)
                query = query.Where(e => e.Game.GameDate.Year == season.Value);

            return await query.Select(e => e.GameId).Distinct().ToListAsync();
        }

        public async Task<IReadOnlyDictionary<int, GamePlayByPlay>> GetGamesPlayByPlayAsync(IEnumerable<int> gameIds)
        {
            var gameIdList = gameIds.ToList();

            var eventModels = await _context.GameEvents
                .Where(e => gameIdList.Contains(e.GameId))
                .Include(e => e.Runners)
                .Include(e => e.Game)
                .OrderBy(e => e.Sequence)
                .ToListAsync();

            var result = new Dictionary<int, GamePlayByPlay>();
            foreach (var group in eventModels.GroupBy(e => e.GameId))
            {
                var plays = group.Select(eventModel => new GameEventPlayRecord
                {
                    Event = _mapper.Map<GameEvent>(eventModel),
                    Runners = eventModel.Runners
                        .Select(runnerModel => new GameEventRunnerRecord
                        {
                            Runner = _mapper.Map<GameEventRunner>(runnerModel),
                            FieldingCredits = Array.Empty<GameEventFieldingCredit>()
                        })
                        .ToList()
                }).ToList();

                var firstEvent = group.First();
                result[group.Key] = new GamePlayByPlay
                {
                    HomeFranchiseId = firstEvent.Game.HomeFranchiseId,
                    VisitorFranchiseId = firstEvent.Game.VisitorFranchiseId,
                    GameDate = firstEvent.Game.GameDate,
                    Plays = plays
                };
            }

            return result;
        }
    }
}
