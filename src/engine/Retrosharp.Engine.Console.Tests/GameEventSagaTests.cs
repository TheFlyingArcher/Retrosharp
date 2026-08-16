using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests
{
    public class GameEventSagaTests
    {
        private static GameEventSaga CreateSaga(FakeGameEventImportService importService, GameEventSagaData? data = null) =>
            new(NullLogger<GameEventSaga>.Instance, importService)
            {
                Data = data ?? new GameEventSagaData()
            };

        [Fact]
        public async Task Handle_Start_ImportSucceeds_SendsGameEventCompleteWithResults()
        {
            var requestId = Guid.NewGuid();
            var importService = new FakeGameEventImportService
            {
                ResultToReturn = new GameEventImportResult
                {
                    GamesInserted = 80,
                    GamesSkipped = 1,
                    StatisticsApplied = 81,
                    StatisticsSkipped = 0
                }
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventStart { RequestId = requestId, FilePath = "2025SDN.EVN" }, context);

            var sent = Assert.Single(context.SentMessages);
            var complete = Assert.IsType<GameEventComplete>(sent.Message);
            Assert.Equal(requestId, complete.RequestId);
            Assert.Equal(80, complete.GamesInserted);
            Assert.Equal(1, complete.GamesSkipped);
            Assert.Equal(81, complete.StatisticsApplied);
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task Handle_Start_FileNotFound_MarksSagaCompleteWithoutSendingComplete()
        {
            // Regression test for spec/defects.md's "Needless Retrying".
            var importService = new FakeGameEventImportService
            {
                ExceptionToThrow = new FileNotFoundException("The file at path 'bad.EVN' was not found.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "bad.EVN" }, context);

            Assert.True(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_InvalidOperationException_MarksSagaCompleteWithoutSendingComplete()
        {
            // Regression test for spec/defects.md's "Needless Retrying" re-open: a play code
            // resolution failure (bad Retrosheet data or a resolver gap, e.g. "InvalidOperationException
            // on base runners") is just as unrecoverable-by-retrying as a missing file.
            var importService = new FakeGameEventImportService
            {
                ExceptionToThrow = new InvalidOperationException(
                    "Play 'S7/L7S.3-H(UR);2-H(UR);1-3' (inning 5) references a runner on Third that the resolver has no record of.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "2025ATH.EVA" }, context);

            Assert.True(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_RecoverableException_PropagatesAndLeavesSagaIncomplete()
        {
            var importService = new FakeGameEventImportService
            {
                ExceptionToThrow = new TimeoutException("Connection timed out.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<TimeoutException>(() =>
                saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "2025SDN.EVN" }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_WhenAlreadyRunning_IgnoresDuplicateAndDoesNotCallImportService()
        {
            var importService = new FakeGameEventImportService();
            var data = new GameEventSagaData { FilePath = "2025SDN.EVN", IsRunning = true };
            var saga = CreateSaga(importService, data);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "2025SDN.EVN" }, context);

            Assert.False(importService.WasCalled);
            Assert.Empty(context.SentMessages);
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task Handle_Complete_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameEventImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventComplete { RequestId = Guid.NewGuid(), FilePath = "2025SDN.EVN" }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_Cancel_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakeGameEventImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new GameEventCancel { RequestId = Guid.NewGuid(), FilePath = "2025SDN.EVN" }, context);

            Assert.True(saga.Completed);
        }
    }
}
