using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Message.Person;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of Retrosheet's biofile. See spec/person.md.
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
        /// Places a message on the service bus to begin parsing the biofile at the given path.
        /// Processing happens asynchronously in Retrosharp.Engine.Console.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] PersonImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return BadRequest("FilePath is required.");

            var message = new PersonStart { RequestId = Guid.NewGuid(), FilePath = request.FilePath };
            await _messageSession.Send(message);
            return Accepted(new { message.RequestId });
        }
    }

    public class PersonImportRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
