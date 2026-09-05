using Retrosharp.Contract.BulkImport;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Read access to bulk Game Event import runs for the status endpoint
    /// (<c>GET /api/gameevent/bulkimport/{trackingId}</c>). Initiating a run is a message on
    /// the bus, not a service call. See spec/bulk-import.md.
    /// </summary>
    public interface IBulkImportService
    {
        /// <summary>
        /// Gets a bulk import run and its per-file rows by tracking id, or null if no run has
        /// that id.
        /// </summary>
        Task<BulkImport?> GetByTrackingIdAsync(Guid trackingId);
    }
}
