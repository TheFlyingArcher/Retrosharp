using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.GameLog;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests
{
    public class GameLogSagaTests
    {
        private static GameLogSaga CreateSaga(FakeGameLogImportService importService) =>
            new(NullLogger<GameLogSaga>.Instance, importService)
            {
                Data = new GameLogSagaData()
            };

        [Fact]
        public async Task Handle_Start_ImportSucceeds_SendsGameLogCompleteWithResults()
        {
            var requestId = Guid.NewGuid();
            var importService = new FakeGameLogImportService
            {
                ResultToReturn = new GameLogImportResult { GamesAdded = 12, GamesSkipped = 3 }
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogStart { RequestId = requestId, SeasonYear = 2025, FilePath = "gl2025.txt" }, context);

            var sent = Assert.Single(context.SentMessages);
            var complete = Assert.IsType<GameLogComplete>(sent.Message);
            Assert.Equal(requestId, complete.RequestId);
            Assert.Equal(2025, complete.SeasonYear);
            Assert.Equal(12, complete.GamesAdded);
            Assert.Equal(3, complete.GamesSkipped);
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task Handle_Start_UnrecoverableException_PropagatesWithoutCompletingOrSendingComplete()
        {
            // The saga no longer catch-and-completes unrecoverable failures (that made a failed
            // import invisible -- see spec/defects.md, "Needless Retrying"). It now lets every
            // exception propagate; EngineRecoverabilityPolicy is the single place that decides
            // an unrecoverable one (like this missing file) goes straight to the error queue
            // with no retries -- see EngineRecoverabilityPolicyTests.
            var importService = new FakeGameLogImportService
            {
                ExceptionToThrow = new FileNotFoundException("The file at path 'bad.txt' was not found.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                saga.Handle(new GameLogStart { RequestId = Guid.NewGuid(), SeasonYear = 2025, FilePath = "bad.txt" }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_RecoverableException_PropagatesAndLeavesSagaIncomplete()
        {
            // A genuinely transient failure (e.g. a dropped DB connection) must still reach
            // NServiceBus's normal recoverability pipeline, not be swallowed here.
            var importService = new FakeGameLogImportService
            {
                ExceptionToThrow = new TimeoutException("Connection timed out.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<TimeoutException>(() =>
                saga.Handle(new GameLogStart { RequestId = Guid.NewGuid(), SeasonYear = 2025, FilePath = "gl2025.txt" }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Complete_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameLogImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogComplete { RequestId = Guid.NewGuid(), SeasonYear = 2025, GamesAdded = 1, GamesSkipped = 0 }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_Cancel_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameLogImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameLogCancel { RequestId = Guid.NewGuid() }, context);

            Assert.True(saga.Completed);
        }
    }
}
