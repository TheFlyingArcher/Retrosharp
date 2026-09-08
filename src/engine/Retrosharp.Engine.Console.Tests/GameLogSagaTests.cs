using System.IO.Compression;

using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Configuration;
using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.GameLog;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Tests
{
    public sealed class GameLogSagaTests : IDisposable
    {
        private readonly string _workRoot =
            Path.Combine(Path.GetTempPath(), "retrosharp-gamelogsaga-tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { if (Directory.Exists(_workRoot)) Directory.Delete(_workRoot, recursive: true); }
            catch { /* best effort */ }
        }

        private GameLogSaga CreateSaga(FakeGameLogImportService importService, FakeRetrosheetArchiveClient archiveClient) =>
            new(NullLogger<GameLogSaga>.Instance, importService, archiveClient,
                new RetrosheetSourceConfiguration { WorkingRoot = _workRoot })
            {
                Data = new GameLogSagaData()
            };

        /// <summary>A download fake that writes a real gl{season}.zip containing GL{season}.TXT.</summary>
        private static FakeRetrosheetArchiveClient DownloadsRealArchive(int season, string gameLogContent = "20240328,0,Thu,...\n") =>
            new()
            {
                OnDownload = (url, dir) =>
                {
                    var zipPath = Path.Combine(dir, Path.GetFileName(url.LocalPath));
                    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                    using var writer = new StreamWriter(zip.CreateEntry($"GL{season}.TXT").Open());
                    writer.Write(gameLogContent);
                    return zipPath;
                }
            };

        [Fact]
        public async Task Handle_Start_DownloadsExtractsImports_SendsCompleteAndCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = DownloadsRealArchive(2025);
            var importService = new FakeGameLogImportService
            {
                ResultToReturn = new GameLogImportResult { GamesAdded = 12, GamesSkipped = 3 }
            };
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogStart { RequestId = requestId, SeasonYear = 2025 }, context);

            Assert.Equal("https://www.retrosheet.org/gamelogs/gl2025.zip", archiveClient.LastUrl!.ToString());
            Assert.True(importService.WasCalled);
            Assert.Equal(2025, importService.LastSeasonYear);
            Assert.Equal("GL2025.TXT", Path.GetFileName(importService.LastFilePath));

            var sent = Assert.Single(context.SentMessages);
            var complete = Assert.IsType<GameLogComplete>(sent.Message);
            Assert.Equal(requestId, complete.RequestId);
            Assert.Equal(2025, complete.SeasonYear);
            Assert.Equal(12, complete.GamesAdded);
            Assert.Equal(3, complete.GamesSkipped);
            Assert.False(saga.Completed);

            Assert.False(Directory.Exists(Path.Combine(_workRoot, "gamelog", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_ArchiveNotFound_PropagatesWithoutImportingAndCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                ExceptionToThrow = new RetrosheetArchiveNotFoundException("HTTP 404 for gl2099.zip")
            };
            var importService = new FakeGameLogImportService();
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<RetrosheetArchiveNotFoundException>(() =>
                saga.Handle(new GameLogStart { RequestId = requestId, SeasonYear = 2099 }, context));

            Assert.False(importService.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
            Assert.False(Directory.Exists(Path.Combine(_workRoot, "gamelog", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_ArchiveUnavailable_PropagatesForRetry()
        {
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                ExceptionToThrow = new RetrosheetArchiveUnavailableException("HTTP 503")
            };
            var saga = CreateSaga(new FakeGameLogImportService(), archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(() =>
                saga.Handle(new GameLogStart { RequestId = Guid.NewGuid(), SeasonYear = 2025 }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_ImportThrows_PropagatesAndStillCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = DownloadsRealArchive(2025);
            var importService = new FakeGameLogImportService
            {
                ExceptionToThrow = new InvalidOperationException("No franchise found for code 'XXX'.")
            };
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                saga.Handle(new GameLogStart { RequestId = requestId, SeasonYear = 2025 }, context));

            Assert.True(importService.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
            Assert.False(Directory.Exists(Path.Combine(_workRoot, "gamelog", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_CorruptArchive_PropagatesInvalidData()
        {
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                OnDownload = (url, dir) =>
                {
                    // A "zip" with no game-log entry -> GameLogArchive.Extract throws.
                    var zipPath = Path.Combine(dir, Path.GetFileName(url.LocalPath));
                    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                    using var writer = new StreamWriter(zip.CreateEntry("readme.txt").Open());
                    writer.Write("not a game log");
                    return zipPath;
                }
            };
            var saga = CreateSaga(new FakeGameLogImportService(), archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                saga.Handle(new GameLogStart { RequestId = Guid.NewGuid(), SeasonYear = 2025 }, context));

            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Complete_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameLogImportService(), new FakeRetrosheetArchiveClient());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogComplete { RequestId = Guid.NewGuid(), SeasonYear = 2025, GamesAdded = 1, GamesSkipped = 0 }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_Cancel_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameLogImportService(), new FakeRetrosheetArchiveClient());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogCancel { RequestId = Guid.NewGuid() }, context);

            Assert.True(saga.Completed);
        }
    }
}
