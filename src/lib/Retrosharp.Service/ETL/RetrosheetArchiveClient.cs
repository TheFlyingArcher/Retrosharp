using System.Net;

using Microsoft.Extensions.Logging;

using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.ETL
{
    /// <summary>
    /// Downloads a Retrosheet archive over HTTP into a local working directory, for the import
    /// sagas in Retrosharp.Engine.Console. The only network seam in the ETL path. See
    /// spec/retrosheet-auto-download.md.
    ///
    /// Failure mapping: HTTP 404/410 -> <see cref="RetrosheetArchiveNotFoundException"/>
    /// (unrecoverable -- a bad season year); a request timeout, a transport fault, or any
    /// other non-success response -> <see cref="RetrosheetArchiveUnavailableException"/>
    /// (retryable); a successful response whose body is not a zip -> <see cref="InvalidDataException"/>.
    /// The typed <see cref="HttpClient"/> is registered in the engine's <c>Program.cs</c>
    /// only, never in Retrosharp.Service's shared registrations -- Retrosharp.UI.Api has no
    /// need of it.
    /// </summary>
    public sealed class RetrosheetArchiveClient : IRetrosheetArchiveClient
    {
        // The local file header that begins every non-empty zip: 'P' 'K' 0x03 0x04.
        private static readonly byte[] ZipLocalFileHeader = [0x50, 0x4B, 0x03, 0x04];

        private readonly HttpClient _httpClient;
        private readonly ILogger<RetrosheetArchiveClient> _logger;

        public RetrosheetArchiveClient(HttpClient httpClient, ILogger<RetrosheetArchiveClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> DownloadAsync(Uri archiveUrl, string targetDirectory, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(targetDirectory);

            var fileName = Path.GetFileName(archiveUrl.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "archive.zip";
            var targetPath = Path.Combine(targetDirectory, fileName);

            _logger.LogInformation("Downloading Retrosheet archive {ArchiveUrl}.", archiveUrl);

            using var response = await SendAsync(archiveUrl, cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                throw new RetrosheetArchiveNotFoundException(
                    $"Retrosheet returned HTTP {(int)response.StatusCode} for '{archiveUrl}'. The archive does not exist -- check the season year.");

            if (!response.IsSuccessStatusCode)
                throw new RetrosheetArchiveUnavailableException(
                    $"Retrosheet returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) for '{archiveUrl}'.");

            await WriteBodyToFileAsync(response, targetPath, archiveUrl, cancellationToken);
            await VerifyZipAsync(targetPath, archiveUrl, cancellationToken);

            _logger.LogInformation(
                "Downloaded Retrosheet archive {ArchiveUrl} to '{TargetPath}' ({Bytes} bytes).",
                archiveUrl, targetPath, new FileInfo(targetPath).Length);

            return targetPath;
        }

        private async Task<HttpResponseMessage> SendAsync(Uri archiveUrl, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.GetAsync(archiveUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // A genuine caller-initiated cancellation, not an HttpClient timeout.
                throw;
            }
            catch (OperationCanceledException ex)
            {
                throw new RetrosheetArchiveUnavailableException(
                    $"The request for '{archiveUrl}' timed out after {_httpClient.Timeout.TotalSeconds:0}s.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new RetrosheetArchiveUnavailableException(
                    $"The request for '{archiveUrl}' failed: {ex.Message}", ex);
            }
        }

        private static async Task WriteBodyToFileAsync(
            HttpResponseMessage response, string targetPath, Uri archiveUrl, CancellationToken cancellationToken)
        {
            try
            {
                await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var destination = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await source.CopyToAsync(destination, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException && !cancellationToken.IsCancellationRequested)
            {
                TryDelete(targetPath);
                throw new RetrosheetArchiveUnavailableException(
                    $"The download of '{archiveUrl}' was interrupted: {ex.Message}", ex);
            }
        }

        private static async Task VerifyZipAsync(string targetPath, Uri archiveUrl, CancellationToken cancellationToken)
        {
            var header = new byte[ZipLocalFileHeader.Length];
            int read;
            await using (var stream = new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
            }

            if (read < ZipLocalFileHeader.Length || !header.AsSpan().SequenceEqual(ZipLocalFileHeader))
            {
                TryDelete(targetPath);
                throw new InvalidDataException(
                    $"The response body from '{archiveUrl}' is not a zip archive (unexpected header bytes). " +
                    "Retrosheet may have returned an error page instead of the file.");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Best effort -- a stale partial file in a per-run working directory is cleaned
                // up with the directory when the import finishes.
            }
        }
    }
}
