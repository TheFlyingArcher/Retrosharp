using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameStatisticsRepository : IGameStatisticsRepository
    {
        private readonly RetrosharpContext _context;

        public GameStatisticsRepository(RetrosharpContext context)
        {
            _context = context;
        }

        public async Task<bool> TryApplyGameStatisticsAsync(int gameId, GameStatisticsDelta delta)
        {
            await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Set<GameEventGameStatusModel>().Add(new GameEventGameStatusModel
                {
                    GameId = gameId,
                    ProcessedUtc = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Another process already claimed this game -- expected under concurrent
                // processing (see spec/game-event.md, Considerations), not a bug.
                await _context.Database.RollbackTransactionAsync();
                return false;
            }

            try
            {
                // Row-level races on a single player's Batting/Pitching/Fielding season row
                // can't actually happen here: every game a player plays for a given franchise
                // lives in that franchise's own event file, processed sequentially by one
                // saga. The only cross-file overlap is the shared-game scenario, and that's
                // already fully serialized by the claim above -- only one saga ever reaches
                // this point for a given GameId. So a plain check-then-act (not an
                // insert/catch/fallback dance) is sufficient and safer to reason about.
                foreach (var battingDelta in delta.Battings)
                    await ApplyBattingDeltaAsync(battingDelta);

                foreach (var pitchingDelta in delta.Pitchings)
                    await ApplyPitchingDeltaAsync(pitchingDelta);

                foreach (var fieldingDelta in delta.Fieldings)
                    await ApplyFieldingDeltaAsync(fieldingDelta);

                await _context.Database.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task ApplyBattingDeltaAsync(BattingDelta delta)
        {
            var existingId = await _context.Set<BattingModel>()
                .Where(b => b.PersonId == delta.PersonId && b.FranchiseId == delta.FranchiseId && b.SeasonYear == delta.SeasonYear)
                .Select(b => (int?)b.Id)
                .FirstOrDefaultAsync();

            if (existingId == null)
            {
                _context.Set<BattingModel>().Add(new BattingModel
                {
                    PersonId = delta.PersonId,
                    FranchiseId = delta.FranchiseId,
                    SeasonYear = delta.SeasonYear,
                    PlateAppearances = delta.PlateAppearances,
                    AtBats = delta.AtBats,
                    // Contract.Batting.Hits (plural) vs Data.Model.BattingModel.Hit (singular)
                    // -- a pre-existing naming mismatch between the two layers, mapped
                    // explicitly here rather than relying on Mapster's convention-based
                    // matching (which would silently leave Hit at 0).
                    Hit = delta.Hits,
                    Doubles = delta.Doubles,
                    Triples = delta.Triples,
                    Homeruns = delta.Homeruns,
                    BaseOnBalls = delta.BaseOnBalls,
                    Strikeouts = delta.Strikeouts,
                    SacrificeFlies = delta.SacrificeFlies,
                    SacrificeBunts = delta.SacrificeBunts,
                    IntentionalBb = delta.IntentionalBb,
                    HitByPitches = delta.HitByPitches,
                    StolenBases = delta.StolenBases,
                    TimesCaughtStealing = delta.TimesCaughtStealing,
                    Runs = delta.Runs,
                    Positions = 0,
                    GroundedIntoDoublePlay = delta.GroundedIntoDoublePlay
                });

                await _context.SaveChangesAsync();
                return;
            }

            await _context.Set<BattingModel>()
                .Where(b => b.Id == existingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.PlateAppearances, b => (short)(b.PlateAppearances + delta.PlateAppearances))
                    .SetProperty(b => b.AtBats, b => (short)(b.AtBats + delta.AtBats))
                    .SetProperty(b => b.Hit, b => (short)(b.Hit + delta.Hits))
                    .SetProperty(b => b.Doubles, b => (short)(b.Doubles + delta.Doubles))
                    .SetProperty(b => b.Triples, b => (short)(b.Triples + delta.Triples))
                    .SetProperty(b => b.Homeruns, b => (short)(b.Homeruns + delta.Homeruns))
                    .SetProperty(b => b.BaseOnBalls, b => (short)(b.BaseOnBalls + delta.BaseOnBalls))
                    .SetProperty(b => b.Strikeouts, b => (short)(b.Strikeouts + delta.Strikeouts))
                    .SetProperty(b => b.SacrificeFlies, b => (short)(b.SacrificeFlies + delta.SacrificeFlies))
                    .SetProperty(b => b.SacrificeBunts, b => (short)(b.SacrificeBunts + delta.SacrificeBunts))
                    .SetProperty(b => b.IntentionalBb, b => (short)(b.IntentionalBb + delta.IntentionalBb))
                    .SetProperty(b => b.HitByPitches, b => (short)(b.HitByPitches + delta.HitByPitches))
                    .SetProperty(b => b.StolenBases, b => (short)(b.StolenBases + delta.StolenBases))
                    .SetProperty(b => b.TimesCaughtStealing, b => (short)(b.TimesCaughtStealing + delta.TimesCaughtStealing))
                    .SetProperty(b => b.Runs, b => (short)(b.Runs + delta.Runs))
                    .SetProperty(b => b.GroundedIntoDoublePlay, b => (short)(b.GroundedIntoDoublePlay + delta.GroundedIntoDoublePlay)));
        }

        private async Task ApplyPitchingDeltaAsync(PitchingDelta delta)
        {
            var existingId = await _context.Set<PitchingModel>()
                .Where(p => p.PersonId == delta.PersonId && p.FranchiseId == delta.FranchiseId && p.SeasonYear == delta.SeasonYear)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            if (existingId == null)
            {
                _context.Set<PitchingModel>().Add(new PitchingModel
                {
                    PersonId = delta.PersonId,
                    FranchiseId = delta.FranchiseId,
                    SeasonYear = delta.SeasonYear,
                    Position = "P",
                    GamesPitched = delta.GamesPitched,
                    GamesStarted = delta.GamesStarted,
                    GamesFinished = delta.GamesFinished,
                    CompleteGames = delta.CompleteGames,
                    Shutouts = delta.Shutouts,
                    Saves = delta.Saves,
                    InningsPitched = delta.InningsPitched,
                    Hits = delta.Hits,
                    Runs = delta.Runs,
                    EarnedRuns = delta.EarnedRuns,
                    BaseOnBalls = delta.BaseOnBalls,
                    Strikeouts = delta.Strikeouts,
                    IntentionalBb = delta.IntentionalBb,
                    HitBatsmen = delta.HitBatsmen,
                    Balks = delta.Balks,
                    WildPitches = delta.WildPitches
                });

                await _context.SaveChangesAsync();
                return;
            }

            await _context.Set<PitchingModel>()
                .Where(p => p.Id == existingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.GamesPitched, p => (short)(p.GamesPitched + delta.GamesPitched))
                    .SetProperty(p => p.GamesStarted, p => (short)(p.GamesStarted + delta.GamesStarted))
                    .SetProperty(p => p.GamesFinished, p => (short)(p.GamesFinished + delta.GamesFinished))
                    .SetProperty(p => p.CompleteGames, p => (short)(p.CompleteGames + delta.CompleteGames))
                    .SetProperty(p => p.Shutouts, p => (short)(p.Shutouts + delta.Shutouts))
                    .SetProperty(p => p.Saves, p => (short)(p.Saves + delta.Saves))
                    .SetProperty(p => p.InningsPitched, p => (short)(p.InningsPitched + delta.InningsPitched))
                    .SetProperty(p => p.Hits, p => (short)(p.Hits + delta.Hits))
                    .SetProperty(p => p.Runs, p => (short)(p.Runs + delta.Runs))
                    .SetProperty(p => p.EarnedRuns, p => (short)(p.EarnedRuns + delta.EarnedRuns))
                    .SetProperty(p => p.BaseOnBalls, p => (short)(p.BaseOnBalls + delta.BaseOnBalls))
                    .SetProperty(p => p.Strikeouts, p => (short)(p.Strikeouts + delta.Strikeouts))
                    .SetProperty(p => p.IntentionalBb, p => (short)(p.IntentionalBb + delta.IntentionalBb))
                    .SetProperty(p => p.HitBatsmen, p => (short)(p.HitBatsmen + delta.HitBatsmen))
                    .SetProperty(p => p.Balks, p => (short)(p.Balks + delta.Balks))
                    .SetProperty(p => p.WildPitches, p => (short)(p.WildPitches + delta.WildPitches)));
        }

        private async Task ApplyFieldingDeltaAsync(FieldingDelta delta)
        {
            var existingId = await _context.Set<FieldingModel>()
                .Where(f => f.PersonId == delta.PersonId && f.FranchiseId == delta.FranchiseId
                    && f.SeasonYear == delta.SeasonYear && f.Position == delta.Position)
                .Select(f => (int?)f.Id)
                .FirstOrDefaultAsync();

            if (existingId == null)
            {
                _context.Set<FieldingModel>().Add(new FieldingModel
                {
                    PersonId = delta.PersonId,
                    FranchiseId = delta.FranchiseId,
                    SeasonYear = delta.SeasonYear,
                    Position = delta.Position,
                    Putouts = delta.Putouts,
                    Assists = delta.Assists,
                    Errors = delta.Errors
                });

                await _context.SaveChangesAsync();
                return;
            }

            await _context.Set<FieldingModel>()
                .Where(f => f.Id == existingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Putouts, f => (f.Putouts ?? 0) + delta.Putouts)
                    .SetProperty(f => f.Assists, f => (f.Assists ?? 0) + delta.Assists)
                    .SetProperty(f => f.Errors, f => (f.Errors ?? 0) + delta.Errors));
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
