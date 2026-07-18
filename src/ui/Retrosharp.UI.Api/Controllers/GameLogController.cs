using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.GameLog;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of Retrosheet's game log file. See spec/game-log.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameLogController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public GameLogController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        /// <summary>
        /// Places a message on the service bus to begin parsing the game log file at the given
        /// path. Processing happens asynchronously in Retrosharp.Engine.Console.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] GameLogImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return BadRequest("FilePath is required.");

            var message = new GameLogStart
            {
                RequestId = Guid.NewGuid(),
                FilePath = request.FilePath,
                SeasonYear = request.SeasonYear
            };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }

    public class GameLogImportRequest
    {
        public string FilePath { get; set; } = string.Empty;

        public int SeasonYear { get; set; }
    }
}
