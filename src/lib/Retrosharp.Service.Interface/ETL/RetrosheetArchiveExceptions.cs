namespace Retrosharp.Service.Interface.ETL
{
    /// <summary>
    /// The requested Retrosheet archive does not exist (HTTP 404) -- typically a season
    /// Retrosheet has published no data for. Retrying can never fix it, so
    /// <c>ImportFailureClassifier</c> treats it as unrecoverable and it is routed straight to
    /// the error queue (game log) or recorded as a <c>Failed</c> run (bulk game event). See
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    public sealed class RetrosheetArchiveNotFoundException : Exception
    {
        public RetrosheetArchiveNotFoundException(string message)
            : base(message)
        {
        }

        public RetrosheetArchiveNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// A Retrosheet archive download failed for a transient reason -- an HTTP 408/429/5xx
    /// response, a request timeout, or a network fault. Deliberately NOT in
    /// <c>ImportFailureClassifier</c>'s unrecoverable set, so the normal immediate/delayed
    /// retry ladder applies and a later attempt re-downloads. See
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    public sealed class RetrosheetArchiveUnavailableException : Exception
    {
        public RetrosheetArchiveUnavailableException(string message)
            : base(message)
        {
        }

        public RetrosheetArchiveUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
