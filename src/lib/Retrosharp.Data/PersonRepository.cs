using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Person;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class PersonRepository : BaseRepository<PersonModel, Person>, IPersonRepository
    {
        public PersonRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Person> GetByRetrosheetIdAsync(string retrosheetId)
        {
            var person = await Context.People
                .Where(p => p.RetroSheetId == retrosheetId)
                .ProjectToType<Person>()
                .FirstOrDefaultAsync();

            return person;
        }

        public async Task<IEnumerable<Person>> SearchBySurnameAsync(string surname)
        {
            if (string.IsNullOrWhiteSpace(surname))
                return Enumerable.Empty<Person>();

            var people = await Context.People
                .Where(p => p.Surname.Contains(surname))
                .ProjectToType<Person>()
                .ToListAsync();

            return people;
        }

        public async Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Person>();

            var searchUpper = searchTerm.ToUpper();

            var people = await Context.People
                .Where(p => 
                    p.Surname.ToUpper().Contains(searchUpper) ||
                    p.UseName.ToUpper().Contains(searchUpper) ||
                    p.FullName.ToUpper().Contains(searchUpper))
                .ProjectToType<Person>()
                .ToListAsync();

            return people;
        }
    }
}
