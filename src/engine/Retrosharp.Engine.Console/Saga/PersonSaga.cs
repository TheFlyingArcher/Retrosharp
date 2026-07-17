using Microsoft.Extensions.Logging;
using NServiceBus;
using Retrosharp.Message.Person;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Saga
{
    public class PersonSaga : Saga<PersonSagaData>,
        IAmStartedByMessages<PersonStart>,
        IHandleMessages<PersonComplete>,
        IHandleMessages<PersonCancel>
    {
        private readonly ILogger<PersonSaga> _logger;
        private readonly IPersonImportService _personImportService;

        public PersonSaga(ILogger<PersonSaga> logger, IPersonImportService personImportService)
        {
            _logger = logger;
            _personImportService = personImportService;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PersonSagaData> mapper)
        {
            mapper.MapSaga(s => s.RequestId)
                .ToMessage<PersonStart>(m => m.RequestId)
                .ToMessage<PersonComplete>(m => m.RequestId)
                .ToMessage<PersonCancel>(m => m.RequestId);
        }

        public async Task Handle(PersonStart message, IMessageHandlerContext context)
        {
            Data.FilePath = message.FilePath;

            _logger.LogInformation("Starting Person import from '{FilePath}'.", message.FilePath);

            var result = await _personImportService.ImportAsync(message.FilePath);

            await context.SendLocal(new PersonComplete
            {
                RequestId = message.RequestId,
                PeopleAdded = result.PeopleAdded,
                PeopleUpdated = result.PeopleUpdated
            });
        }

        public Task Handle(PersonComplete message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Person import complete for '{FilePath}': {PeopleAdded} added, {PeopleUpdated} updated.",
                Data.FilePath, message.PeopleAdded, message.PeopleUpdated);

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public Task Handle(PersonCancel message, IMessageHandlerContext context)
        {
            _logger.LogWarning("Person import for '{FilePath}' was cancelled.", Data.FilePath);

            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
