using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Resolves FIP's league-normalizing constant for one league-season. Shared between player
    /// statistics (Step 7c) and team statistics (Step 7e), which both need the exact same
    /// resolution rather than two copies. Stateless -- callers that resolve the constant for
    /// many rows in one request (a player's whole career, spanning many league-seasons) should
    /// keep their own cache, the same way <c>PlayerStatisticsService.GetPitchingAsync</c> already
    /// does, rather than re-resolving the same league-season repeatedly.
    /// </summary>
    public interface IFipConstantResolver
    {
        Task<FipConstantResult> ResolveAsync(int leagueId, short season);
    }
}
