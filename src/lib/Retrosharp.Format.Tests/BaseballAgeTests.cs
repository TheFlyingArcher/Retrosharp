namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="BaseballAge.ComputeAge"/> against the Baseball-Reference "age as of
    /// June 30" convention. See spec/frontend-prototype.md's "Resolved: 'Average Age' as of
    /// June 30" note.
    /// </summary>
    public class BaseballAgeTests
    {
        [Fact]
        public void ComputeAge_BirthdayBeforeJune30_HasAlreadyTurnedAgeByCutoff()
        {
            // Born June 1, 2000 -- turns 25 on June 1, 2025, before the June 30 cutoff.
            var age = BaseballAge.ComputeAge(new DateTime(2000, 6, 1), 2025);

            Assert.Equal(25, age);
        }

        [Fact]
        public void ComputeAge_BirthdayAfterJune30_HasNotYetTurnedAgeByCutoff()
        {
            // Born July 1, 2000 -- doesn't turn 25 until July 1, 2025, after the June 30 cutoff,
            // so their baseball age for 2025 is still 24.
            var age = BaseballAge.ComputeAge(new DateTime(2000, 7, 1), 2025);

            Assert.Equal(24, age);
        }

        [Fact]
        public void ComputeAge_BirthdayExactlyOnJune30_HasAlreadyTurnedAgeByCutoff()
        {
            // Born June 30 -- turns that age ON the cutoff itself, which counts as "by" June 30.
            var age = BaseballAge.ComputeAge(new DateTime(2000, 6, 30), 2025);

            Assert.Equal(25, age);
        }

        [Fact]
        public void ComputeAge_SameMonthDifferentDayBeforeCutoff_HasAlreadyTurnedAge()
        {
            var age = BaseballAge.ComputeAge(new DateTime(2000, 6, 29), 2025);

            Assert.Equal(25, age);
        }

        [Fact]
        public void ComputeAge_SameMonthDifferentDayAfterCutoff_HasNotYetTurnedAge()
        {
            var age = BaseballAge.ComputeAge(new DateTime(2000, 7, 1), 2025);

            Assert.Equal(24, age);
        }

        [Fact]
        public void ComputeAge_NullBirthDate_ReturnsNull()
        {
            var age = BaseballAge.ComputeAge(null, 2025);

            Assert.Null(age);
        }

        [Fact]
        public void ComputeAge_LeapYearBirthdate_DoesNotThrow()
        {
            // February 29 birthdate against a non-leap season year -- the reference date itself
            // (June 30) is never affected by leap years, so this should never throw regardless.
            var age = BaseballAge.ComputeAge(new DateTime(2000, 2, 29), 2025);

            Assert.Equal(25, age);
        }
    }
}
