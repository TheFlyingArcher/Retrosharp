namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Distinguishes import failures retrying can never fix (e.g. a mistyped/missing file path)
    /// from transient ones (a dropped DB/broker connection) that NServiceBus's normal
    /// immediate/delayed recoverability policy (see Program.cs) should still retry. Used by each
    /// import saga's Start handler to fail fast on the former instead of letting the exception
    /// reach the message pipeline and get retried 3 immediate + 5 delayed times for no reason --
    /// see spec/defects.md, "Needless Retrying".
    /// </summary>
    internal static class ImportFailureClassifier
    {
        public static bool IsUnrecoverable(Exception exception) =>
            exception is FileNotFoundException or DirectoryNotFoundException;
    }
}
