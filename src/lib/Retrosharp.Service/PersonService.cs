using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IBattingRepository _battingRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IFieldingRepository _fieldingRepository;
        private readonly ILogger<PersonService> _logger;

        public PersonService(
            IPersonRepository personRepository,
            IBattingRepository battingRepository,
            IPitchingRepository pitchingRepository,
            IFieldingRepository fieldingRepository,
            ILogger<PersonService> logger)
        {
            _personRepository = personRepository;
            _battingRepository = battingRepository;
            _pitchingRepository = pitchingRepository;
            _fieldingRepository = fieldingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _personRepository.GetAllAsync();
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _personRepository.GetByIdAsync(id);
        }

        public async Task<Person> GetByRetrosheetIdAsync(string retrosheetId)
        {
            return await _personRepository.GetByRetrosheetIdAsync(retrosheetId);
        }

        public async Task<Person> GetWithCareerStatsAsync(int personId)
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
                return null;

            // Note: In a more complete implementation, you would load batting, pitching, 
            // and fielding stats and attach them to the person object
            // For now, this returns the person without stats loaded into the object itself

            return person;
        }

        public async Task<Person> SaveAsync(Person entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Check if person already exists by Retrosheet ID
            var existing = await _personRepository.GetByRetrosheetIdAsync(entity.RetroSheetId);

            if (existing != null)
            {
                _logger.LogInformation("Person with Retrosheet ID {RetroSheetId} already exists. Skipping insert.", entity.RetroSheetId);
                return existing;
            }

            return await _personRepository.CreateAsync(entity);
        }

        public async Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Person>();

            return await _personRepository.SearchByNameAsync(searchTerm);
        }
    }
}
