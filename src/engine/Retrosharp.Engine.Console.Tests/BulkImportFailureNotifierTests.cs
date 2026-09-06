using System.Text;
using System.Text.Json;

using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console.Tests
{
    public class BulkImportFailureNotifierTests
    {
        private const string GameEventStartType = "Retrosharp.Message.GameEvent.GameEventStart";

        [Fact]
        public void IsGameEventStart_MatchesBareTypeName() =>
            Assert.True(BulkImportFailureNotifier.IsGameEventStart(GameEventStartType));

        [Fact]
        public void IsGameEventStart_MatchesAssemblyQualifiedEntryInAList() =>
            Assert.True(BulkImportFailureNotifier.IsGameEventStart(
                $"{GameEventStartType}, Retrosharp, Version=1.0.0.0;Some.Other.Type, Other"));

        [Fact]
        public void IsGameEventStart_DoesNotMatchGameEventImportFailed() =>
            Assert.False(BulkImportFailureNotifier.IsGameEventStart(
                "Retrosharp.Message.GameEvent.GameEventImportFailed, Retrosharp"));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsGameEventStart_HandlesMissingHeader(string? value) =>
            Assert.False(BulkImportFailureNotifier.IsGameEventStart(value));

        [Fact]
        public void TryReadChildFields_ReadsBulkImportIdAndFilePath()
        {
            var bulkImportId = Guid.NewGuid();
            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new { RequestId = Guid.NewGuid(), FilePath = "/data/retrosheet/_bulk-import/x/2024SDN.EVN", BulkImportId = bulkImportId }));

            var ok = BulkImportFailureNotifier.TryReadChildFields(body, out var readId, out var filePath);

            Assert.True(ok);
            Assert.Equal(bulkImportId, readId);
            Assert.Equal("/data/retrosheet/_bulk-import/x/2024SDN.EVN", filePath);
        }

        [Fact]
        public void TryReadChildFields_StandaloneImport_YieldsEmptyGuid()
        {
            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new { RequestId = Guid.NewGuid(), FilePath = "2024SDN.EVN", BulkImportId = Guid.Empty }));

            var ok = BulkImportFailureNotifier.TryReadChildFields(body, out var readId, out _);

            Assert.True(ok);
            Assert.Equal(Guid.Empty, readId);
        }

        [Fact]
        public void TryReadChildFields_NonJsonBody_ReturnsFalse()
        {
            var ok = BulkImportFailureNotifier.TryReadChildFields(Encoding.UTF8.GetBytes("<xml/>"), out _, out _);

            Assert.False(ok);
        }
    }
}
