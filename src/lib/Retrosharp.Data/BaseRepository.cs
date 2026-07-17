using System.Linq.Expressions;

using Mapster;

using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public abstract class BaseRepository<TM, TC> : IRepository<TC>
        where TM : DbModel
        where TC : Entity
    {
        protected BaseRepository(RetrosharpContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
            Set = context.Set<TM>();
        }

        public async Task<TC> CreateAsync(TC entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof (entity));

            try
            {
                await Context.Database.BeginTransactionAsync();
                var model = Mapper.Map<TM>(entity);
                Set.Add(model);
                await Context.SaveChangesAsync();
                await Context.Database.CommitTransactionAsync();
                return entity;
            }
            catch
            {
                await Context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var model = await Set.FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
                return;

            try
            {
                await Context.Database.BeginTransactionAsync();
                Set.Remove(model);
                await Context.SaveChangesAsync();
                await Context.Database.CommitTransactionAsync();
            }
            catch
            {
                await Context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TC>> GetAllAsync()
        {
            var entities = await Set
                .ProjectToType<TC>()
                .ToListAsync();

            return entities;
        }

        public async Task<TC> GetByIdAsync(int id)
        {
            var entity = await Set
                .ProjectToType<TC>()
                .FirstOrDefaultAsync(c=>c.Id == id);

            return entity;
        }

        public async Task<TC> UpdateAsync(TC entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                await Context.Database.BeginTransactionAsync();
                var model = await Set.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (model == null)
                    throw new InvalidOperationException($"No {typeof(TM).Name} found with Id {entity.Id}.");

                // Mapster's Map(source, destination) copies every matching property, including
                // Id -- restore it explicitly afterward so a caller-supplied entity can never
                // touch the tracked model's primary key, regardless of what it happens to carry.
                var modelId = model.Id;
                Mapper.Map(entity, model);
                model.Id = modelId;
                await Context.SaveChangesAsync();
                await Context.Database.CommitTransactionAsync();
                return entity;
            }
            catch
            {
                await Context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        protected RetrosharpContext Context { get; }

        protected IMapper Mapper { get; }

        protected DbSet<TM> Set { get; }
    }
}
