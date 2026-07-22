namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One row of a player search result. See spec/api.md, "GET /players/search".
    /// </summary>
    public class PlayerSearchResult
    {
        public int Id { get; set; }

        public string RetroSheetId { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? UseName { get; set; }

        public string? Bats { get; set; }

        public string? Throws { get; set; }

        public DateTime? PlayerDebutDate { get; set; }

        public DateTime? PlayerLastDate { get; set; }

        public bool IsHof { get; set; }
    }
}
