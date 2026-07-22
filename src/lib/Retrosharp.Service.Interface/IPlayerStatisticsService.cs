using Retrosharp.Contract;
using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Pitching;
using Retrosharp.Contract.Fielding;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for calculating and aggregating player statistics.
    /// See spec/api.md, "GET /players/{id}/batting|pitching|fielding".
    /// </summary>
    public interface IPlayerStatisticsService
    {
        /// <summary>
        /// Gets a player's batting statistics for one season, or their whole career when
        /// <paramref name="season"/> is null, including a combined total when more than one
        /// franchise row applies.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="season">The season year to scope to, or null for the player's career.</param>
        Task<PlayerStatLines<BattingStatistics>> GetBattingAsync(int personId, short? season);

        /// <summary>
        /// Gets a player's pitching statistics for one season, or their whole career when
        /// <paramref name="season"/> is null, including a combined total when more than one
        /// franchise row applies.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="season">The season year to scope to, or null for the player's career.</param>
        Task<PlayerStatLines<PitchingStatistics>> GetPitchingAsync(int personId, short? season);

        /// <summary>
        /// Gets a player's fielding statistics for one season, or their whole career when
        /// <paramref name="season"/> is null, including a combined total when more than one
        /// (franchise, position) row applies.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="season">The season year to scope to, or null for the player's career.</param>
        Task<PlayerStatLines<FieldingStatistics>> GetFieldingAsync(int personId, short? season);
    }
}
