using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Derives a player's per-game batting/pitching lines on demand from their games' play-by-play.
    /// See spec/api.md, "GET /players/{id}/games".
    /// </summary>
    public interface IPlayerGameLogService
    {
        /// <summary>
        /// Gets a player's batting line for every game they batted in, for one season or their
        /// whole career when <paramref name="season"/> is null.
        /// </summary>
        Task<IEnumerable<PlayerGameBattingLine>> GetBattingGameLogAsync(int personId, short? season);

        /// <summary>
        /// Gets a player's pitching line for every game they pitched in, for one season or their
        /// whole career when <paramref name="season"/> is null.
        /// </summary>
        Task<IEnumerable<PlayerGamePitchingLine>> GetPitchingGameLogAsync(int personId, short? season);
    }
}
