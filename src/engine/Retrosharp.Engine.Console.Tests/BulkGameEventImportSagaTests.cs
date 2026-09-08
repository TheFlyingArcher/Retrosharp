using System.IO.Compression;

using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Configuration;
using Retrosharp.Contract.BulkImport;
using Retrosharp.Contract.Game;
using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Tests
{
    public sealed class BulkGameEventImportSagaTests : IDisposable
    {
        // Sorted, so the first N dispatched files are predictable.
        private static readonly string[] FourFiles = { "2024ARI.EVN", "2024LAN.EVN", "2024SDN.EVN", "2024SEA.EVA" };

        private const string EventArchiveUrl2024 = "https://www.retrosheet.org/events/2024eve.zip";

        private readonly string _tempRoot;
        private readonly string _workingRoot;

        public BulkGameEventImportSagaTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "retrosharp-bulk-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            _workingRoot = Path.Combine(_tempRoot, "work");
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* best effort */ }
        }

        // --- helpers ---

        /// <summary>Writes a zip (outside any working dir) the fake download client copies in.</summary>
        private string BuildSeasonZip(params string[] entryNames)
        {
            var zipPath = Path.Combine(_tempRoot, $"source-{Guid.NewGuid():N}.zip");
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (var entryName in entryNames)
            {
                using var stream = zip.CreateEntry(entryName).Open();
                using var writer = new StreamWriter(stream);
                writer.Write($"id,{entryName}\n");
            }
            return zipPath;
        }

        /// <summary>
        /// A download fake that copies <paramref name="sourceZip"/> (default: a zip of
        /// <see cref="FourFiles"/>) into the run's working directory, or throws
        /// <paramref name="throws"/>.
        /// </summary>
        private FakeRetrosheetArchiveClient ArchiveClient(string? sourceZip = null, Exception? throws = null)
        {
            if (throws is not null)
                return new FakeRetrosheetArchiveClient { ExceptionToThrow = throws };

            var zip = sourceZip ?? BuildSeasonZip(FourFiles);
            return new FakeRetrosheetArchiveClient
            {
                OnDownload = (url, dir) =>
                {
                    var dest = Path.Combine(dir, Path.GetFileName(url.LocalPath));
                    File.Copy(zip, dest, overwrite: true);
                    return dest;
                }
            };
        }

        private BulkGameEventImportSaga CreateSaga(
            FakeBulkImportRepository bulkRepo,
            FakeGameRepository gameRepo,
            FakeRetrosheetArchiveClient archiveClient,
            int batchSize = 2)
            => new(
                NullLogger<BulkGameEventImportSaga>.Instance,
                bulkRepo,
                gameRepo,
                archiveClient,
                new BulkImportConfiguration { DefaultBatchSize = batchSize, WatchdogTimeoutHours = 1 },
                new RetrosheetSourceConfiguration { WorkingRoot = _workingRoot })
            {
                Data = new BulkGameEventImportSagaData()
            };

        private static FakeGameRepository GameLogImported() => new() { GamesBySeason = { new Game() } };

        private static BulkGameEventImportStart StartFor(Guid trackingId, int? season = 2024, int? batchSize = null) =>
            new() { RequestId = trackingId, BulkImportId = trackingId, SeasonYear = season, BatchSize = batchSize };

        private static IReadOnlyList<GameEventStart> SentStarts(TestableMessageHandlerContext context) =>
            context.SentMessages.Select(m => m.Message).OfType<GameEventStart>().ToList();

        // --- startup validation ---

        [Fact]
        public async Task Start_SeasonYearMissing_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var archiveClient = ArchiveClient();
            var saga = CreateSaga(bulkRepo, GameLogImported(), archiveClient);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), season: null), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("season year", bulkRepo.Run.FailureReason);
            Assert.False(archiveClient.WasCalled);
            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Start_GameLogNotImported_MarksRunFailedAndDispatchesNothing()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var trackingId = Guid.NewGuid();
            var saga = CreateSaga(bulkRepo, new FakeGameRepository(), ArchiveClient()); // GamesBySeason empty
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(trackingId), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("Game Log for season 2024", bulkRepo.Run.FailureReason);
            Assert.Empty(SentStarts(context));
            Assert.Empty(context.TimeoutMessages);
            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Start_ArchiveNotFound_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported(),
                ArchiveClient(throws: new RetrosheetArchiveNotFoundException("HTTP 404 for 2024eve.zip")));
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("could not be downloaded", bulkRepo.Run.FailureReason);
            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Start_DownloadedArchiveNotAZip_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported(),
                ArchiveClient(throws: new InvalidDataException("unexpected header bytes")));
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("not a valid zip", bulkRepo.Run.FailureReason);
        }

        [Fact]
        public async Task Start_DownloadUnavailable_PropagatesForRetryWithoutCreatingRun()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported(),
                ArchiveClient(throws: new RetrosheetArchiveUnavailableException("HTTP 503")));
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(() =>
                saga.Handle(StartFor(Guid.NewGuid()), context));

            Assert.Null(bulkRepo.Run);
            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Start_MultiSeasonArchive_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported(),
                ArchiveClient(sourceZip: BuildSeasonZip("2024SDN.EVN", "2023ARI.EVN")));
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("multiple seasons", bulkRepo.Run.FailureReason);
        }

        [Fact]
        public async Task Start_RequestedSeasonMismatch_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            // Request 2023, but the (mock) host serves an archive of 2024 files.
            var saga = CreateSaga(bulkRepo, GameLogImported(), ArchiveClient(sourceZip: BuildSeasonZip(FourFiles)));
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), season: 2023), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("2023", bulkRepo.Run.FailureReason);
            Assert.Contains("2024", bulkRepo.Run.FailureReason);
        }

        [Fact]
        public async Task Start_WhenAlreadyProcessing_IgnoresDuplicate()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var archiveClient = ArchiveClient();
            var saga = CreateSaga(bulkRepo, GameLogImported(), archiveClient);
            saga.Data.ProcessingStarted = true;
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Null(bulkRepo.Run);
            Assert.False(archiveClient.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
        }

        // --- happy path / dispatch window ---

        [Fact]
        public async Task Start_HappyPath_DownloadsSeasonArchiveSeedsExtractsAndDispatchesUpToBatchSize()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var trackingId = Guid.NewGuid();
            var archiveClient = ArchiveClient();
            var saga = CreateSaga(bulkRepo, GameLogImported(), archiveClient, batchSize: 2);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(trackingId), context);

            Assert.Equal(EventArchiveUrl2024, archiveClient.LastUrl!.ToString());
            Assert.Equal(EventArchiveUrl2024, bulkRepo.Run!.SourceZipPath);
            Assert.Equal(BulkImportStatus.InProgress, bulkRepo.Run.Status);
            Assert.Equal(4, bulkRepo.Run.Files.Count);

            var starts = SentStarts(context);
            Assert.Equal(2, starts.Count);
            Assert.All(starts, s => Assert.Equal(trackingId, s.BulkImportId));
            Assert.All(starts, s => Assert.True(File.Exists(s.FilePath), $"expected extracted file at {s.FilePath}"));
            Assert.Equal(new[] { "2024ARI.EVN", "2024LAN.EVN" }, starts.Select(s => Path.GetFileName(s.FilePath)).OrderBy(n => n));

            Assert.Equal(2, bulkRepo.Run.Files.Count(f => f.Status == BulkImportFileStatus.InProgress));
            Assert.Equal(2, bulkRepo.Run.Files.Count(f => f.Status == BulkImportFileStatus.Pending));
            Assert.Single(context.TimeoutMessages);
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task Start_RerunSkipsFilesWhoseMostRecentOutcomeIsSuccessOrSkipped()
        {
            var bulkRepo = new FakeBulkImportRepository();
            bulkRepo.PriorOutcomes[(2024, "2024ARI.EVN")] = BulkImportFileStatus.Success;
            bulkRepo.PriorOutcomes[(2024, "2024LAN.EVN")] = BulkImportFileStatus.Skipped; // already imported on an earlier rerun => still skip
            bulkRepo.PriorOutcomes[(2024, "2024SDN.EVN")] = BulkImportFileStatus.Failed;  // failed => reprocess
            // 2024SEA.EVA has no prior row => reprocess
            var saga = CreateSaga(bulkRepo, GameLogImported(), ArchiveClient(), batchSize: 10);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Equal(BulkImportFileStatus.Skipped, bulkRepo.FileNamed("2024ARI.EVN").Status);
            Assert.Equal(BulkImportFileStatus.Skipped, bulkRepo.FileNamed("2024LAN.EVN").Status);
            Assert.False(File.Exists(Path.Combine(saga.Data.WorkingDirectory, "2024ARI.EVN")));
            Assert.False(File.Exists(Path.Combine(saga.Data.WorkingDirectory, "2024LAN.EVN")));
            var dispatched = SentStarts(context).Select(s => Path.GetFileName(s.FilePath)).ToList();
            Assert.Equal(new[] { "2024SDN.EVN", "2024SEA.EVA" }, dispatched.OrderBy(n => n));
        }

        [Fact]
        public async Task Start_AllFilesAlreadySucceeded_CompletesImmediatelyWithNoDispatchOrTimeout_AndCleansUp()
        {
            var bulkRepo = new FakeBulkImportRepository();
            foreach (var f in FourFiles)
                bulkRepo.PriorOutcomes[(2024, f)] = BulkImportFileStatus.Success;
            var saga = CreateSaga(bulkRepo, GameLogImported(), ArchiveClient());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid()), context);

            Assert.Empty(context.SentMessages);
            Assert.Empty(context.TimeoutMessages);
            Assert.Equal(BulkImportStatus.Completed, bulkRepo.Run!.Status);
            Assert.True(saga.Completed);
            Assert.False(Directory.Exists(saga.Data.WorkingDirectory)); // downloaded zip removed, dir emptied
        }

        // --- resolution / completion ---

        [Fact]
        public async Task GameEventComplete_MarksFileSuccessAndDispatchesNext()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 2);

            await saga.Handle(
                new GameEventComplete { BulkImportId = trackingId, FilePath = "2024ARI.EVN", GamesInserted = 81, GamesSkipped = 0 },
                context);

            var ari = bulkRepo.FileNamed("2024ARI.EVN");
            Assert.Equal(BulkImportFileStatus.Success, ari.Status);
            Assert.Equal(81, ari.GamesInserted);

            Assert.Contains("2024SDN.EVN", SentStarts(context).Select(s => Path.GetFileName(s.FilePath)));
            Assert.Equal(2, bulkRepo.Run!.Files.Count(f => f.Status == BulkImportFileStatus.InProgress));
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task GameEventImportFailed_MarksFileFailedWithErrorAndDispatchesNext()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 2);

            await saga.Handle(
                new GameEventImportFailed { BulkImportId = trackingId, FileName = "2024ARI.EVN", Error = "PlayCodeParseException: bad code" },
                context);

            var ari = bulkRepo.FileNamed("2024ARI.EVN");
            Assert.Equal(BulkImportFileStatus.Failed, ari.Status);
            Assert.Equal("PlayCodeParseException: bad code", ari.ErrorMessage);
            Assert.Contains("2024SDN.EVN", SentStarts(context).Select(s => Path.GetFileName(s.FilePath)));
        }

        [Fact]
        public async Task GameEventComplete_ForFileNotInFlight_IsIgnored()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 2);
            var dispatchedBefore = SentStarts(context).Count;

            // 2024SEA.EVA is still Pending (batch size 2 dispatched ARI + LAN only).
            await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = "2024SEA.EVA" }, context);

            Assert.Equal(BulkImportFileStatus.Pending, bulkRepo.FileNamed("2024SEA.EVA").Status);
            Assert.Equal(dispatchedBefore, SentStarts(context).Count);
        }

        [Fact]
        public async Task Run_FinishesWithCompletedWithFailures_WhenAnyFileFailed_KeepsFailedFileButDeletesArchiveAndSuccesses()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 4);
            var workingDir = saga.Data.WorkingDirectory;
            var downloadedZip = saga.Data.DownloadedArchivePath;

            await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = "2024ARI.EVN", GamesInserted = 10 }, context);
            await saga.Handle(new GameEventImportFailed { BulkImportId = trackingId, FileName = "2024LAN.EVN", Error = "boom" }, context);
            await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = "2024SDN.EVN", GamesInserted = 20 }, context);
            Assert.False(saga.Completed);
            await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = "2024SEA.EVA", GamesInserted = 30 }, context);

            Assert.True(saga.Completed);
            Assert.Equal(BulkImportStatus.CompletedWithFailures, bulkRepo.Run!.Status);
            Assert.NotNull(bulkRepo.Run.CompletedUtc);

            Assert.False(File.Exists(Path.Combine(workingDir, "2024ARI.EVN")));
            Assert.False(File.Exists(Path.Combine(workingDir, "2024SDN.EVN")));
            Assert.False(File.Exists(downloadedZip)); // downloaded archive always removed
            Assert.True(File.Exists(Path.Combine(workingDir, "2024LAN.EVN"))); // failed file kept
        }

        [Fact]
        public async Task Run_FinishesWithCompleted_WhenEveryFileSucceeded_AndRemovesEmptyWorkingDir()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 4);
            var workingDir = saga.Data.WorkingDirectory;

            foreach (var name in FourFiles)
                await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = name, GamesInserted = 1 }, context);

            Assert.Equal(BulkImportStatus.Completed, bulkRepo.Run!.Status);
            Assert.False(Directory.Exists(workingDir));
        }

        [Fact]
        public async Task Timeout_Watchdog_FailsEveryUnfinishedFileAndFinishes()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 2);

            await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = "2024ARI.EVN", GamesInserted = 5 }, context);
            await saga.Timeout(new BulkGameEventImportSaga.Watchdog { BulkImportId = trackingId }, context);

            Assert.True(saga.Completed);
            Assert.Equal(BulkImportStatus.CompletedWithFailures, bulkRepo.Run!.Status);
            Assert.Equal(BulkImportFileStatus.Success, bulkRepo.FileNamed("2024ARI.EVN").Status);
            foreach (var name in new[] { "2024LAN.EVN", "2024SDN.EVN", "2024SEA.EVA" })
            {
                var file = bulkRepo.FileNamed(name);
                Assert.Equal(BulkImportFileStatus.Failed, file.Status);
                Assert.Contains("watchdog", file.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task Timeout_Watchdog_AfterRunAlreadyFinished_IsNoOp()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 4);
            foreach (var name in FourFiles)
                await saga.Handle(new GameEventComplete { BulkImportId = trackingId, FilePath = name, GamesInserted = 1 }, context);
            Assert.Equal(BulkImportStatus.Completed, bulkRepo.Run!.Status);

            await saga.Timeout(new BulkGameEventImportSaga.Watchdog { BulkImportId = trackingId }, context);

            Assert.Equal(BulkImportStatus.Completed, bulkRepo.Run.Status);
        }

        private async Task<(BulkGameEventImportSaga Saga, FakeBulkImportRepository BulkRepo, TestableMessageHandlerContext Context, Guid TrackingId)>
            StartedRun(int batchSize)
        {
            var bulkRepo = new FakeBulkImportRepository();
            var trackingId = Guid.NewGuid();
            var saga = CreateSaga(bulkRepo, GameLogImported(), ArchiveClient(), batchSize);
            var context = new TestableMessageHandlerContext();
            await saga.Handle(StartFor(trackingId), context);
            return (saga, bulkRepo, context, trackingId);
        }
    }
}
