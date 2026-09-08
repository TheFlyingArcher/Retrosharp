using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    /// <summary>
    /// Hand-rolled test double for <see cref="IRetrosheetArchiveClient"/>. By default it
    /// writes a real (tiny) zip into the requested working directory so downstream extraction
    /// runs for real; set <see cref="ExceptionToThrow"/> to exercise a download failure, or
    /// <see cref="OnDownload"/> to control exactly what lands on disk.
    /// </summary>
    public class FakeRetrosheetArchiveClient : IRetrosheetArchiveClient
    {
        public Exception? ExceptionToThrow { get; set; }

        /// <summary>
        /// Given (archiveUrl, targetDirectory), stages files and returns the downloaded zip's
        /// path. Defaults to writing an empty zip named after the URL's last segment.
        /// </summary>
        public Func<Uri, string, string>? OnDownload { get; set; }

        public bool WasCalled { get; private set; }
        public Uri? LastUrl { get; private set; }
        public string? LastTargetDirectory { get; private set; }

        public Task<string> DownloadAsync(Uri archiveUrl, string targetDirectory, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            LastUrl = archiveUrl;
            LastTargetDirectory = targetDirectory;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            Directory.CreateDirectory(targetDirectory);
            var path = OnDownload?.Invoke(archiveUrl, targetDirectory)
                       ?? Path.Combine(targetDirectory, Path.GetFileName(archiveUrl.LocalPath));

            return Task.FromResult(path);
        }
    }
}
