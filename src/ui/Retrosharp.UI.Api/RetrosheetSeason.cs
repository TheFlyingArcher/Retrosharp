namespace Retrosharp.UI.Api
{
    /// <summary>
    /// Plausible-range check for a season year supplied to an ETL endpoint. Retrosheet's
    /// earliest data is 1871; the upper bound allows the current season plus one for
    /// schedules published early. Shared by GameLogController and GameEventController. See
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    internal static class RetrosheetSeason
    {
        public const int Earliest = 1871;

        public static int Latest => DateTime.UtcNow.Year + 1;

        public static bool IsPlausible(int seasonYear) => seasonYear >= Earliest && seasonYear <= Latest;

        public static string RangeMessage => $"seasonYear must be between {Earliest} and {Latest}.";
    }
}
