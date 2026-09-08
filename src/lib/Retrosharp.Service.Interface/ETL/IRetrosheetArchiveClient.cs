namespace Retrosharp.Service.Interface.ETL
{
    /// <summary>
    /// Downloads a Retrosheet source archive (a season game-log or event zip, or the biofile)
    /// over HTTP into a local working directory. The one network seam the import sagas in
    /// Retrosharp.Engine.Console use. See spec/retrosheet-auto-download.md.
    /// </summary>
    public interface IRetrosheetArchiveClient
    {
        /// <summary>
        /// Downloads the zip at <paramref name="archiveUrl"/> into
        /// <paramref name="targetDirectory"/> (created if it does not exist) and returns the
        /// local path of the written file.
        /// </summary>
        /// <exception cref="RetrosheetArchiveNotFoundException">
        /// The archive does not exist (HTTP 404/410) -- typically a bad season year.
        /// </exception>
        /// <exception cref="RetrosheetArchiveUnavailableException">
        /// A request timeout, a transport fault, or any other non-success response -- retrying
        /// may succeed.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// The response succeeded but its body is not a zip archive.
        /// </exception>
        Task<string> DownloadAsync(Uri archiveUrl, string targetDirectory, CancellationToken cancellationToken = default);
    }
}
