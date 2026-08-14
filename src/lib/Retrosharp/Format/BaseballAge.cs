using System;

namespace Retrosharp.Format
{
    /// <summary>
    /// Computes a player's "baseball age" for a season -- their age as of June 30 of that
    /// season, the standard Baseball-Reference convention (chosen because June 30 is roughly the
    /// midpoint of a 162-game season, so it's the one date that best represents how old a player
    /// was for most of it). A player's baseball age is fixed for the whole season regardless of
    /// when their actual birthday falls within it. See spec/frontend-prototype.md's "Resolved:
    /// 'Average Age' as of June 30" note.
    /// </summary>
    public static class BaseballAge
    {
        /// <returns>
        /// The player's age as of June 30 of <paramref name="seasonYear"/>, or null if
        /// <paramref name="birthDate"/> is null (incomplete biographical data is tolerated, not
        /// treated as an error -- the same convention person.md already establishes).
        /// </returns>
        public static int? ComputeAge(DateTime? birthDate, short seasonYear)
        {
            if (birthDate is not { } birth)
                return null;

            var asOf = new DateTime(seasonYear, 6, 30);
            var age = asOf.Year - birth.Year;

            if (birth.Month > asOf.Month || (birth.Month == asOf.Month && birth.Day > asOf.Day))
                age--;

            return age;
        }
    }
}
