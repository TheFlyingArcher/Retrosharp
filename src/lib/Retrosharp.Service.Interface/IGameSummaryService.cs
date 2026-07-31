using Retrosharp.Contract.Game;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for a single game's summary (score, box score, lineups, decisions,
    /// umpires, ballpark). See spec/api.md, "GET /games/{gameId}".
    /// </summary>
    public interface IGameSummaryService
    {
        Task<GameSummary> GetSummaryAsync(int gameId);
    }
}
