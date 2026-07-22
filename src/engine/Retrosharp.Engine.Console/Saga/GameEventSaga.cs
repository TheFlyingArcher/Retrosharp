using Microsoft.Extensions.Logging;
using NServiceBus;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Saga
{
    public class GameEventSaga : Saga<GameEventSagaData>,
        IAmStartedByMessages<GameEventStart>,
        IHandleMessages<GameEventComplete>,
        IHandleMessages<GameEventCancel>
    {
        private readonly ILogger<GameEventSaga> _logger;
        private readonly IGameEventImportService _gameEventImportService;

        public GameEventSaga(ILogger<GameEventSaga> logger, IGameEventImportService gameEventImportService)
        {
            _logger = logger;
            _gameEventImportService = gameEventImportService;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GameEventSagaData> mapper)
        {
            // Correlated on FilePath, not RequestId (unlike PersonSagaData/GameLogSagaData) --
            // this is what lets a second GameEventStart for the same file route to the same
            // saga instance instead of always starting an independent duplicate import.
            mapper.MapSaga(s => s.FilePath)
                .ToMessage<GameEventStart>(m => m.FilePath)
                .ToMessage<GameEventComplete>(m => m.FilePath)
                .ToMessage<GameEventCancel>(m => m.FilePath);
        }

        public async Task Handle(GameEventStart message, IMessageHandlerContext context)
        {
            if (Data.IsRunning)
            {
                _logger.LogInformation(
                    "Game Event import for '{FilePath}' is already in progress; ignoring duplicate request {RequestId}.",
                    message.FilePath, message.RequestId);
                return;
            }

            Data.RequestId = message.RequestId;
            Data.FilePath = message.FilePath;
            Data.IsRunning = true;

            _logger.LogInformation("Starting Game Event import from '{FilePath}'.", message.FilePath);

            var result = await _gameEventImportService.ImportAsync(message.FilePath);

            await context.SendLocal(new GameEventComplete
            {
                RequestId = message.RequestId,
                FilePath = message.FilePath,
                GamesInserted = result.GamesInserted,
                GamesSkipped = result.GamesSkipped,
                StatisticsApplied = result.StatisticsApplied,
                StatisticsSkipped = result.StatisticsSkipped
            });
        }

        public Task Handle(GameEventComplete message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Game Event import complete for '{FilePath}': {GamesInserted} games inserted, {GamesSkipped} games skipped, " +
                "{StatisticsApplied} games' statistics applied, {StatisticsSkipped} games' statistics already claimed.",
                Data.FilePath, message.GamesInserted, message.GamesSkipped, message.StatisticsApplied, message.StatisticsSkipped);

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public Task Handle(GameEventCancel message, IMessageHandlerContext context)
        {
            _logger.LogWarning("Game Event import for '{FilePath}' was cancelled.", Data.FilePath);

            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
