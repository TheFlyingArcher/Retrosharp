using Microsoft.Extensions.Logging;
using NServiceBus;

using Retrosharp.Configuration;
using Retrosharp.Message.Person;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Saga
{
    public class PersonSaga : Saga<PersonSagaData>,
        IAmStartedByMessages<PersonStart>,
        IHandleMessages<PersonComplete>,
        IHandleMessages<PersonCancel>
    {
        private readonly ILogger<PersonSaga> _logger;
        private readonly IPersonImportService _personImportService;
        private readonly IRetrosheetArchiveClient _archiveClient;
        private readonly RetrosheetSourceConfiguration _source;

        public PersonSaga(
            ILogger<PersonSaga> logger,
            IPersonImportService personImportService,
            IRetrosheetArchiveClient archiveClient,
            RetrosheetSourceConfiguration source)
        {
            _logger = logger;
            _personImportService = personImportService;
            _archiveClient = archiveClient;
            _source = source;
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
            var archiveUrl = _source.BioFileUri();
            var workingDirectory = Path.Combine(
                _source.ResolvedWorkingRoot, "person", message.RequestId.ToString("N"));

            _logger.LogInformation("Starting Person import from {ArchiveUrl}.", archiveUrl);

            try
            {
                var archivePath = await _archiveClient.DownloadAsync(archiveUrl, workingDirectory, context.CancellationToken);
                var bioFilePath = BioFileArchive.Extract(archivePath, workingDirectory);

                // Any failure propagates to the endpoint's recoverability policy
                // (EngineRecoverabilityPolicy): a transient error (a 5xx/timeout download, a
                // dropped DB connection) is retried with backoff; an unrecoverable one (a 404,
                // a corrupt archive, a deterministic data failure) is routed straight to the
                // error queue. The saga deliberately does not catch-and-complete unrecoverable
                // failures -- see spec/defects.md, "Needless Retrying".
                var result = await _personImportService.ImportAsync(bioFilePath);

                await context.SendLocal(new PersonComplete
                {
                    RequestId = message.RequestId,
                    PeopleAdded = result.PeopleAdded,
                    PeopleUpdated = result.PeopleUpdated
                });
            }
            finally
            {
                // The working directory (downloaded archive + extracted biofile0.csv) is
                // scratch space; a retry re-downloads. See spec/retrosheet-auto-download.md.
                WorkingDirectory.TryDelete(workingDirectory, _logger);
            }
        }

        public Task Handle(PersonComplete message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Person import complete: {PeopleAdded} added, {PeopleUpdated} updated.",
                message.PeopleAdded, message.PeopleUpdated);

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public Task Handle(PersonCancel message, IMessageHandlerContext context)
        {
            _logger.LogWarning("Person import was cancelled.");

            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
