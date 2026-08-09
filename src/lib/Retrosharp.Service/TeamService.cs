using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Game;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class TeamService : ITeamService
    {
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IBattingRepository _battingRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IFieldingRepository _fieldingRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IGameRepository _gameRepository;

        public TeamService(
            IFranchiseRepository franchiseRepository,
            IBattingRepository battingRepository,
            IPitchingRepository pitchingRepository,
            IFieldingRepository fieldingRepository,
            IPersonRepository personRepository,
            IGameRepository gameRepository)
        {
            _franchiseRepository = franchiseRepository;
            _battingRepository = battingRepository;
            _pitchingRepository = pitchingRepository;
            _fieldingRepository = fieldingRepository;
            _personRepository = personRepository;
            _gameRepository = gameRepository;
        }

        public Task<(IEnumerable<Franchise> Items, int TotalCount)> SearchAsync(string? q, string? code, short? season, int limit, int offset) =>
            _franchiseRepository.SearchAsync(q, code, season, limit, offset);

        public Task<Franchise> GetByIdAsync(int id) => _franchiseRepository.GetByIdAsync(id);

        public async Task<IEnumerable<Person>> GetRosterAsync(int franchiseId, short? season)
        {
            var battingPersonIds = (await _battingRepository.GetByFranchiseAsync(franchiseId, season)).Select(b => b.PersonId);
            var pitchingPersonIds = (await _pitchingRepository.GetByFranchiseAsync(franchiseId, season)).Select(p => p.PersonId);
            var fieldingPersonIds = (await _fieldingRepository.GetByFranchiseAsync(franchiseId, season)).Select(f => f.PersonId);

            var personIds = battingPersonIds.Concat(pitchingPersonIds).Concat(fieldingPersonIds).Distinct();

            var roster = new List<Person>();
            foreach (var personId in personIds)
            {
                var person = await _personRepository.GetByIdAsync(personId);
                if (person != null)
                    roster.Add(person);
            }

            return roster.OrderBy(p => p.Surname).ThenBy(p => p.UseName).ToList();
        }

        public async Task<IEnumerable<ManagerTenure>> GetManagerHistoryAsync(int franchiseId, short season)
        {
            var games = (await _gameRepository.GetByFranchiseSeasonAsync(franchiseId, season)).ToList();

            var personCache = new Dictionary<int, Person>();
            async Task<Person> ResolvePersonAsync(int personId)
            {
                if (!personCache.TryGetValue(personId, out var person))
                {
                    person = await _personRepository.GetByIdAsync(personId);
                    personCache[personId] = person;
                }

                return person;
            }

            var tenures = new List<ManagerTenure>();
            int? currentManagerId = null;
            ManagerTenure? currentTenure = null;

            // GetByFranchiseSeasonAsync already orders games chronologically (date, then game
            // number for doubleheaders), so collapsing consecutive same-manager games into one
            // tenure only needs a single forward pass -- no re-sorting or grouping required.
            foreach (var game in games)
            {
                var managerId = game.HomeFranchiseId == franchiseId ? game.HomeManagerId : game.VisitorManagerId;

                if (currentTenure != null && managerId == currentManagerId)
                {
                    currentTenure.ToDate = game.GameDate;
                    continue;
                }

                currentTenure = new ManagerTenure
                {
                    Manager = await ResolvePersonAsync(managerId),
                    FromDate = game.GameDate,
                    ToDate = game.GameDate
                };
                currentManagerId = managerId;
                tenures.Add(currentTenure);
            }

            return tenures;
        }
    }
}
