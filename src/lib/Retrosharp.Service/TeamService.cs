using Retrosharp.Contract.Franchise;
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

        public TeamService(
            IFranchiseRepository franchiseRepository,
            IBattingRepository battingRepository,
            IPitchingRepository pitchingRepository,
            IFieldingRepository fieldingRepository,
            IPersonRepository personRepository)
        {
            _franchiseRepository = franchiseRepository;
            _battingRepository = battingRepository;
            _pitchingRepository = pitchingRepository;
            _fieldingRepository = fieldingRepository;
            _personRepository = personRepository;
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
    }
}
