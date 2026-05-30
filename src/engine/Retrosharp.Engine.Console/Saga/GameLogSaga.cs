using Microsoft.Extensions.Logging;

using Retrosharp.Data.Saga.GameLog;
using Retrosharp.Message.GameLog;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Saga
{
    public class GameLogSaga : Saga<GameLogSagaData>,
        IAmStartedByMessages<GameLogStart>,
        IHandleMessages<GameLogComplete>,
        IHandleMessages<GameLogCancel>
    {
        public GameLogSaga(ILogger<GameLogSaga> logger, IGameService service)
        {

        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GameLogSagaData> mapper)
        {
            mapper.MapSaga(s => s.RequestId)
                .ToMessage<GameLogStart>(m => m.RequestId)
                .ToMessage<GameLogComplete>(m => m.RequestId);
        }

        public Task Handle(GameLogCancel message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        public Task Handle(GameLogComplete message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        public Task Handle(GameLogStart message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
