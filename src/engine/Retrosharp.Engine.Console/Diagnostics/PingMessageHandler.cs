using Microsoft.Extensions.Logging;
using NServiceBus;
using Retrosharp.Message.Diagnostics;

namespace Retrosharp.Engine.Console.Diagnostics
{
    public class PingMessageHandler : IHandleMessages<PingMessage>
    {
        private readonly ILogger<PingMessageHandler> _logger;

        public PingMessageHandler(ILogger<PingMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(PingMessage message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Received PingMessage {RequestId}, sent at {SentAtUtc}.",
                message.RequestId, message.SentAtUtc);

            return Task.CompletedTask;
        }
    }
}
