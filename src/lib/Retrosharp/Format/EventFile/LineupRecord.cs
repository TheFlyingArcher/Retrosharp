namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// The shared shape of Retrosheet's "start" and "sub" records -- a player occupying a
    /// batting-order slot and/or a field position for one team. "start" records seed a game's
    /// initial lineup; "sub" records (position player substitution, pinch hitter, pinch
    /// runner, or pitching change) update it mid-game. <see cref="BattingOrder"/> is 0 when the
    /// player has no batting-order slot (for example, a relief pitcher in a DH game).
    /// </summary>
    public abstract class LineupRecord : EventFileRecord
    {
        public required string RetrosheetId { get; init; }

        public required string Name { get; init; }

        public required bool IsHomeTeam { get; init; }

        public required byte BattingOrder { get; init; }

        /// <summary>
        /// Retrosheet's numeric position code: 1-9 are the standard fielding positions,
        /// 10 is designated hitter, 11 is pinch hitter, 12 is pinch runner.
        /// </summary>
        public required byte Position { get; init; }
    }
}
