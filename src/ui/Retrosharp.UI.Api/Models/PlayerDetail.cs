namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Player identity/biographical detail. Statistics are served by separate endpoints
    /// (Step 7b onward), not attached here. See spec/api.md, "GET /players/{personId}".
    /// </summary>
    public class PlayerDetail
    {
        public int Id { get; set; }

        public string RetroSheetId { get; set; } = string.Empty;

        public string? Surname { get; set; }

        public string? UseName { get; set; }

        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? BirthCity { get; set; }

        public string? BirthStateProvince { get; set; }

        public string? BirthCountry { get; set; }

        public DateTime? DeathDate { get; set; }

        public string? DeathCity { get; set; }

        public string? DeathStateProvince { get; set; }

        public string? DeathCountry { get; set; }

        public string? Bats { get; set; }

        public string? Throws { get; set; }

        public float? Height { get; set; }

        public float? Weight { get; set; }

        public bool IsHof { get; set; }

        public DateTime? PlayerDebutDate { get; set; }

        public DateTime? PlayerLastDate { get; set; }

        public DateTime? ManagerDebutDate { get; set; }

        public DateTime? ManagerLastDate { get; set; }

        public DateTime? CoachDebutDate { get; set; }

        public DateTime? CoachLastDate { get; set; }

        public DateTime? UmpireDebutDate { get; set; }

        public DateTime? UmpireLastDate { get; set; }
    }
}
