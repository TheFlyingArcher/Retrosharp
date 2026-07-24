namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Franchise identity detail. See spec/api.md, "GET /teams/{id}".
    /// </summary>
    public class TeamDetail
    {
        public int Id { get; set; }

        public int? LeagueId { get; set; }

        public string FranchiseIdentifier { get; set; } = string.Empty;

        public string FranchiseCode { get; set; } = string.Empty;

        public string? DivisionCode { get; set; }

        public string FranchiseLocation { get; set; } = string.Empty;

        public string Nickname { get; set; } = string.Empty;

        public string? AlternateNickname { get; set; }

        public DateTime FranchiseStart { get; set; }

        public DateTime? FranchiseEnd { get; set; }

        public string PlayingCity { get; set; } = string.Empty;

        public string PlayingState { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
