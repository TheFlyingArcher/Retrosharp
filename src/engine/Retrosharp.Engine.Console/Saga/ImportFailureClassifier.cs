using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Distinguishes import failures retrying can never fix from transient ones (a dropped
    /// DB/broker connection) that NServiceBus's normal immediate/delayed recoverability policy
    /// (see Program.cs) should still retry. Used by each import saga's Start handler to fail
    /// fast on the former instead of letting the exception reach the message pipeline and get
    /// retried 3 immediate + 5 delayed times for no reason -- see spec/defects.md,
    /// "Needless Retrying".
    ///
    /// FileNotFoundException/DirectoryNotFoundException: a mistyped/missing file path.
    /// InvalidOperationException: every throw site in this codebase (GameLogImportService,
    /// GameEventImportService, GameEventResolver, GameContextResolver, PersonImportService,
    /// BaseRepository) is a deterministic "no matching franchise/game/person/lineup slot for
    /// this input" condition -- the same input will fail identically on every retry. The one
    /// InvalidOperationException in the codebase that ISN'T one of these,
    /// FailingPingMessageHandler's deliberate diagnostic failure, is unaffected: it's a plain
    /// message handler, not one of these three sagas, so this classifier is never consulted for
    /// it.
    /// PlayCodeParseException: a Retrosheet play code this parser doesn't recognize -- the same
    /// file will produce the same unparseable code on every retry.
    /// </summary>
    internal static class ImportFailureClassifier
    {
        public static bool IsUnrecoverable(Exception exception) =>
            exception is FileNotFoundException or DirectoryNotFoundException
                or InvalidOperationException or PlayCodeParseException;
    }
}
