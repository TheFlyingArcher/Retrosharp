using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Game;
using Retrosharp.Data;
using Retrosharp.Format;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IBallparkRepository _ballparkRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IGameRepository gameRepository,
            IFranchiseRepository franchiseRepository,
            IPersonRepository personRepository,
            IBallparkRepository ballparkRepository,
            ILogger<GameService> logger)
        {
            _gameRepository = gameRepository;
            _franchiseRepository = franchiseRepository;
            _personRepository = personRepository;
            _ballparkRepository = ballparkRepository;
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

        public async Task<int> ProcessGameLogsAsync(IEnumerable<GameLog> gameLogs, int seasonYear)
        {
            if (gameLogs == null || !gameLogs.Any())
                return 0;

            int processedCount = 0;

            foreach (var gameLog in gameLogs)
            {
                try
                {
                    // Transform GameLog to Game entity
                    // This is a simplified version - you'd need to map all fields properly
                    // and handle lookups for franchises, persons (managers, umpires), ballparks

                    var game = new Game
                    {
                        GameDate = gameLog.GameDate,
                        GameNumber = byte.Parse(gameLog.GameNumber.ToString()),
                        GameWeekDay = gameLog.DayOfWeek,
                        GameDayNight = gameLog.DayOrNight.ToString(),
                        VisitorRuns = (byte)gameLog.VisitorScore,
                        HomeTeamRuns = (byte)gameLog.HomeScore,
                        GameLengthMinutes = (short?)gameLog.GameLengthMinutes,
                        ParkAttendance = gameLog.GameAttendance
                        // Note: Foreign key IDs would need to be resolved by looking up 
                        // franchises, people, and ballparks by their codes
                    };

                    // In a real implementation, you would:
                    // 1. Look up franchise IDs by team codes
                    // 2. Look up manager IDs by their retrosheet IDs
                    // 3. Look up ballpark ID by site code
                    // 4. Look up umpire IDs
                    // 5. Check if the game already exists (idempotency)

                    _logger.LogInformation("Processed game from {GameDate}", gameLog.GameDate);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing game log for date {GameDate}", gameLog.GameDate);
                }
            }

            return processedCount;
        }

        public async Task<Game> SaveAsync(Game entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await _gameRepository.CreateAsync(entity);
        }
    }
}
