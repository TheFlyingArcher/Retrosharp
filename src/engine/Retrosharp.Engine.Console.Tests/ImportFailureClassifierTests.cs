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
    }
}
