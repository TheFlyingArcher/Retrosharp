namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One team search result row. See spec/api.md, "GET /teams/search".
    /// </summary>
    public class TeamSearchResult
    {
        public int Id { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string PlayingCity { get; set; } = string.Empty;

        public string Nickname { get; set; } = string.Empty;

        public string? DivisionCode { get; set; }

        public bool IsActive { get; set; }

        public DateTime FranchiseStart { get; set; }

        public DateTime? FranchiseEnd { get; set; }
    }
}
