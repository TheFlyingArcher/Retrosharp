using Microsoft.Extensions.Logging;
using NServiceBus;
using Retrosharp.Message.GameLog;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Saga
{
    public class GameLogSaga : Saga<GameLogSagaData>,
        IAmStartedByMessages<GameLogStart>,
        IHandleMessages<GameLogComplete>,
        IHandleMessages<GameLogCancel>
    {
        private readonly ILogger<GameLogSaga> _logger;
        private readonly IGameLogImportService _gameLogImportService;

        public GameLogSaga(ILogger<GameLogSaga> logger, IGameLogImportService gameLogImportService)
        {
            _logger = logger;
            _gameLogImportService = gameLogImportService;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GameLogSagaData> mapper)
        {
            mapper.MapSaga(s => s.RequestId)
                .ToMessage<GameLogStart>(m => m.RequestId)
                .ToMessage<GameLogComplete>(m => m.RequestId)
                .ToMessage<GameLogCancel>(m => m.RequestId);
        }

        public async Task Handle(GameLogStart message, IMessageHandlerContext context)
        {
            Data.SeasonYear = message.SeasonYear;
            Data.FilePath = message.FilePath;

            _logger.LogInformation(
                "Starting Game Log import for season {SeasonYear} from '{FilePath}'.",
                message.SeasonYear, message.FilePath);

            GameLogImportResult result;
            try
            {
                result = await _gameLogImportService.ImportAsync(message.FilePath, message.SeasonYear);
            }
            catch (Exception ex) when (ImportFailureClassifier.IsUnrecoverable(ex))
            {
                // Retrying a mistyped/missing file path can never succeed -- fail the saga here
                // rather than letting the exception reach NServiceBus's recoverability pipeline,
                // which can't tell this apart from a transient failure like a dropped DB
                // connection. See spec/defects.md, "Needless Retrying".
                _logger.LogWarning(ex,
                    "Game Log import for season {SeasonYear} from '{FilePath}' failed with an unrecoverable error; not retrying.",
                    message.SeasonYear, message.FilePath);
                MarkAsComplete();
                return;
            }

            await context.SendLocal(new GameLogComplete
            {
                RequestId = message.RequestId,
                SeasonYear = message.SeasonYear,
                GamesAdded = result.GamesAdded,
                GamesSkipped = result.GamesSkipped
            });
        }

        public Task Handle(GameLogComplete message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Game Log import complete for season {SeasonYear} from '{FilePath}': {GamesAdded} added, {GamesSkipped} skipped.",
                Data.SeasonYear, Data.FilePath, message.GamesAdded, message.GamesSkipped);

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public Task Handle(GameLogCancel message, IMessageHandlerContext context)
        {
            _logger.LogWarning(
                "Game Log import for season {SeasonYear} from '{FilePath}' was cancelled.",
                Data.SeasonYear, Data.FilePath);

            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
