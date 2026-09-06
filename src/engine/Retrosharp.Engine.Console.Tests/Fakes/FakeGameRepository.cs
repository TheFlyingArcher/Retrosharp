using Retrosharp.Contract.Game;
using Retrosharp.Data;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    /// <summary>
    /// <see cref="IGameRepository"/> double for BulkGameEventImportSaga tests, which only
    /// exercise <see cref="GetBySeasonAsync"/> (the "is the Game Log for this season imported?"
    /// check). Every other member throws.
    /// </summary>
    internal sealed class FakeGameRepository : IGameRepository
    {
        /// <summary>Games returned by <see cref="GetBySeasonAsync"/>; empty means "Game Log not imported".</summary>
        public List<Game> GamesBySeason { get; set; } = new();

        public Task<IEnumerable<Game>> GetBySeasonAsync(short seasonYear) =>
            Task.FromResult<IEnumerable<Game>>(GamesBySeason);

        public Task<Game> CreateAsync(Game entity) => throw new NotImplementedException();
        public Task<IEnumerable<Game>> GetAllAsync() => throw new NotImplementedException();
        public Task<Game> GetByIdAsync(int id) => throw new NotImplementedException();
        public Task<Game> UpdateAsync(Game entity) => throw new NotImplementedException();
        public Task DeleteAsync(int id) => throw new NotImplementedException();
        public Task<IEnumerable<Game>> GetByVisitorFranchiseIdAsync(int visitorFranchiseId) => throw new NotImplementedException();
        public Task<IEnumerable<Game>> GetByHomeFranchiseIdAsync(int homeFranchiseId) => throw new NotImplementedException();
        public Task<(int Added, int Skipped)> BulkInsertAsync(IEnumerable<GameLogRecord> records) => throw new NotImplementedException();
        public Task<Game> GetByNaturalKeyAsync(DateTime gameDate, byte gameNumber, int homeFranchiseId, int visitorFranchiseId) => throw new NotImplementedException();
        public Task<(IEnumerable<Game> Items, int TotalCount)> SearchAsync(DateTime? date, short? season, int? franchiseId, int limit, int offset) => throw new NotImplementedException();
        public Task<IEnumerable<Game>> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear) => throw new NotImplementedException();
    }
}
