using System;

namespace Retrosharp.Contract.Franchise
{
    /// <summary>
    /// Represents a baseball franchise (team).
    /// </summary>
    public class Franchise : Entity
    {
        /// <summary>
        /// Foreign key to the League.
        /// </summary>
        public int? LeagueId { get; set; }

        /// <summary>
        /// Franchise identifier code.
        /// </summary>
        public string FranchiseIdentifier { get; set; }

        /// <summary>
        /// Franchise code (typically 3 letters).
        /// </summary>
        public string FranchiseCode { get; set; }

        /// <summary>
        /// Division code (e.g., "AL", "NL"). Not populated for eras before divisional play.
        /// </summary>
        public string? DivisionCode { get; set; }

        /// <summary>
        /// Geographic location of the franchise.
        /// </summary>
        public string FranchiseLocation { get; set; }

        /// <summary>
        /// Primary team nickname.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Alternative team nickname if applicable.
        /// </summary>
        public string? AlternateNickname { get; set; }

        /// <summary>
        /// Date when the franchise was established.
        /// </summary>
        public DateTime FranchiseStart { get; set; }

        /// <summary>
        /// Date when the franchise ceased operations (if applicable).
        /// </summary>
        public DateTime? FranchiseEnd { get; set; }

        /// <summary>
        /// City where the team plays/played their home games.
        /// </summary>
        public string PlayingCity { get; set; }

        /// <summary>
        /// State abbreviation where the team plays/played.
        /// </summary>
        public string PlayingState { get; set; }

        /// <summary>
        /// Indicates if the franchise is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
