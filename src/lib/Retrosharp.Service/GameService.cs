using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Game;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(IGameRepository gameRepository, ILogger<GameService> logger)
        {
            _gameRepository = gameRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Game>> GetAllAsync()
        {
            return await _gameRepository.GetAllAsync();
        }

        public async Task<Game> GetByIdAsync(int id)
        {
            return await _gameRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Game>> GetByDateAsync(DateTime gameDate)
        {
            var allGames = await _gameRepository.GetAllAsync();
            return allGames.Where(g => g.GameDate.Date == gameDate.Date);
        }

        public async Task<IEnumerable<Game>> GetByFranchiseIdAsync(int franchiseId)
        {
            var homeGames = await _gameRepository.GetByHomeFranchiseIdAsync(franchiseId);
            var visitorGames = await _gameRepository.GetByVisitorFranchiseIdAsync(franchiseId);

            return homeGames.Concat(visitorGames).Distinct();
        }

        public async Task<Game> SaveAsync(Game entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await _gameRepository.CreateAsync(entity);
        }
    }
}
