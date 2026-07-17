using NServiceBus;

namespace Retrosharp.Message.Diagnostics
{
    /// <summary>
    /// Infrastructure-only message whose handler always throws, used to verify the Step 4
    /// retry/backoff/dead-letter path end-to-end. Not tied to any real parser.
    /// See spec/phase-1-build-plan.md Step 4.
    /// </summary>
    public class FailingPingMessage : BaseMessage, IMessage
    {
        public FailingPingMessage() { }

        public DateTime SentAtUtc { get; set; }
    }
}
