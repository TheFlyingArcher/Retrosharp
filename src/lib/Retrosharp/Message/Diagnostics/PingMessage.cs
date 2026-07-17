using NServiceBus;

namespace Retrosharp.Message.Diagnostics
{
    /// <summary>
    /// Infrastructure-only message used to verify the Step 4 messaging pipeline
    /// (send from Retrosharp.UI.Api, receive and process in Retrosharp.Engine.Console)
    /// end-to-end. Not tied to any real parser. See spec/phase-1-build-plan.md Step 4.
    /// </summary>
    public class PingMessage : BaseMessage, IMessage
    {
        public PingMessage() { }

        public DateTime SentAtUtc { get; set; }
    }
}
