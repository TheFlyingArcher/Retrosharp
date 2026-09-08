using System.IO.Compression;

using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Configuration;
using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.Person;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Tests
{
    public sealed class PersonSagaTests : IDisposable
    {
        private readonly string _workRoot =
            Path.Combine(Path.GetTempPath(), "retrosharp-personsaga-tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { if (Directory.Exists(_workRoot)) Directory.Delete(_workRoot, recursive: true); }
            catch { /* best effort */ }
        }

        private PersonSaga CreateSaga(FakePersonImportService importService, FakeRetrosheetArchiveClient archiveClient) =>
            new(NullLogger<PersonSaga>.Instance, importService, archiveClient,
                new RetrosheetSourceConfiguration { WorkingRoot = _workRoot })
            {
                Data = new PersonSagaData()
            };

        /// <summary>A download fake that writes a real biodata.zip (biofile0.csv plus noise).</summary>
        private static FakeRetrosheetArchiveClient DownloadsRealBioArchive(string bioContent = "id,last\naardh101,Aardsma\n") =>
            new()
            {
                OnDownload = (url, dir) =>
                {
                    var zipPath = Path.Combine(dir, Path.GetFileName(url.LocalPath));
                    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                    foreach (var (entry, content) in new[]
                             {
                                 ("biofile0.csv", bioContent),
                                 ("biofile.csv", "legacy"),
                                 ("umpires0.csv", "umps")
                             })
                    {
                        using var writer = new StreamWriter(zip.CreateEntry(entry).Open());
                        writer.Write(content);
                    }
                    return zipPath;
                }
            };

        [Fact]
        public async Task Handle_Start_DownloadsExtractsImports_SendsCompleteAndCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = DownloadsRealBioArchive();
            var importService = new FakePersonImportService
            {
                ResultToReturn = new PersonImportResult { PeopleAdded = 5, PeopleUpdated = 2 }
            };
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonStart { RequestId = requestId }, context);

            Assert.Equal("https://www.retrosheet.org/downloads/biodata.zip", archiveClient.LastUrl!.ToString());
            Assert.True(importService.WasCalled);
            Assert.Equal("biofile0.csv", Path.GetFileName(importService.LastFilePath));

            var sent = Assert.Single(context.SentMessages);
            var complete = Assert.IsType<PersonComplete>(sent.Message);
            Assert.Equal(requestId, complete.RequestId);
            Assert.Equal(5, complete.PeopleAdded);
            Assert.Equal(2, complete.PeopleUpdated);
            Assert.False(saga.Completed);

            Assert.False(Directory.Exists(Path.Combine(_workRoot, "person", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_ArchiveNotFound_PropagatesWithoutImportingAndCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                ExceptionToThrow = new RetrosheetArchiveNotFoundException("HTTP 404 for biodata.zip")
            };
            var importService = new FakePersonImportService();
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<RetrosheetArchiveNotFoundException>(() =>
                saga.Handle(new PersonStart { RequestId = requestId }, context));

            Assert.False(importService.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
            Assert.False(Directory.Exists(Path.Combine(_workRoot, "person", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_ArchiveUnavailable_PropagatesForRetry()
        {
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                ExceptionToThrow = new RetrosheetArchiveUnavailableException("HTTP 503")
            };
            var saga = CreateSaga(new FakePersonImportService(), archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(() =>
                saga.Handle(new PersonStart { RequestId = Guid.NewGuid() }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_ImportThrows_PropagatesAndStillCleansUp()
        {
            var requestId = Guid.NewGuid();
            var archiveClient = DownloadsRealBioArchive();
            var importService = new FakePersonImportService
            {
                ExceptionToThrow = new InvalidOperationException("bad row")
            };
            var saga = CreateSaga(importService, archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                saga.Handle(new PersonStart { RequestId = requestId }, context));

            Assert.True(importService.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
            Assert.False(Directory.Exists(Path.Combine(_workRoot, "person", requestId.ToString("N"))));
        }

        [Fact]
        public async Task Handle_Start_ArchiveHasNoBiofile0_PropagatesInvalidData()
        {
            var archiveClient = new FakeRetrosheetArchiveClient
            {
                OnDownload = (url, dir) =>
                {
                    var zipPath = Path.Combine(dir, Path.GetFileName(url.LocalPath));
                    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                    using var writer = new StreamWriter(zip.CreateEntry("umpires0.csv").Open());
                    writer.Write("only umpires here");
                    return zipPath;
                }
            };
            var saga = CreateSaga(new FakePersonImportService(), archiveClient);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                saga.Handle(new PersonStart { RequestId = Guid.NewGuid() }, context));

            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Complete_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakePersonImportService(), new FakeRetrosheetArchiveClient());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonComplete { RequestId = Guid.NewGuid(), PeopleAdded = 1, PeopleUpdated = 0 }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_Cancel_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakePersonImportService(), new FakeRetrosheetArchiveClient());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonCancel { RequestId = Guid.NewGuid() }, context);

            Assert.True(saga.Completed);
        }
    }
}
