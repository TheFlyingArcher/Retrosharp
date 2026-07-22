using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<PersonService> _logger;

        public PersonService(
            IPersonRepository personRepository,
            ILogger<PersonService> logger)
        {
            _personRepository = personRepository;
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

        public async Task<(IEnumerable<Person> Items, int TotalCount)> SearchByNameAsync(string searchTerm, int limit, int offset)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return (Enumerable.Empty<Person>(), 0);

            return await _personRepository.SearchByNameAsync(searchTerm, limit, offset);
        }
    }
}
