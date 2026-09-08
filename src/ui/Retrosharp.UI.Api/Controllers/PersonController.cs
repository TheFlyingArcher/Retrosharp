using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.Person;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of Retrosheet's biofile. The engine downloads the archive
    /// from Retrosheet itself -- the request has no body. See spec/person.md and
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public PersonController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        /// <summary>
        /// Places a message on the service bus to begin importing Retrosheet's biographical
        /// data. Retrosharp.Engine.Console downloads <c>biodata.zip</c> from Retrosheet,
        /// extracts <c>biofile0.csv</c>, and processes it asynchronously.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import()
        {
            var message = new PersonStart { RequestId = Guid.NewGuid() };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }
}
