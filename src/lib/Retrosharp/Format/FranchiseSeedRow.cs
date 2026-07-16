using System;

namespace Retrosharp.Format
{
    /// <summary>
    /// Represents one row of Retrosheet's franchise reference data
    /// (docs/csv/franchises.csv, sourced from Retrosheet's CurrentNames.csv). Each row is one
    /// historical era of a franchise — the same <see cref="FranchiseIdentifier"/> can appear
    /// across multiple rows as a team relocates or renames. See spec/seed-data.md.
    /// </summary>
    public class FranchiseSeedRow
    {
        public string FranchiseIdentifier { get; set; }

        public string FranchiseCode { get; set; }

        public string LeagueCode { get; set; }

        public string? DivisionCode { get; set; }

        public string FranchiseLocation { get; set; }

        public string Nickname { get; set; }

        public string? AlternateNickname { get; set; }

        public DateTime FranchiseStart { get; set; }

        public DateTime? FranchiseEnd { get; set; }

        public string PlayingCity { get; set; }

        public string PlayingState { get; set; }
    }
}
