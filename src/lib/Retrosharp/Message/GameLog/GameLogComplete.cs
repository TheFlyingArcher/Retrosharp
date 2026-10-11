using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Message.GameLog
{
    public class GameLogComplete :BaseMessage, IMessage
    {
        public GameLogComplete() { }

        /// <summary>
        /// The baseball season in which the game log was processed.
        /// </summary>
        public int SeasonYear { get; set; }

        /// <summary>
        /// Number of games added to the database.
        /// </summary>
        public int GamesAdded { get; set; }

        /// <summary>
        /// Number of games skipped because they were already present, matched by natural key.
        /// </summary>
        public int GamesSkipped { get; set; }

        /// <summary>
        /// True when the season's precomputed standings were recomputed after the import.
        /// False means the import succeeded but the recompute failed (see
        /// <see cref="StandingsError"/>); standings can be rebuilt via
        /// <c>POST /api/standings/compute?season=</c>.
        /// </summary>
        public bool StandingsRecomputed { get; set; }

        /// <summary>
        /// Number of franchises a standing was computed for. Zero when
        /// <see cref="StandingsRecomputed"/> is false.
        /// </summary>
        public int StandingsFranchiseCount { get; set; }

        /// <summary>
        /// Why the standings recompute failed, or null when it succeeded.
        /// </summary>
        public string? StandingsError { get; set; }
    }
}
