using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Retrosharp.Contract.BulkImport;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Initiates a bulk ETL import of a season's Retrosheet play-by-play event files. The
    /// engine downloads the season's event archive from Retrosheet itself -- the request
    /// carries only the season year. See spec/bulk-import.md and
    /// spec/retrosheet-auto-download.md.
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
        /// Places a message on the service bus to begin a bulk import of a season's event
        /// files. Returns immediately with a tracking id; Retrosharp.Engine.Console downloads
        /// <c>{season}eve.zip</c> from Retrosheet, validates it (the season's Game Log must
        /// already be imported), and processes it asynchronously. See spec/bulk-import.md.
        /// </summary>
        [HttpPost("bulkimport")]
        public async Task<IActionResult> BulkImport([FromBody] BulkGameEventImportRequest request)
        {
            if (!RetrosheetSeason.IsPlausible(request.SeasonYear))
                return BadRequest(RetrosheetSeason.RangeMessage);

            if (request.BatchSize is <= 0)
                return BadRequest("batchSize must be a positive number.");

            var trackingId = Guid.NewGuid();
            await _messageSession.Send(new BulkGameEventImportStart
            {
                RequestId = trackingId,
                BulkImportId = trackingId,
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

    public class BulkGameEventImportRequest
    {
        /// <summary>
        /// The season to import. Range-checked by the controller; the engine builds the
        /// Retrosheet event-archive URL from it.
        /// </summary>
        public int SeasonYear { get; set; }

        /// <summary>
        /// Optional. Files processed concurrently; defaults to the engine's configured value.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}
