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

        public async Task<(int Added, int Updated)> BulkUpsertAsync(IEnumerable<Person> people)
        {
            const int saveChangesBatchSize = 1000;

            var existingByRetrosheetId = await Context.People
                .ToDictionaryAsync(p => p.RetroSheetId);

            var added = 0;
            var updated = 0;
            var pendingChanges = 0;

            try
            {
                await Context.Database.BeginTransactionAsync();

                foreach (var person in people)
                {
                    if (existingByRetrosheetId.TryGetValue(person.RetroSheetId, out var existingModel))
                    {
                        // The incoming Person never carries a real Id (it's parsed from a file,
                        // not loaded from the database), so mapping onto the tracked model would
                        // otherwise overwrite its primary key with 0 -- EF Core's change tracker
                        // rejects any modification to a key property, tracked or not.
                        var existingId = existingModel.Id;
                        Mapper.Map(person, existingModel);
                        existingModel.Id = existingId;
                        updated++;
                    }
                    else
                    {
                        var model = Mapper.Map<PersonModel>(person);
                        Set.Add(model);
                        added++;
                    }

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
                return (added, updated);
            }
            catch
            {
                await Context.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
