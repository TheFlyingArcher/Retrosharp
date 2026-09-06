using System.IO.Compression;

using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Configuration;
using Retrosharp.Contract.BulkImport;
using Retrosharp.Contract.Game;
using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.GameEvent;

namespace Retrosharp.Engine.Console.Tests
{
    public sealed class BulkGameEventImportSagaTests : IDisposable
    {
        // Sorted, so the first N dispatched files are predictable.
        private static readonly string[] FourFiles = { "2024ARI.EVN", "2024LAN.EVN", "2024SDN.EVN", "2024SEA.EVA" };

        private readonly string _tempRoot;
        private readonly string _extractionRoot;

        public BulkGameEventImportSagaTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "retrosharp-bulk-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            _extractionRoot = Path.Combine(_tempRoot, "extract");
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* best effort */ }
        }

        // --- helpers ---

        private string CreateArchive(string name, params string[] entryNames)
        {
            var zipPath = Path.Combine(_tempRoot, name);
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (var entryName in entryNames)
            {
                using var stream = zip.CreateEntry(entryName).Open();
                using var writer = new StreamWriter(stream);
                writer.Write($"id,{entryName}\n");
            }
            return zipPath;
        }

        private BulkGameEventImportSaga CreateSaga(
            FakeBulkImportRepository bulkRepo,
            FakeGameRepository gameRepo,
            int batchSize = 2)
            => new(
                NullLogger<BulkGameEventImportSaga>.Instance,
                bulkRepo,
                gameRepo,
                new BulkImportConfiguration { DefaultBatchSize = batchSize, WatchdogTimeoutHours = 1, ExtractionRoot = _extractionRoot })
            {
                Data = new BulkGameEventImportSagaData()
            };

        private static FakeGameRepository GameLogImported() => new() { GamesBySeason = { new Game() } };

        private static BulkGameEventImportStart StartFor(Guid trackingId, string zipPath, int? season = null, int? batchSize = null) =>
            new() { RequestId = trackingId, BulkImportId = trackingId, ZipPath = zipPath, SeasonYear = season, BatchSize = batchSize };

        private static IReadOnlyList<GameEventStart> SentStarts(TestableMessageHandlerContext context) =>
            context.SentMessages.Select(m => m.Message).OfType<GameEventStart>().ToList();

        // --- startup validation ---

        [Fact]
        public async Task Start_GameLogNotImported_MarksRunFailedAndDispatchesNothing()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var trackingId = Guid.NewGuid();
            var saga = CreateSaga(bulkRepo, new FakeGameRepository()); // GamesBySeason empty
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(trackingId, CreateArchive("2024eve.zip", FourFiles)), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("Game Log for season 2024", bulkRepo.Run.FailureReason);
            Assert.Empty(SentStarts(context));
            Assert.Empty(context.TimeoutMessages);
            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Start_ArchiveMissing_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), Path.Combine(_tempRoot, "does-not-exist.zip")), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("could not be read", bulkRepo.Run.FailureReason);
            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Start_MultiSeasonArchive_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), CreateArchive("mixed.zip", "2024SDN.EVN", "2023ARI.EVN")), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("multiple seasons", bulkRepo.Run.FailureReason);
        }

        [Fact]
        public async Task Start_RequestedSeasonMismatch_MarksRunFailed()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), CreateArchive("2024eve.zip", FourFiles), season: 2023), context);

            Assert.Equal(BulkImportStatus.Failed, bulkRepo.Run!.Status);
            Assert.Contains("2023", bulkRepo.Run.FailureReason);
            Assert.Contains("2024", bulkRepo.Run.FailureReason);
        }

        [Fact]
        public async Task Start_WhenAlreadyProcessing_IgnoresDuplicate()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var saga = CreateSaga(bulkRepo, GameLogImported());
            saga.Data.ProcessingStarted = true;
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), CreateArchive("2024eve.zip", FourFiles)), context);

            Assert.Null(bulkRepo.Run);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
        }

        // --- happy path / dispatch window ---

        [Fact]
        public async Task Start_HappyPath_SeedsExtractsAndDispatchesUpToBatchSize()
        {
            var bulkRepo = new FakeBulkImportRepository();
            var trackingId = Guid.NewGuid();
            var saga = CreateSaga(bulkRepo, GameLogImported(), batchSize: 2);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(trackingId, CreateArchive("2024eve.zip", FourFiles)), context);

            Assert.Equal(BulkImportStatus.InProgress, bulkRepo.Run!.Status);
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
            var saga = CreateSaga(bulkRepo, GameLogImported(), batchSize: 10);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), CreateArchive("2024eve.zip", FourFiles)), context);

            Assert.Equal(BulkImportFileStatus.Skipped, bulkRepo.FileNamed("2024ARI.EVN").Status);
            Assert.Equal(BulkImportFileStatus.Skipped, bulkRepo.FileNamed("2024LAN.EVN").Status);
            Assert.False(File.Exists(Path.Combine(saga.Data.WorkingDirectory, "2024ARI.EVN")));
            Assert.False(File.Exists(Path.Combine(saga.Data.WorkingDirectory, "2024LAN.EVN")));
            var dispatched = SentStarts(context).Select(s => Path.GetFileName(s.FilePath)).ToList();
            Assert.Equal(new[] { "2024SDN.EVN", "2024SEA.EVA" }, dispatched.OrderBy(n => n));
        }

        [Fact]
        public async Task Start_AllFilesAlreadySucceeded_CompletesImmediatelyWithNoDispatchOrTimeout()
        {
            var bulkRepo = new FakeBulkImportRepository();
            foreach (var f in FourFiles)
                bulkRepo.PriorOutcomes[(2024, f)] = BulkImportFileStatus.Success;
            var saga = CreateSaga(bulkRepo, GameLogImported());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(StartFor(Guid.NewGuid(), CreateArchive("2024eve.zip", FourFiles)), context);

            Assert.Empty(context.SentMessages);
            Assert.Empty(context.TimeoutMessages);
            Assert.Equal(BulkImportStatus.Completed, bulkRepo.Run!.Status);
            Assert.True(saga.Completed);
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
        public async Task Run_FinishesWithCompletedWithFailures_WhenAnyFileFailed_AndCleansUpOnlySuccesses()
        {
            var (saga, bulkRepo, context, trackingId) = await StartedRun(batchSize: 4);
            var workingDir = saga.Data.WorkingDirectory;

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
            var saga = CreateSaga(bulkRepo, GameLogImported(), batchSize);
            var context = new TestableMessageHandlerContext();
            await saga.Handle(StartFor(trackingId, CreateArchive("2024eve.zip", FourFiles)), context);
            return (saga, bulkRepo, context, trackingId);
        }
    }
}
