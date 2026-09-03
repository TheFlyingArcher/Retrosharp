using Microsoft.EntityFrameworkCore;

using Npgsql;

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

            var statusModel = new GameEventGameStatusModel
            {
                GameId = gameId,
                ProcessedUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            _context.Set<GameEventGameStatusModel>().Add(statusModel);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Another process already claimed this game -- expected under concurrent
                // processing (see spec/game-event.md, Considerations), not a bug.
                //
                // RollbackTransactionAsync only undoes the *database* transaction --
                // statusModel is still tracked by this DbContext's change tracker as Added.
                // Left alone, it gets swept up into whatever SaveChangesAsync call happens
                // next on this same context (this repository is called once per game inside
                // GameEventRepository.BulkInsertAsync's loop, which reuses one context across
                // every game in the file), retrying the exact same insert -- which fails again,
                // this time with no catch expecting it, crashing the entire import. Confirmed
                // against a real re-run of an interrupted docs/csv/2025PHI.EVN import: 17
                // already-claimed games each left their own stray tracked entity behind, and
                // the 18th game's own ordinary event-insert SaveChangesAsync (a completely
                // unrelated call, in GameEventRepository.cs) was the one that blew up. Detaching
                // is what actually undoes the "Add" from this method's own point of view.
                await _context.Database.RollbackTransactionAsync();
                _context.Entry(statusModel).State = EntityState.Detached;
                return false;
            }

            try
            {
                // A player's Batting/Pitching/Fielding SEASON row IS shared across event files:
                // a road game appears in the host team's file, so franchise X's players' rows
                // are written by every other team's file that hosted franchise X. Under
                // concurrent import (stress-test Step 2) that produced Postgres deadlocks --
                // two files' transactions each held one player's row lock and waited on the
                // other's, in opposite order. Fixed by acquiring the locks in a single
                // deterministic order across every transaction: sort each delta group by its
                // natural key, and keep the fixed group order (batting, then pitching, then
                // fielding). With a global acquisition order no lock cycle can form -- two
                // files touching the same rows now simply queue instead of deadlocking. Each
                // Apply*DeltaAsync additionally tolerates a concurrent insert of the same new
                // season row (unique-violation -> fall through to the update path).
                foreach (var battingDelta in delta.Battings
                    .OrderBy(d => d.PersonId).ThenBy(d => d.FranchiseId).ThenBy(d => d.SeasonYear))
                    await ApplyBattingDeltaAsync(battingDelta);

                foreach (var pitchingDelta in delta.Pitchings
                    .OrderBy(d => d.PersonId).ThenBy(d => d.FranchiseId).ThenBy(d => d.SeasonYear))
                    await ApplyPitchingDeltaAsync(pitchingDelta);

                foreach (var fieldingDelta in delta.Fieldings
                    .OrderBy(d => d.PersonId).ThenBy(d => d.FranchiseId).ThenBy(d => d.SeasonYear).ThenBy(d => d.Position))
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
            var existingId = await FindBattingIdAsync(delta);

            if (existingId == null)
            {
                var model = new BattingModel
                {
                    PersonId = delta.PersonId,
                    FranchiseId = delta.FranchiseId,
                    SeasonYear = delta.SeasonYear,
                    PlateAppearances = delta.PlateAppearances,
                    AtBats = delta.AtBats,
                    Hits = delta.Hits,
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
                    GroundedIntoDoublePlay = delta.GroundedIntoDoublePlay,
                    RunsBattedIn = delta.RunsBattedIn,
                    GamesPlayed = delta.GamesPlayed,
                    GamesStarted = delta.GamesStarted
                };
                _context.Set<BattingModel>().Add(model);

                if (await TrySaveNewSeasonRowAsync(model, "sp_batting"))
                    return;

                // Lost the insert race to a concurrently-processing game (another file that
                // hosted a road game for this player). Fall through to the additive update
                // against the row that now exists.
                existingId = await FindBattingIdAsync(delta)
                    ?? throw new DbUpdateConcurrencyException(
                        $"Batting row for person {delta.PersonId}/franchise {delta.FranchiseId}/" +
                        $"{delta.SeasonYear} disappeared after a concurrent insert; the retry will re-derive it.");
            }

            await _context.Set<BattingModel>()
                .Where(b => b.Id == existingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.PlateAppearances, b => (short)(b.PlateAppearances + delta.PlateAppearances))
                    .SetProperty(b => b.AtBats, b => (short)(b.AtBats + delta.AtBats))
                    .SetProperty(b => b.Hits, b => (short)(b.Hits + delta.Hits))
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
                    .SetProperty(b => b.GroundedIntoDoublePlay, b => (short)(b.GroundedIntoDoublePlay + delta.GroundedIntoDoublePlay))
                    .SetProperty(b => b.RunsBattedIn, b => (short)(b.RunsBattedIn + delta.RunsBattedIn))
                    .SetProperty(b => b.GamesPlayed, b => (short)(b.GamesPlayed + delta.GamesPlayed))
                    .SetProperty(b => b.GamesStarted, b => (short)(b.GamesStarted + delta.GamesStarted)));
        }

        private async Task ApplyPitchingDeltaAsync(PitchingDelta delta)
        {
            var existingId = await FindPitchingIdAsync(delta);

            if (existingId == null)
            {
                var model = new PitchingModel
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
                };
                _context.Set<PitchingModel>().Add(model);

                if (await TrySaveNewSeasonRowAsync(model, "sp_pitching"))
                    return;

                existingId = await FindPitchingIdAsync(delta)
                    ?? throw new DbUpdateConcurrencyException(
                        $"Pitching row for person {delta.PersonId}/franchise {delta.FranchiseId}/" +
                        $"{delta.SeasonYear} disappeared after a concurrent insert; the retry will re-derive it.");
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
            var existingId = await FindFieldingIdAsync(delta);

            if (existingId == null)
            {
                var model = new FieldingModel
                {
                    PersonId = delta.PersonId,
                    FranchiseId = delta.FranchiseId,
                    SeasonYear = delta.SeasonYear,
                    Position = delta.Position,
                    Putouts = delta.Putouts,
                    Assists = delta.Assists,
                    Errors = delta.Errors
                };
                _context.Set<FieldingModel>().Add(model);

                if (await TrySaveNewSeasonRowAsync(model, "sp_fielding"))
                    return;

                existingId = await FindFieldingIdAsync(delta)
                    ?? throw new DbUpdateConcurrencyException(
                        $"Fielding row for person {delta.PersonId}/franchise {delta.FranchiseId}/" +
                        $"{delta.SeasonYear}/pos {delta.Position} disappeared after a concurrent insert; " +
                        "the retry will re-derive it.");
            }

            await _context.Set<FieldingModel>()
                .Where(f => f.Id == existingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Putouts, f => (f.Putouts ?? 0) + delta.Putouts)
                    .SetProperty(f => f.Assists, f => (f.Assists ?? 0) + delta.Assists)
                    .SetProperty(f => f.Errors, f => (f.Errors ?? 0) + delta.Errors));
        }

        /// <summary>
        /// Saves a just-<c>Add</c>ed brand-new season row, guarded by a savepoint. If a
        /// concurrently-processing game (another file) inserted the same natural key first
        /// (unique violation), roll back to the savepoint -- so the outer per-game transaction
        /// isn't left in Postgres's aborted state -- detach the doomed entity, and return
        /// <see langword="false"/> so the caller falls through to its additive update. Returns
        /// <see langword="true"/> when the insert won the race. The savepoint is released on
        /// success so it doesn't accumulate across the many players in one game.
        /// </summary>
        private async Task<bool> TrySaveNewSeasonRowAsync(object model, string savepointName)
        {
            var transaction = _context.Database.CurrentTransaction
                ?? throw new InvalidOperationException("TrySaveNewSeasonRowAsync requires an open transaction.");

            await transaction.CreateSavepointAsync(savepointName);
            try
            {
                await _context.SaveChangesAsync();
                await transaction.ReleaseSavepointAsync(savepointName);
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                await transaction.RollbackToSavepointAsync(savepointName);
                _context.Entry(model).State = EntityState.Detached;
                return false;
            }
        }

        private Task<int?> FindBattingIdAsync(BattingDelta delta) =>
            _context.Set<BattingModel>()
                .Where(b => b.PersonId == delta.PersonId && b.FranchiseId == delta.FranchiseId && b.SeasonYear == delta.SeasonYear)
                .Select(b => (int?)b.Id)
                .FirstOrDefaultAsync();

        private Task<int?> FindPitchingIdAsync(PitchingDelta delta) =>
            _context.Set<PitchingModel>()
                .Where(p => p.PersonId == delta.PersonId && p.FranchiseId == delta.FranchiseId && p.SeasonYear == delta.SeasonYear)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

        private Task<int?> FindFieldingIdAsync(FieldingDelta delta) =>
            _context.Set<FieldingModel>()
                .Where(f => f.PersonId == delta.PersonId && f.FranchiseId == delta.FranchiseId
                    && f.SeasonYear == delta.SeasonYear && f.Position == delta.Position)
                .Select(f => (int?)f.Id)
                .FirstOrDefaultAsync();

        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
