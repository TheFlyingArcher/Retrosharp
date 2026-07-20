using System;

namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// Thrown when a Retrosheet event file's record structure doesn't match what
    /// <see cref="EventFileReader"/> expects -- an unrecognized record-type token, a missing
    /// required field, or an "id" record with no matching "info" data. Mirrors
    /// <see cref="PlayByPlay.PlayCodeParseException"/>'s "don't guess" convention.
    /// </summary>
    public sealed class EventFileParseException : Exception
    {
        public string FilePath { get; }

        public EventFileParseException(string filePath, string message) : base($"{message} (file: '{filePath}')")
        {
            FilePath = filePath;
        }
    }
}
