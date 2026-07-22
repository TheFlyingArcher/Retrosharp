using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.GameEvent;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of Retrosheet's play-by-play event file. See spec/game-event.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameEventController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public GameEventController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        /// <summary>
        /// Places a message on the service bus to begin parsing the game event file at the
        /// given path. Processing happens asynchronously in Retrosharp.Engine.Console.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] GameEventImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return BadRequest("FilePath is required.");

            var message = new GameEventStart { RequestId = Guid.NewGuid(), FilePath = request.FilePath };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }

    public class GameEventImportRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
