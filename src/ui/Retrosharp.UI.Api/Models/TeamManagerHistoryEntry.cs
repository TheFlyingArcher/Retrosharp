namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One manager's tenure with a franchise for a stretch of a season -- a season with no
    /// mid-season change returns a single entry spanning the whole season; a manager fired and
    /// replaced mid-season returns one entry per manager, in chronological order. See
    /// spec/api.md, "GET /teams/{id}/managers".
    /// </summary>
    public class TeamManagerHistoryEntry
    {
        public PlayerSearchResult? Manager { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
