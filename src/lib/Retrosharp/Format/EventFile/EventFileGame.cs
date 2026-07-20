using System;
using System.Collections.Generic;

namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// One game's worth of records from a Retrosheet event file, in strict original file
    /// order (a "sub" between two "play" records must be applied before the next "play" is
    /// resolved -- see spec/phase-1-build-plan.md Step 6b). A game's boundaries are its
    /// enclosing "id" record; because the same physical game appears in both participating
    /// teams' files (spec/game-event.md, Considerations), <see cref="GameId"/> alone is not a
    /// database key -- resolving to an actual Game row uses <see cref="GameDate"/>,
    /// <see cref="GameNumber"/>, <see cref="HomeTeamCode"/>, and <see cref="VisitingTeamCode"/>.
    /// </summary>
    public sealed class EventFileGame
    {
        /// <summary>
        /// Retrosheet's own game identifier, for example "SDN202503270" -- the "id" record's
        /// value, kept for traceability but not used as a lookup key.
        /// </summary>
        public required string GameId { get; init; }

        public required string HomeTeamCode { get; init; }

        public required string VisitingTeamCode { get; init; }

        public required DateTime GameDate { get; init; }

        public required byte GameNumber { get; init; }

        public required IReadOnlyList<EventFileRecord> Records { get; init; }
    }
}
