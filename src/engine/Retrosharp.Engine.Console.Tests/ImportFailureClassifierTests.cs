using Npgsql;

using Retrosharp.Engine.Console.Saga;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Engine.Console.Tests
{
    public class ImportFailureClassifierTests
    {
        [Theory]
        [InlineData(typeof(FileNotFoundException))]
        [InlineData(typeof(DirectoryNotFoundException))]
        [InlineData(typeof(InvalidOperationException))]
        public void IsUnrecoverable_KnownUnrecoverableTypes_ReturnsTrue(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            Assert.True(ImportFailureClassifier.IsUnrecoverable(exception));
        }

        [Fact]
        public void IsUnrecoverable_PlayCodeParseException_ReturnsTrue()
        {
            // No parameterless constructor, so this can't join the Theory above.
            var exception = new PlayCodeParseException("1/BL1S", "Fielded-out code has no trajectory modifier.");

            Assert.True(ImportFailureClassifier.IsUnrecoverable(exception));
        }

        [Theory]
        [InlineData(typeof(TimeoutException))]
        [InlineData(typeof(IOException))]
        public void IsUnrecoverable_TransientExceptionTypes_ReturnsFalse(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            Assert.False(ImportFailureClassifier.IsUnrecoverable(exception));
        }

        [Fact]
        public void IsUnrecoverable_PostgresDeadlock_ReturnsFalse()
        {
            // 40P01 = deadlock_detected. NpgsqlException.IsTransient is true for it.
            var deadlock = new PostgresException("deadlock detected", "ERROR", "ERROR", "40P01");

            Assert.False(ImportFailureClassifier.IsUnrecoverable(deadlock));
        }

        [Fact]
        public void IsUnrecoverable_PostgresDeadlockWrappedInInvalidOperationException_ReturnsFalse()
        {
            // The real shape seen under concurrent Game Event imports: EF Core / Npgsql wrap
            // the deadlock in an InvalidOperationException ("...likely due to a transient
            // failure"). The blanket InvalidOperationException rule must not win here.
            var wrapped = new InvalidOperationException(
                "An exception has been raised that is likely due to a transient failure.",
                new PostgresException("deadlock detected", "ERROR", "ERROR", "40P01"));

            Assert.False(ImportFailureClassifier.IsUnrecoverable(wrapped));
        }

        [Fact]
        public void IsUnrecoverable_NonTransientPostgresErrorWrappedInInvalidOperationException_ReturnsTrue()
        {
            // A non-transient DB error (23503 = foreign_key_violation, IsTransient false)
            // wrapped in InvalidOperationException stays unrecoverable -- retrying can't fix a
            // missing FK target, same as the bare-InvalidOperationException resolution failures.
            var wrapped = new InvalidOperationException(
                "insert or update violates foreign key constraint",
                new PostgresException("violates foreign key constraint", "ERROR", "ERROR", "23503"));

            Assert.True(ImportFailureClassifier.IsUnrecoverable(wrapped));
        }
    }
}
