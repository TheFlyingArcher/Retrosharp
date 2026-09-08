using Microsoft.Extensions.Logging;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Cleanup for the per-run working directories the download-based import sagas stage their
    /// archives in (the downloaded zip plus everything extracted from it). The parent is
    /// <c>RetrosheetSourceConfiguration.ResolvedWorkingRoot</c>. See
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    internal static class WorkingDirectory
    {
        /// <summary>
        /// Deletes <paramref name="path"/> and its contents. Never throws -- a directory that
        /// cannot be removed is logged and left for the next run / the OS temp sweep.
        /// </summary>
        public static void TryDelete(string path, ILogger logger)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not delete working directory '{Path}'.", path);
            }
        }
    }
}
