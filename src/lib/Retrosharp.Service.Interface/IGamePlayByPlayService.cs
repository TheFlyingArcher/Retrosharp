using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Person;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for a single game's full play-by-play -- GameEvent interleaved with
    /// GameSubstitution/GameAdjustment/GameComment, in true chronological order. See spec/api.md,
    /// "GET /games/{gameId}/events".
    /// </summary>
    public interface IGamePlayByPlayService
    {
        /// <summary>
        /// Returns the game's chronologically-ordered play-by-play entries (identified only by
        /// PersonId, matching every other Contract-layer GameEvent type) plus a glossary of
        /// every distinct person referenced anywhere in them (batters, pitchers, runners,
        /// fielding-credit players, substitution/adjustment subjects) -- resolved once per
        /// distinct person rather than embedded redundantly in every play/runner/credit.
        /// </summary>
        Task<(IReadOnlyList<GamePlayByPlayEntry> Entries, IReadOnlyDictionary<int, Person> People)> GetPlayByPlayAsync(int gameId);
    }
}
