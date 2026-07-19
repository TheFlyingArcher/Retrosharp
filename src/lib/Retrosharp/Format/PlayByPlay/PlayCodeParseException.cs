using System;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Thrown when <see cref="PlayCodeParser"/> encounters a play code it doesn't recognize,
    /// rather than silently producing a wrong-but-plausible result. Callers (the eventual
    /// Game Event saga) catch this per-play, log it, and continue with the rest of the file --
    /// per spec/parser.md's "handle errors gracefully... continuing to process the remaining
    /// events."
    /// </summary>
    public sealed class PlayCodeParseException : Exception
    {
        public string RawEventText { get; }

        public PlayCodeParseException(string rawEventText, string message)
            : base($"{message} Raw play code: '{rawEventText}'.")
        {
            RawEventText = rawEventText;
        }
    }
}
