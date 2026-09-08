using Retrosharp.Contract.BulkImport;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// State for one bulk Game Event import run. Correlated on <see cref="BulkImportId"/> (the
    /// caller-facing tracking id), which every child <c>GameEventStart</c>/<c>GameEventComplete</c>/
    /// <c>GameEventImportFailed</c> also carries. See spec/bulk-import.md.
    /// </summary>
    public class BulkGameEventImportSagaData : BaseSagaData
    {
        /// <summary>
        /// The run's tracking id and this saga's correlation key. Mirrors
        /// <see cref="Retrosharp.Message.GameEvent.BulkGameEventImportStart.BulkImportId"/>.
        /// </summary>
        public Guid BulkImportId { get; set; }

        /// <summary>
        /// Primary key of the persisted <c>BulkImport</c> row this saga updates as files
        /// resolve. Zero until the start handler has created it.
        /// </summary>
        public int BulkImportRowId { get; set; }

        /// <summary>
        /// The season being imported (from the request, cross-checked against the downloaded
        /// archive's file names).
        /// </summary>
        public short SeasonYear { get; set; }

        /// <summary>
        /// Per-run working directory: the downloaded event archive plus everything extracted
        /// from it. Removed (bar any files that ended <c>Failed</c>) when the run finishes.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Local path of the downloaded <c>{season}eve.zip</c>. Always deleted when the run
        /// finishes -- it is re-downloadable. See spec/retrosheet-auto-download.md.
        /// </summary>
        public string DownloadedArchivePath { get; set; }

        /// <summary>
        /// Maximum number of files dispatched concurrently.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// True once the start handler has got past validation and begun dispatching files.
        /// Guards against a redelivered <c>BulkGameEventImportStart</c> re-running startup.
        /// </summary>
        public bool ProcessingStarted { get; set; }

        /// <summary>
        /// One entry per event file discovered in the archive, tracking its progress through
        /// this run. The run is finished when every entry is terminal
        /// (<see cref="BulkImportFileStatus.Success"/>/<see cref="BulkImportFileStatus.Failed"/>/
        /// <see cref="BulkImportFileStatus.Skipped"/>).
        /// </summary>
        public List<BulkGameEventImportFileState> Files { get; set; } = new();
    }

    /// <summary>
    /// Per-file progress within a bulk import run, held in the saga data.
    /// </summary>
    public class BulkGameEventImportFileState
    {
        /// <summary>
        /// Primary key of the file's <c>BulkImportFile</c> row.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Event file name only, e.g. <c>2024SDN.EVN</c>.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// <see cref="BulkImportFileStatus.Pending"/> (not yet dispatched),
        /// <see cref="BulkImportFileStatus.InProgress"/> (dispatched, awaiting result), or a
        /// terminal outcome.
        /// </summary>
        public BulkImportFileStatus Status { get; set; }
    }
}
