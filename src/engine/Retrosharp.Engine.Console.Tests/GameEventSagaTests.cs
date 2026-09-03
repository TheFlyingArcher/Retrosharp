using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Format.PlayByPlay;
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
        public async Task Handle_Start_UnrecoverableException_PropagatesWithoutCompletingOrSendingComplete()
        {
            // The saga no longer catch-and-completes unrecoverable failures. Doing so meant a
            // whole event file could fail with nothing on the error queue and a saga that
            // looked successful (found during stress-test Run 1: 2024MIA.EVN's bare "2" silently
            // dropped all 81 games). It now lets every exception propagate; EngineRecoverability
            // Policy routes an unrecoverable one straight to the error queue with no retries --
            // see EngineRecoverabilityPolicyTests and spec/defects.md, "Needless Retrying".
            var importService = new FakeGameEventImportService
            {
                ExceptionToThrow = new PlayCodeParseException(
                    "2", "Fielded-out code has no trajectory modifier (G/L/F/P/BG/BP/BL) to determine GroundOut vs FlyOut.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<PlayCodeParseException>(() =>
                saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "2024MIA.EVN" }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_ResolutionFailure_PropagatesWithoutCompletingOrSendingComplete()
        {
            // A deterministic resolution failure (bad Retrosheet data or a resolver gap, e.g.
            // "InvalidOperationException on base runners") is unrecoverable-by-retrying just like
            // a missing file -- it still propagates, and the recoverability policy routes it to
            // the error queue.
            var importService = new FakeGameEventImportService
            {
                ExceptionToThrow = new InvalidOperationException(
                    "Play 'S7/L7S.3-H(UR);2-H(UR);1-3' (inning 5) references a runner on Third that the resolver has no record of.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                saga.Handle(new GameEventStart { RequestId = Guid.NewGuid(), FilePath = "2025ATH.EVA" }, context));

            Assert.False(saga.Completed);
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
