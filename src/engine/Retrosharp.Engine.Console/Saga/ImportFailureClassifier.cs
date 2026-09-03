using Npgsql;

using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Feeds <see cref="EngineRecoverabilityPolicy"/>: distinguishes import failures retrying
    /// can never fix (routed straight to the error queue with no retries) from transient ones
    /// that NServiceBus's normal immediate/delayed recoverability should still retry -- see
    /// spec/defects.md, "Needless Retrying".
    ///
    /// A transient database failure -- a deadlock (<c>40P01</c>), a serialization failure
    /// (<c>40001</c>), a dropped/timed-out connection -- is retryable no matter what wraps it,
    /// so it is checked first, walking the whole exception chain. EF Core and Npgsql surface a
    /// Postgres deadlock as a <see cref="System.InvalidOperationException"/> whose message is
    /// "An exception has been raised that is likely due to a transient failure"; without this
    /// the blanket InvalidOperationException rule below would classify it unrecoverable and
    /// drop the message with no retry. Found under concurrent Game Event imports
    /// (spec/stress-testing.md Step 2): files sharing Batting/Pitching/Fielding rows deadlock
    /// on the upsert, and the deadlock victim just needs to retry once the contending imports
    /// finish. <see cref="NpgsqlException.IsTransient"/> is Npgsql's own curated set;
    /// <c>PostgresException</c> derives from it and checks the SQLSTATE.
    ///
    /// Genuinely unrecoverable, everything else being equal:
    /// FileNotFoundException/DirectoryNotFoundException -- a mistyped/missing file path.
    /// InvalidOperationException -- every non-transient throw site in this codebase
    /// (GameLogImportService, GameEventImportService, GameEventResolver, GameContextResolver,
    /// PersonImportService, BaseRepository) is a deterministic "no matching
    /// franchise/game/person/lineup slot for this input" condition that fails identically on
    /// every retry. FailingPingMessageHandler's deliberate diagnostic InvalidOperationException
    /// is unaffected -- it's a plain handler, not one of the three sagas this classifier feeds.
    /// PlayCodeParseException -- a Retrosheet play code this parser doesn't recognize; the same
    /// file produces the same unparseable code on every retry.
    /// </summary>
    internal static class ImportFailureClassifier
    {
        public static bool IsUnrecoverable(Exception exception)
        {
            for (var inner = exception; inner is not null; inner = inner.InnerException)
            {
                if (inner is NpgsqlException { IsTransient: true } or TimeoutException)
                    return false;
            }

            return exception is FileNotFoundException or DirectoryNotFoundException
                or InvalidOperationException or PlayCodeParseException;
        }
    }
}
