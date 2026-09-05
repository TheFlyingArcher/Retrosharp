using Retrosharp.Contract.BulkImport;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    /// <summary>
    /// Thin read wrapper over <see cref="IBulkImportRepository"/> for the bulk import status
    /// endpoint. See spec/bulk-import.md.
    /// </summary>
    public class BulkImportService : IBulkImportService
    {
        private readonly IBulkImportRepository _bulkImportRepository;

        public BulkImportService(IBulkImportRepository bulkImportRepository)
        {
            _bulkImportRepository = bulkImportRepository;
        }

        public Task<BulkImport?> GetByTrackingIdAsync(Guid trackingId) =>
            _bulkImportRepository.GetByTrackingIdAsync(trackingId);
    }
}
