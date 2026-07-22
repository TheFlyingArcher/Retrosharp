namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One game's batting line for a player. See spec/api.md, "GET /players/{id}/games".
    /// </summary>
    public class GameBattingLine
    {
        public int GameId { get; set; }

        public DateTime GameDate { get; set; }

        public bool IsHome { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string OpponentFranchiseCode { get; set; } = string.Empty;

        public short PlateAppearances { get; set; }

        public short AtBats { get; set; }

        public short Hits { get; set; }

        public short Doubles { get; set; }

        public short Triples { get; set; }

        public short Homeruns { get; set; }

        public short BaseOnBalls { get; set; }

        public short Strikeouts { get; set; }

        public short SacrificeFlies { get; set; }

        public short SacrificeBunts { get; set; }

        public short IntentionalBb { get; set; }

        public short HitByPitches { get; set; }

        public short StolenBases { get; set; }

        public short TimesCaughtStealing { get; set; }

        public short Runs { get; set; }

        public short GroundedIntoDoublePlay { get; set; }
    }
}
