using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Pitching;
using Retrosharp.Contract.Fielding;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for calculating and aggregating player statistics.
    /// </summary>
    public interface IPlayerStatisticsService
    {
        /// <summary>
        /// Gets batting statistics for a player, optionally filtered by season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="seasonYear">Optional season year filter.</param>
        /// <returns>A collection of batting statistics.</returns>
        Task<IEnumerable<Batting>> GetBattingStatsAsync(int personId, short? seasonYear = null);

        /// <summary>
        /// Gets pitching statistics for a player, optionally filtered by season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="seasonYear">Optional season year filter.</param>
        /// <returns>A collection of pitching statistics.</returns>
        Task<IEnumerable<Pitching>> GetPitchingStatsAsync(int personId, short? seasonYear = null);

        /// <summary>
        /// Gets fielding statistics for a player, optionally filtered by season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="seasonYear">Optional season year filter.</param>
        /// <returns>A collection of fielding statistics.</returns>
        Task<IEnumerable<Fielding>> GetFieldingStatsAsync(int personId, short? seasonYear = null);

        /// <summary>
        /// Calculates career batting statistics by aggregating all seasons.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <returns>Aggregated career batting statistics.</returns>
        Task<BattingStatistics> CalculateCareerBattingStatsAsync(int personId);
    }
}
