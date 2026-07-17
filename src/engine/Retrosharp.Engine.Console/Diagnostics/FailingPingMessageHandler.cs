using Microsoft.Extensions.Logging;
using NServiceBus;
using Retrosharp.Message.Diagnostics;

namespace Retrosharp.Engine.Console.Diagnostics
{
    public class FailingPingMessageHandler : IHandleMessages<FailingPingMessage>
    {
        private readonly ILogger<FailingPingMessageHandler> _logger;

        public FailingPingMessageHandler(ILogger<FailingPingMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(FailingPingMessage message, IMessageHandlerContext context)
        {
            _logger.LogWarning(
                "Received FailingPingMessage {RequestId}, sent at {SentAtUtc}. Throwing deliberately to exercise the retry/backoff/dead-letter path.",
                message.RequestId, message.SentAtUtc);

            throw new InvalidOperationException("Deliberate failure for Step 4 retry/backoff/dead-letter verification.");
        }
    }
}
