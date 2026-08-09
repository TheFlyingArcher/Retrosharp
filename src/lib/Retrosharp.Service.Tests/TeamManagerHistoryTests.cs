using Retrosharp.Contract.Game;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service;

namespace Retrosharp.Service.Tests
{
    /// <summary>
    /// Constructed-fixture tests for <see cref="TeamService.GetManagerHistoryAsync"/> (Step 7g,
    /// see spec/phase-1-build-plan.md). Uses hand-written in-memory fakes rather than a real
    /// database or a mocking library -- <see cref="ITeamService"/> has no test project yet, and
    /// this method only exercises two of TeamService's six constructor dependencies
    /// (<see cref="IGameRepository"/>/<see cref="IPersonRepository"/>), so the other four are
    /// passed as <c>null!</c> since <see cref="TeamService.GetManagerHistoryAsync"/> never
    /// touches them.
    /// </summary>
    public class TeamManagerHistoryTests
    {
        private const int FranchiseId = 106;
        private const short Season = 2025;

        private static readonly Person Bochy = new() { Id = 1, RetroSheetId = "bochb001", FullName = "Bruce Bochy" };
        private static readonly Person Shildt = new() { Id = 2, RetroSheetId = "shilm001", FullName = "Mike Shildt" };

        private static Game MakeGame(DateTime date, byte gameNumber, int homeManagerId, int visitorManagerId, bool franchiseIsHome) => new()
        {
            GameDate = date,
            GameNumber = gameNumber,
            HomeFranchiseId = franchiseIsHome ? FranchiseId : 999,
            VisitorFranchiseId = franchiseIsHome ? 999 : FranchiseId,
            HomeManagerId = homeManagerId,
            VisitorManagerId = visitorManagerId
        };

        [Fact]
        public async Task NoManagerChange_ReturnsSingleTenureSpanningWholeSeason()
        {
            var games = new List<Game>
            {
                MakeGame(new DateTime(Season, 4, 1), 0, homeManagerId: Bochy.Id, visitorManagerId: 0, franchiseIsHome: true),
                MakeGame(new DateTime(Season, 6, 15), 0, homeManagerId: Bochy.Id, visitorManagerId: 0, franchiseIsHome: true),
                MakeGame(new DateTime(Season, 9, 28), 0, homeManagerId: Bochy.Id, visitorManagerId: 0, franchiseIsHome: true)
            };

            var service = BuildService(games, Bochy, Shildt);

            var history = (await service.GetManagerHistoryAsync(FranchiseId, Season)).ToList();

            var tenure = Assert.Single(history);
            Assert.Equal(Bochy.Id, tenure.Manager.Id);
            Assert.Equal(new DateTime(Season, 4, 1), tenure.FromDate);
            Assert.Equal(new DateTime(Season, 9, 28), tenure.ToDate);
        }

        [Fact]
        public async Task MidSeasonManagerChange_CollapsesIntoTwoCorrectlyDatedTenures()
        {
            var games = new List<Game>
            {
                MakeGame(new DateTime(Season, 4, 1), 0, homeManagerId: Bochy.Id, visitorManagerId: 0, franchiseIsHome: true),
                MakeGame(new DateTime(Season, 5, 10), 0, homeManagerId: Bochy.Id, visitorManagerId: 0, franchiseIsHome: true),
                MakeGame(new DateTime(Season, 6, 15), 0, homeManagerId: Shildt.Id, visitorManagerId: 0, franchiseIsHome: true),
                MakeGame(new DateTime(Season, 9, 28), 0, homeManagerId: Shildt.Id, visitorManagerId: 0, franchiseIsHome: true)
            };

            var service = BuildService(games, Bochy, Shildt);

            var history = (await service.GetManagerHistoryAsync(FranchiseId, Season)).ToList();

            Assert.Equal(2, history.Count);

            Assert.Equal(Bochy.Id, history[0].Manager.Id);
            Assert.Equal(new DateTime(Season, 4, 1), history[0].FromDate);
            Assert.Equal(new DateTime(Season, 5, 10), history[0].ToDate);

            Assert.Equal(Shildt.Id, history[1].Manager.Id);
            Assert.Equal(new DateTime(Season, 6, 15), history[1].FromDate);
            Assert.Equal(new DateTime(Season, 9, 28), history[1].ToDate);
        }

        [Fact]
        public async Task VisitorGames_UseVisitorManagerId_NotHomeManagerId()
        {
            // Franchise is the visitor for every game here -- the collapsing logic must read
            // VisitorManagerId, not blindly assume the requested franchise is always Home.
            var games = new List<Game>
            {
                MakeGame(new DateTime(Season, 4, 1), 0, homeManagerId: 0, visitorManagerId: Bochy.Id, franchiseIsHome: false),
                MakeGame(new DateTime(Season, 8, 1), 0, homeManagerId: 0, visitorManagerId: Bochy.Id, franchiseIsHome: false)
            };

            var service = BuildService(games, Bochy, Shildt);

            var history = (await service.GetManagerHistoryAsync(FranchiseId, Season)).ToList();

            var tenure = Assert.Single(history);
            Assert.Equal(Bochy.Id, tenure.Manager.Id);
        }

        [Fact]
        public async Task NoGamesForSeason_ReturnsEmpty()
        {
            var service = BuildService(new List<Game>(), Bochy, Shildt);

            var history = await service.GetManagerHistoryAsync(FranchiseId, Season);

            Assert.Empty(history);
        }

        private static TeamService BuildService(List<Game> games, params Person[] people) =>
            new(
                franchiseRepository: null!,
                battingRepository: null!,
                pitchingRepository: null!,
                fieldingRepository: null!,
                personRepository: new FakePersonRepository(people),
                gameRepository: new FakeGameRepository(games));

        private class FakeGameRepository : IGameRepository
        {
            private readonly List<Game> _games;

            public FakeGameRepository(List<Game> games) => _games = games;

            public Task<IEnumerable<Game>> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear) =>
                Task.FromResult(_games
                    .Where(g => (g.HomeFranchiseId == franchiseId || g.VisitorFranchiseId == franchiseId)
                        && g.GameDate.Year == seasonYear)
                    .OrderBy(g => g.GameDate)
                    .ThenBy(g => g.GameNumber)
                    .AsEnumerable());

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
        }

        private class FakePersonRepository : IPersonRepository
        {
            private readonly Dictionary<int, Person> _people;

            public FakePersonRepository(IEnumerable<Person> people) => _people = people.ToDictionary(p => p.Id);

            public Task<Person> GetByIdAsync(int id) => Task.FromResult(_people[id]);

            public Task<Person> CreateAsync(Person entity) => throw new NotImplementedException();
            public Task<IEnumerable<Person>> GetAllAsync() => throw new NotImplementedException();
            public Task<Person> UpdateAsync(Person entity) => throw new NotImplementedException();
            public Task DeleteAsync(int id) => throw new NotImplementedException();
            public Task<Person> GetByRetrosheetIdAsync(string retrosheetId) => throw new NotImplementedException();
            public Task<IEnumerable<Person>> SearchBySurnameAsync(string surname) => throw new NotImplementedException();
            public Task<(IEnumerable<Person> Items, int TotalCount)> SearchByNameAsync(string searchTerm, int limit, int offset) => throw new NotImplementedException();
            public Task<(int Added, int Updated)> BulkUpsertAsync(IEnumerable<Person> people) => throw new NotImplementedException();
        }
    }
}
