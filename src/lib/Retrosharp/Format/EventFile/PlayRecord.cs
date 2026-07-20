namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// One "play" record -- a single play. <see cref="RetrosheetId"/> is the batter, given
    /// directly by the file (unlike fielders/runners, which are only ever identified by
    /// position/base number inside <see cref="RawEventText"/> -- see
    /// spec/phase-1-build-plan.md Step 6a/6b).
    /// </summary>
    public sealed class PlayRecord : EventFileRecord
    {
        public required byte Inning { get; init; }

        /// <summary>
        /// True when the home team is batting (bottom of the inning), false when the
        /// visiting team is batting (top of the inning).
        /// </summary>
        public required bool IsHomeTeamBatting { get; init; }

        public required string RetrosheetId { get; init; }

        public required string CountField { get; init; }

        public required string PitchSequence { get; init; }

        public required string RawEventText { get; init; }
    }
}
