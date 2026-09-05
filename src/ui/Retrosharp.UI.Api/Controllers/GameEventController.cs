using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Contract.BulkImport;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates ETL processing of Retrosheet's play-by-play event files -- one file at a time
    /// (<c>import</c>) or a whole season's zip archive (<c>bulkimport</c>). See
    /// spec/game-event.md and spec/bulk-import.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameEventController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly IBulkImportService _bulkImportService;

        public GameEventController(IMessageSession messageSession, IBulkImportService bulkImportService)
        {
            _messageSession = messageSession;
            _bulkImportService = bulkImportService;
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

        /// <summary>
        /// Places a message on the service bus to begin a bulk import of a season's zip archive
        /// of team-season event files. Returns immediately with a tracking id; the archive is
        /// read, validated (Game Log for the season must already be imported), and processed
        /// asynchronously in Retrosharp.Engine.Console. See spec/bulk-import.md.
        /// </summary>
        [HttpPost("bulkimport")]
        public async Task<IActionResult> BulkImport([FromBody] BulkGameEventImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ZipPath))
                return BadRequest("ZipPath is required.");

            if (request.BatchSize is <= 0)
                return BadRequest("batchSize must be a positive number.");

            var trackingId = Guid.NewGuid();
            await _messageSession.Send(new BulkGameEventImportStart
            {
                RequestId = trackingId,
                BulkImportId = trackingId,
                ZipPath = request.ZipPath,
                SeasonYear = request.SeasonYear,
                BatchSize = request.BatchSize
            });

            return Accepted(new { trackingId });
        }

        /// <summary>
        /// Reports the progress of a bulk import run: overall status plus a row per event file
        /// discovered in the archive. See spec/bulk-import.md.
        /// </summary>
        [HttpGet("bulkimport/{trackingId:guid}")]
        public async Task<ActionResult<BulkImportStatusResponse>> GetBulkImportStatus(Guid trackingId)
        {
            var run = await _bulkImportService.GetByTrackingIdAsync(trackingId);
            if (run == null)
                return NotFound();

            return ToResponse(run);
        }

        private static BulkImportStatusResponse ToResponse(Retrosharp.Contract.BulkImport.BulkImport run)
        {
            var files = run.Files
                .Select(f => new BulkImportFileLine
                {
                    FileName = f.FileName,
                    Status = f.Status.ToString(),
                    GamesInserted = f.GamesInserted,
                    GamesSkipped = f.GamesSkipped,
                    ErrorMessage = f.ErrorMessage,
                    StartedUtc = f.StartedUtc,
                    ProcessedUtc = f.ProcessedUtc
                })
                .ToList();

            return new BulkImportStatusResponse
            {
                TrackingId = run.TrackingId,
                SeasonYear = run.SeasonYear,
                Status = run.Status.ToString(),
                BatchSize = run.BatchSize,
                FailureReason = run.FailureReason,
                CreatedUtc = run.CreatedUtc,
                CompletedUtc = run.CompletedUtc,
                Counts = new BulkImportCounts
                {
                    Total = run.Files.Count,
                    Pending = run.Files.Count(f => f.Status == BulkImportFileStatus.Pending),
                    InProgress = run.Files.Count(f => f.Status == BulkImportFileStatus.InProgress),
                    Success = run.Files.Count(f => f.Status == BulkImportFileStatus.Success),
                    Failed = run.Files.Count(f => f.Status == BulkImportFileStatus.Failed),
                    Skipped = run.Files.Count(f => f.Status == BulkImportFileStatus.Skipped)
                },
                Files = files
            };
        }
    }

    public class GameEventImportRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }

    public class BulkGameEventImportRequest
    {
        /// <summary>
        /// Path to the <c>.zip</c> archive of the season's team-season event files, on a
        /// volume visible to both Retrosharp.UI.Api and Retrosharp.Engine.Console.
        /// </summary>
        public string ZipPath { get; set; } = string.Empty;

        /// <summary>
        /// Optional. Validated against the season parsed from the archive's file names.
        /// </summary>
        public int? SeasonYear { get; set; }

        /// <summary>
        /// Optional. Files processed concurrently; defaults to the engine's configured value.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}
