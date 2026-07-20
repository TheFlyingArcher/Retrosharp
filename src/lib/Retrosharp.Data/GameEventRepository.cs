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

        public GameEventRepository(RetrosharpContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<(int GamesInserted, int GamesSkipped)> BulkInsertAsync(IEnumerable<GameEventRecord> records)
        {
            var existingGameIds = (await _context.Set<GameEventModel>()
                    .Select(e => e.GameId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();

            var gamesInserted = 0;
            var gamesSkipped = 0;

            try
            {
                await _context.Database.BeginTransactionAsync();

                foreach (var record in records)
                {
                    if (existingGameIds.Contains(record.GameId))
                    {
                        gamesSkipped++;
                        continue;
                    }

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

                    existingGameIds.Add(record.GameId);
                    gamesInserted++;

                    // One game (plays + runners + fielding credits, typically a few hundred
                    // rows) is a natural, safe SaveChanges batch boundary -- large enough to
                    // avoid saving per-row, small enough to bound change-tracker overhead
                    // across a whole file's worth of games.
                    await _context.SaveChangesAsync();
                }

                await _context.Database.CommitTransactionAsync();
                return (gamesInserted, gamesSkipped);
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
