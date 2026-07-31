namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One interleaved record of a game's full play-by-play -- exactly one of <see cref="Play"/>,
    /// <see cref="Substitution"/>, <see cref="Adjustment"/>, or <see cref="Comment"/> is populated.
    /// <see cref="RecordIndex"/> (not any individual record's own, type-scoped Sequence) is what
    /// puts these in true chronological order across all four record types -- see
    /// <see cref="GameEvent.RecordIndex"/>. See spec/api.md, "GET /games/{gameId}/events".
    /// </summary>
    public class GamePlayByPlayEntry
    {
        public int RecordIndex { get; set; }

        public GameEventPlayRecord Play { get; set; }

        public GameSubstitution Substitution { get; set; }

        public GameAdjustment Adjustment { get; set; }

        public GameComment Comment { get; set; }
    }
}
