using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Batting;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class BattingService : IBattingService
    {
        private readonly IBattingRepository _battingRepository;
        private readonly ILogger<BattingService> _logger;

        public BattingService(IBattingRepository battingRepository, ILogger<BattingService> logger)
        {
            _battingRepository = battingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Batting>> GetAllAsync()
        {
            return await _battingRepository.GetAllAsync();
        }

        public async Task<Batting> GetByIdAsync(int id)
        {
            return await _battingRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Batting>> GetByPersonIdAsync(int personId)
        {
            return await _battingRepository.GetByPersonIdAsync(personId);
        }

        public async Task<Batting> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear)
        {
            return await _battingRepository.GetByPersonFranchiseSeasonAsync(personId, franchiseId, seasonYear);
        }

        public async Task<Batting> SaveAsync(Batting entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Check for existing record to maintain idempotency
            if (entity.SeasonYear.HasValue)
            {
                var existing = await _battingRepository.GetByPersonFranchiseSeasonAsync(
                    entity.PersonId, 
                    entity.FranchiseId, 
                    entity.SeasonYear.Value);

                if (existing != null)
                {
                    _logger.LogInformation("Batting record already exists for person {PersonId}, franchise {FranchiseId}, season {Season}",
                        entity.PersonId, entity.FranchiseId, entity.SeasonYear);
                    return existing;
                }
            }

            return await _battingRepository.CreateAsync(entity);
        }
    }
}
