using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.GameLog;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of a season's Retrosheet game log. The engine downloads the
    /// archive from Retrosheet itself -- the request carries only the season year. See
    /// spec/game-log.md and spec/retrosheet-auto-download.md.
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
        /// Places a message on the service bus to begin importing the given season's game log.
        /// Retrosharp.Engine.Console downloads <c>gl{season}.zip</c> from Retrosheet, extracts
        /// the game-log file, and processes it asynchronously.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] GameLogImportRequest request)
        {
            if (!RetrosheetSeason.IsPlausible(request.SeasonYear))
                return BadRequest(RetrosheetSeason.RangeMessage);

            var message = new GameLogStart
            {
                RequestId = Guid.NewGuid(),
                SeasonYear = request.SeasonYear
            };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }

    public class GameLogImportRequest
    {
        public int SeasonYear { get; set; }
    }
}
