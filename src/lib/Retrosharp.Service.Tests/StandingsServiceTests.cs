using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Game;
using Retrosharp.Contract.Standing;
using Retrosharp.Data;

namespace Retrosharp.Service.Tests
{
    /// <summary>
    /// Tests for <see cref="StandingsService.RecomputeAllSeasonsAsync"/> using hand-written
    /// in-memory fakes, matching <see cref="TeamManagerHistoryTests"/>' style.
    /// </summary>
    public class StandingsServiceTests
    {
        [Fact]
        public async Task RecomputeAllSeasonsAsync_RecomputesEveryImportedSeasonInOrder()
        {
            var games = new List<Game>
            {
                new() { GameDate = new DateTime(2001, 4, 5), HomeFranchiseId = 1, VisitorFranchiseId = 2, HomeTeamRuns = 5, VisitorRuns = 3 },
                new() { GameDate = new DateTime(2003, 4, 5), HomeFranchiseId = 1, VisitorFranchiseId = 2, HomeTeamRuns = 1, VisitorRuns = 4 }
            };
            var standings = new FakeStandingRepository();
            var service = new StandingsService(new FakeGameRepository(games), new FakeFranchiseRepository(), standings);

            var results = await service.RecomputeAllSeasonsAsync();

            Assert.Equal(new short[] { 2001, 2003 }, results.Select(r => r.SeasonYear).ToArray());
            Assert.All(results, r => Assert.Equal(2, r.FranchiseCount));
            Assert.Equal(new short[] { 2001, 2003 }, standings.ReplacedSeasons.ToArray());
        }

        [Fact]
        public async Task RecomputeAllSeasonsAsync_NoImportedGames_ReturnsEmpty()
        {
            var standings = new FakeStandingRepository();
            var service = new StandingsService(new FakeGameRepository([]), new FakeFranchiseRepository(), standings);

            Assert.Empty(await service.RecomputeAllSeasonsAsync());
            Assert.Empty(standings.ReplacedSeasons);
        }

        private class FakeGameRepository : IGameRepository
        {
            private readonly List<Game> _games;

            public FakeGameRepository(List<Game> games) => _games = games;

            public Task<IEnumerable<short>> GetSeasonYearsAsync() =>
                Task.FromResult<IEnumerable<short>>(_games.Select(g => (short)g.GameDate.Year).Distinct().OrderBy(y => y).ToList());

            public Task<IEnumerable<Game>> GetBySeasonAsync(short seasonYear) =>
                Task.FromResult<IEnumerable<Game>>(_games.Where(g => g.GameDate.Year == seasonYear).ToList());

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

        private class FakeFranchiseRepository : IFranchiseRepository
        {
            public Task<Franchise> GetByIdAsync(int id) =>
                Task.FromResult(new Franchise { Id = id, LeagueId = 1 });

            public Task<Franchise> CreateAsync(Franchise entity) => throw new NotImplementedException();
            public Task<IEnumerable<Franchise>> GetAllAsync() => throw new NotImplementedException();
            public Task<Franchise> UpdateAsync(Franchise entity) => throw new NotImplementedException();
            public Task DeleteAsync(int id) => throw new NotImplementedException();
            public Task<Franchise> GetByFranchiseCodeAsync(string franchiseCode) => throw new NotImplementedException();
            public Task<Franchise> GetByFranchiseCodeAndDateAsync(string franchiseCode, DateTime asOfDate) => throw new NotImplementedException();
            public Task<IEnumerable<Franchise>> GetByLeagueIdAsync(int leagueId) => throw new NotImplementedException();
            public Task<IEnumerable<Franchise>> GetActiveAsync() => throw new NotImplementedException();
            public Task<(IEnumerable<Franchise> Items, int TotalCount)> SearchAsync(string? q, string? code, short? season, int limit, int offset) => throw new NotImplementedException();
        }

        private class FakeStandingRepository : IFranchiseSeasonStandingRepository
        {
            public List<short> ReplacedSeasons { get; } = new();

            public Task ReplaceSeasonAsync(short seasonYear, IEnumerable<FranchiseSeasonStanding> standings)
            {
                ReplacedSeasons.Add(seasonYear);
                return Task.CompletedTask;
            }

            public Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear) => throw new NotImplementedException();
            public Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear) => throw new NotImplementedException();
            public Task<IEnumerable<FranchiseSeasonStanding>> GetAllAsync() => throw new NotImplementedException();
        }
    }
}
