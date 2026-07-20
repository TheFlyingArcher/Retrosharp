using System;
using System.Collections.Generic;
using System.Text;

using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameRepository : BaseRepository<GameModel, Game>, IGameRepository
    {
        public GameRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<Game>> GetByHomeFranchiseIdAsync(int homeFranchiseId)
        {
            var models = await Context.Set<GameModel>()
                .Where(g => g.HomeFranchiseId == homeFranchiseId)
                .ToListAsync();

            return models.Select(m => Mapper.Map<Game>(m));
        }

        public async Task<IEnumerable<Game>> GetByVisitorFranchiseIdAsync(int visitorFranchiseId)
        {
            var models = await Context.Set<GameModel>()
                .Where(g => g.VisitorFranchiseId == visitorFranchiseId)
                .ToListAsync();

            return models.Select(m => Mapper.Map<Game>(m));
        }

        public async Task<Game> GetByNaturalKeyAsync(DateTime gameDate, byte gameNumber, int homeFranchiseId, int visitorFranchiseId)
        {
            var game = await Context.Set<GameModel>()
                .Where(g => g.GameDate == gameDate
                    && g.GameNumber == gameNumber
                    && g.HomeFranchiseId == homeFranchiseId
                    && g.VisitorFranchiseId == visitorFranchiseId)
                .ProjectToType<Game>()
                .FirstOrDefaultAsync();

            return game;
        }

        public async Task<(int Added, int Skipped)> BulkInsertAsync(IEnumerable<GameLogRecord> records)
        {
            const int saveChangesBatchSize = 200;

            var existingKeys = (await Context.Set<GameModel>()
                    .Select(g => new { g.GameDate, g.GameNumber, g.HomeFranchiseId, g.VisitorFranchiseId })
                    .ToListAsync())
                .Select(g => (g.GameDate, g.GameNumber, g.HomeFranchiseId, g.VisitorFranchiseId))
                .ToHashSet();

            var added = 0;
            var skipped = 0;
            var pendingChanges = 0;

            try
            {
                await Context.Database.BeginTransactionAsync();

                foreach (var record in records)
                {
                    var key = (record.Game.GameDate, record.Game.GameNumber, record.Game.HomeFranchiseId, record.Game.VisitorFranchiseId);
                    if (existingKeys.Contains(key))
                    {
                        skipped++;
                        continue;
                    }

                    // Children are attached via navigation properties, not by pre-setting their
                    // GameId -- the Game's Id doesn't exist yet. EF Core fixes up every child's
                    // FK from the parent's generated identity when the whole graph is saved
                    // together.
                    var model = Mapper.Map<GameModel>(record.Game);
                    model.GameLineups = record.Lineups.Select(l => Mapper.Map<GameLineupModel>(l)).ToList();
                    model.GameBattingStatistics = record.BattingStatistics.Select(s => Mapper.Map<GameBattingStatisticsModel>(s)).ToList();
                    model.GamePitchingStatistics = record.PitchingStatistics.Select(s => Mapper.Map<GamePitchingStatisticsModel>(s)).ToList();
                    model.GameFieldingStatistics = record.FieldingStatistics.Select(s => Mapper.Map<GameFieldingStatisticsModel>(s)).ToList();

                    Set.Add(model);
                    existingKeys.Add(key);
                    added++;

                    pendingChanges++;
                    if (pendingChanges >= saveChangesBatchSize)
                    {
                        await Context.SaveChangesAsync();
                        pendingChanges = 0;
                    }
                }

                if (pendingChanges > 0)
                    await Context.SaveChangesAsync();

                await Context.Database.CommitTransactionAsync();
                return (added, skipped);
            }
            catch
            {
                await Context.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
