namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// A free-text commentary record ("com"). Recognized here but not persisted until Step 6c
    /// (GameComment).
    /// </summary>
    public sealed class ComRecord : EventFileRecord
    {
        public required string CommentText { get; init; }
    }
}
