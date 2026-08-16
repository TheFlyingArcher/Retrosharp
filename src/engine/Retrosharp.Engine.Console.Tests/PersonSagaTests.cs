using Microsoft.Extensions.Logging.Abstractions;

using NServiceBus.Testing;

using Retrosharp.Engine.Console.Saga;
using Retrosharp.Engine.Console.Tests.Fakes;
using Retrosharp.Message.Person;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests
{
    public class PersonSagaTests
    {
        private static PersonSaga CreateSaga(FakePersonImportService importService) =>
            new(NullLogger<PersonSaga>.Instance, importService)
            {
                Data = new PersonSagaData()
            };

        [Fact]
        public async Task Handle_Start_ImportSucceeds_SendsPersonCompleteWithResults()
        {
            var requestId = Guid.NewGuid();
            var importService = new FakePersonImportService
            {
                ResultToReturn = new PersonImportResult { PeopleAdded = 5, PeopleUpdated = 2 }
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonStart { RequestId = requestId, FilePath = "biofile0.csv" }, context);

            var sent = Assert.Single(context.SentMessages);
            var complete = Assert.IsType<PersonComplete>(sent.Message);
            Assert.Equal(requestId, complete.RequestId);
            Assert.Equal(5, complete.PeopleAdded);
            Assert.Equal(2, complete.PeopleUpdated);
            Assert.False(saga.Completed);
        }

        [Fact]
        public async Task Handle_Start_FileNotFound_MarksSagaCompleteWithoutSendingComplete()
        {
            // Regression test for spec/defects.md's "Needless Retrying".
            var importService = new FakePersonImportService
            {
                ExceptionToThrow = new FileNotFoundException("The file at path 'bad.csv' was not found.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonStart { RequestId = Guid.NewGuid(), FilePath = "bad.csv" }, context);

            Assert.True(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Start_RecoverableException_PropagatesAndLeavesSagaIncomplete()
        {
            var importService = new FakePersonImportService
            {
                ExceptionToThrow = new TimeoutException("Connection timed out.")
            };
            var saga = CreateSaga(importService);
            var context = new TestableMessageHandlerContext();

            await Assert.ThrowsAsync<TimeoutException>(() =>
                saga.Handle(new PersonStart { RequestId = Guid.NewGuid(), FilePath = "biofile0.csv" }, context));

            Assert.False(saga.Completed);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_Complete_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakePersonImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonComplete { RequestId = Guid.NewGuid(), PeopleAdded = 1, PeopleUpdated = 0 }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_Cancel_MarksSagaComplete()
        {
            var saga = CreateSaga(new FakePersonImportService());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new PersonCancel { RequestId = Guid.NewGuid() }, context);

            Assert.True(saga.Completed);
        }
    }
}
