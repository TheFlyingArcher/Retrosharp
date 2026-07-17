using System.Globalization;

namespace Retrosharp.Format
{
    /// <summary>
    /// Parses Retrosheet's raw "yyyymmdd" date fields, applying the normalization rules from
    /// spec/person.md: day "00" becomes "01", month "00" becomes "01", and a missing/unparseable
    /// value becomes null. Shared across every date field in the biofile (birth, death, and
    /// debut/last dates for player, coach, manager, and umpire roles) to avoid the kind of
    /// copy-pasted-per-field bug that previously misread one field from the wrong column.
    /// </summary>
    public static class RetrosheetDateParser
    {
        public static DateTime? Parse(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw.Length != 8)
                return null;

            var year = raw.Substring(0, 4);
            var month = raw.Substring(4, 2);
            var day = raw.Substring(6, 2);

            if (month == "00")
                month = "01";

            if (day == "00")
                day = "01";

            return DateTime.TryParseExact(
                year + month + day,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result)
                ? result
                : null;
        }
    }
}
