using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console.Tests
{
    public class ImportFailureClassifierTests
    {
        [Theory]
        [InlineData(typeof(FileNotFoundException))]
        [InlineData(typeof(DirectoryNotFoundException))]
        public void IsUnrecoverable_FileOrDirectoryNotFound_ReturnsTrue(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            Assert.True(ImportFailureClassifier.IsUnrecoverable(exception));
        }

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(TimeoutException))]
        [InlineData(typeof(IOException))]
        public void IsUnrecoverable_OtherExceptionTypes_ReturnsFalse(Type exceptionType)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            Assert.False(ImportFailureClassifier.IsUnrecoverable(exception));
        }
    }
}
