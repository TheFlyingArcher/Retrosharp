namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// A franchise's season statistics. Reuses the same <see cref="BattingLine"/>/
    /// <see cref="PitchingLine"/>/<see cref="FieldingLine"/> DTOs a player's stats use -- a
    /// team-season line is the same shape, just without the null-when-combined
    /// franchise/season fields (populated normally here, since there's exactly one row).
    /// See spec/api.md, "GET /teams/{id}/stats".
    /// </summary>
    public class TeamSeasonStatsResponse
    {
        public BattingLine? Batting { get; set; }

        public PitchingLine? Pitching { get; set; }

        public FieldingLine? Fielding { get; set; }

        /// <summary>
        /// Null if standings haven't been computed for this season yet -- see
        /// <c>POST /api/standings/compute</c>.
        /// </summary>
        public FranchiseStandingLine? Standing { get; set; }
    }
}
