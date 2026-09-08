using System.IO.Compression;
using System.Net;
using System.Text;

using Microsoft.Extensions.Logging.Abstractions;

using Retrosharp.Service.ETL;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.Tests
{
    /// <summary>
    /// Tests for <see cref="RetrosheetArchiveClient"/> (Step 11b, see
    /// spec/retrosheet-auto-download.md) using a stub <see cref="HttpMessageHandler"/> -- no
    /// network.
    /// </summary>
    public sealed class RetrosheetArchiveClientTests : IDisposable
    {
        private static readonly Uri ArchiveUrl = new("https://www.retrosheet.org/gamelogs/gl2024.zip");

        private readonly string _workRoot =
            Path.Combine(Path.GetTempPath(), "retrosharp-archiveclient-tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { Directory.Delete(_workRoot, recursive: true); } catch { /* best effort */ }
        }

        [Fact]
        public async Task DownloadAsync_SuccessfulZip_WritesFileAndReturnsPath()
        {
            var zipBytes = MakeZip();
            var handler = new StubHandler(_ => BinaryResponse(HttpStatusCode.OK, zipBytes));
            var client = ClientFor(handler);
            var targetDir = Path.Combine(_workRoot, "run1");

            var path = await client.DownloadAsync(ArchiveUrl, targetDir);

            Assert.Equal(Path.Combine(targetDir, "gl2024.zip"), path);
            Assert.True(File.Exists(path));
            Assert.Equal(zipBytes, await File.ReadAllBytesAsync(path));
            Assert.Equal(ArchiveUrl, handler.LastRequestUri);
        }

        [Fact]
        public async Task DownloadAsync_CreatesTargetDirectoryWhenMissing()
        {
            var handler = new StubHandler(_ => BinaryResponse(HttpStatusCode.OK, MakeZip()));
            var client = ClientFor(handler);
            var targetDir = Path.Combine(_workRoot, "does", "not", "exist", "yet");

            var path = await client.DownloadAsync(ArchiveUrl, targetDir);

            Assert.True(File.Exists(path));
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Gone)]
        public async Task DownloadAsync_NotFoundOrGone_ThrowsArchiveNotFound(HttpStatusCode status)
        {
            var client = ClientFor(new StubHandler(_ => new HttpResponseMessage(status)));

            await Assert.ThrowsAsync<RetrosheetArchiveNotFoundException>(
                () => client.DownloadAsync(ArchiveUrl, _workRoot));
            Assert.False(File.Exists(Path.Combine(_workRoot, "gl2024.zip")));
        }

        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task DownloadAsync_OtherNonSuccess_ThrowsArchiveUnavailable(HttpStatusCode status)
        {
            var client = ClientFor(new StubHandler(_ => new HttpResponseMessage(status)));

            await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(
                () => client.DownloadAsync(ArchiveUrl, _workRoot));
        }

        [Fact]
        public async Task DownloadAsync_TransportFault_ThrowsArchiveUnavailable()
        {
            var client = ClientFor(new StubHandler(_ => throw new HttpRequestException("connection reset")));

            var ex = await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(
                () => client.DownloadAsync(ArchiveUrl, _workRoot));
            Assert.IsType<HttpRequestException>(ex.InnerException);
        }

        [Fact]
        public async Task DownloadAsync_Timeout_ThrowsArchiveUnavailable()
        {
            // HttpClient surfaces its own timeout as a TaskCanceledException with no
            // cancellation on the caller's token.
            var client = ClientFor(new StubHandler(
                _ => throw new TaskCanceledException("timed out", new TimeoutException())));

            await Assert.ThrowsAsync<RetrosheetArchiveUnavailableException>(
                () => client.DownloadAsync(ArchiveUrl, _workRoot));
        }

        [Fact]
        public async Task DownloadAsync_CallerCancellation_PropagatesOperationCanceled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var client = ClientFor(new StubHandler(
                (_, ct) => { ct.ThrowIfCancellationRequested(); return BinaryResponse(HttpStatusCode.OK, MakeZip()); }));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => client.DownloadAsync(ArchiveUrl, _workRoot, cts.Token));
        }

        [Fact]
        public async Task DownloadAsync_NonZipBody_ThrowsInvalidDataAndRemovesFile()
        {
            var htmlBytes = Encoding.UTF8.GetBytes("<html><body>Not found</body></html>");
            var client = ClientFor(new StubHandler(_ => BinaryResponse(HttpStatusCode.OK, htmlBytes)));

            await Assert.ThrowsAsync<InvalidDataException>(() => client.DownloadAsync(ArchiveUrl, _workRoot));
            Assert.False(File.Exists(Path.Combine(_workRoot, "gl2024.zip")));
        }

        private static RetrosheetArchiveClient ClientFor(HttpMessageHandler handler) =>
            new(new HttpClient(handler), NullLogger<RetrosheetArchiveClient>.Instance);

        private static byte[] MakeZip(string entryName = "GL2024.TXT", string content = "20240328,...\n")
        {
            using var buffer = new MemoryStream();
            using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            {
                using var writer = new StreamWriter(zip.CreateEntry(entryName).Open());
                writer.Write(content);
            }

            return buffer.ToArray();
        }

        private static HttpResponseMessage BinaryResponse(HttpStatusCode status, byte[] body) =>
            new(status) { Content = new ByteArrayContent(body) };

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _responder;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
                : this((request, _) => responder(request))
            {
            }

            public StubHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responder) =>
                _responder = responder;

            public Uri? LastRequestUri { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestUri = request.RequestUri;
                return Task.FromResult(_responder(request, cancellationToken));
            }
        }
    }
}
