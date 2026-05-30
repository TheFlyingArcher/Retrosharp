using System;

namespace Retrosharp.Contract.League
{
    /// <summary>
    /// Represents a baseball league (e.g., American League, National League).
    /// </summary>
    public class League : Entity
    {
        /// <summary>
        /// League code (e.g., "AL", "NL").
        /// </summary>
        public string LeagueCode { get; set; }

        /// <summary>
        /// Full name of the league.
        /// </summary>
        public string LeagueName { get; set; }
    }
}
