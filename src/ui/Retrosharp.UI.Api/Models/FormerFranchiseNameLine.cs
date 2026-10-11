namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One former name of a franchise lineage with its span and record under that name. Backs
    /// the Teams page's "Former names" table. See spec/former-franchise-names.md.
    /// </summary>
    public class FormerFranchiseNameLine
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public short FirstSeasonYear { get; set; }

        public short LastSeasonYear { get; set; }

        public int CurrentFranchiseId { get; set; }

        public string CurrentName { get; set; } = string.Empty;

        public short GamesPlayed { get; set; }

        public short Wins { get; set; }

        public short Losses { get; set; }

        public short Ties { get; set; }

        public float WinPercentage { get; set; }
    }
}
