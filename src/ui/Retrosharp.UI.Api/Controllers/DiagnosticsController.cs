using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.Diagnostics;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Infrastructure-only endpoints used to verify the Step 4 messaging pipeline end-to-end
    /// (send from here, received and processed in Retrosharp.Engine.Console). Not tied to any
    /// real parser. See spec/phase-1-build-plan.md Step 4.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public DiagnosticsController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        [HttpPost("ping")]
        public async Task<IActionResult> Ping()
        {
            var message = new PingMessage { RequestId = Guid.NewGuid(), SentAtUtc = DateTime.UtcNow };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }

        [HttpPost("ping-failing")]
        public async Task<IActionResult> PingFailing()
        {
            var message = new FailingPingMessage { RequestId = Guid.NewGuid(), SentAtUtc = DateTime.UtcNow };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }
}
