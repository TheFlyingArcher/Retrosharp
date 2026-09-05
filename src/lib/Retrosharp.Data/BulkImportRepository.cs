using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.BulkImport;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    /// <summary>
    /// EF Core-backed <see cref="IBulkImportRepository"/>. See spec/bulk-import.md.
    /// </summary>
    public class BulkImportRepository : IBulkImportRepository
    {
        private readonly RetrosharpContext _context;
        private readonly IMapper _mapper;

        public BulkImportRepository(RetrosharpContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BulkImport> CreateAsync(BulkImport bulkImport)
        {
            if (bulkImport == null)
                throw new ArgumentNullException(nameof(bulkImport));

            var model = _mapper.Map<BulkImportModel>(bulkImport);
            // Mapster copies Id straight across; a create must let the database assign it, on
            // the parent and every child.
            model.Id = 0;
            foreach (var file in model.Files)
            {
                file.Id = 0;
                file.BulkImportId = 0;
            }

            try
            {
                await _context.Database.BeginTransactionAsync();
                _context.Set<BulkImportModel>().Add(model);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }

            return _mapper.Map<BulkImport>(model);
        }

        public async Task<BulkImport?> GetByTrackingIdAsync(Guid trackingId)
        {
            var model = await _context.Set<BulkImportModel>()
                .AsNoTracking()
                .Include(b => b.Files)
                .FirstOrDefaultAsync(b => b.TrackingId == trackingId);

            if (model == null)
                return null;

            model.Files = model.Files.OrderBy(f => f.Id).ToList();
            return _mapper.Map<BulkImport>(model);
        }

        public async Task<BulkImportFileStatus?> GetMostRecentFileOutcomeAsync(short seasonYear, string fileName)
        {
            return await _context.Set<BulkImportFileModel>()
                .AsNoTracking()
                .Where(f => f.FileName == fileName && f.BulkImport.SeasonYear == seasonYear)
                .OrderByDescending(f => f.BulkImport.CreatedUtc)
                .ThenByDescending(f => f.Id)
                .Select(f => (BulkImportFileStatus?)f.Status)
                .FirstOrDefaultAsync();
        }

        public async Task MarkFileInProgressAsync(int bulkImportFileId, DateTime startedUtc)
        {
            var model = await _context.Set<BulkImportFileModel>()
                .FirstOrDefaultAsync(f => f.Id == bulkImportFileId)
                ?? throw new InvalidOperationException($"No BulkImportFile found with Id {bulkImportFileId}.");

            model.Status = BulkImportFileStatus.InProgress;
            model.StartedUtc = startedUtc;
            await _context.SaveChangesAsync();
        }

        public async Task MarkFileCompletedAsync(
            int bulkImportFileId,
            BulkImportFileStatus status,
            DateTime processedUtc,
            string? errorMessage = null,
            int? gamesInserted = null,
            int? gamesSkipped = null)
        {
            var model = await _context.Set<BulkImportFileModel>()
                .FirstOrDefaultAsync(f => f.Id == bulkImportFileId)
                ?? throw new InvalidOperationException($"No BulkImportFile found with Id {bulkImportFileId}.");

            model.Status = status;
            model.ProcessedUtc = processedUtc;
            model.ErrorMessage = errorMessage;
            model.GamesInserted = gamesInserted;
            model.GamesSkipped = gamesSkipped;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBulkStatusAsync(
            int bulkImportId,
            BulkImportStatus status,
            string? failureReason = null,
            DateTime? completedUtc = null)
        {
            var model = await _context.Set<BulkImportModel>()
                .FirstOrDefaultAsync(b => b.Id == bulkImportId)
                ?? throw new InvalidOperationException($"No BulkImport found with Id {bulkImportId}.");

            model.Status = status;
            if (failureReason != null)
                model.FailureReason = failureReason;
            if (completedUtc != null)
                model.CompletedUtc = completedUtc;
            await _context.SaveChangesAsync();
        }
    }
}
