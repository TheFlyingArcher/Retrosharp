namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One game search result row. See spec/api.md, "GET /games/search".
    /// </summary>
    public class GameSearchResult
    {
        public int Id { get; set; }

        public DateTime GameDate { get; set; }

        /// <summary>
        /// 0 for a single game, 1/2 for the first/second game of a doubleheader.
        /// </summary>
        public byte GameNumber { get; set; }

        public int HomeFranchiseId { get; set; }

        public string HomeFranchiseCode { get; set; } = string.Empty;

        public int VisitorFranchiseId { get; set; }

        public string VisitorFranchiseCode { get; set; } = string.Empty;

        public byte HomeTeamRuns { get; set; }

        public byte VisitorRuns { get; set; }
    }
}
