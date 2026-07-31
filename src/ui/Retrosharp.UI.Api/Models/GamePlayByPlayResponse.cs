using Retrosharp.Contract.GameEvent;

namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// A game's full play-by-play. <see cref="People"/> is a glossary of every distinct person
    /// referenced anywhere in <see cref="Events"/> (resolved once per distinct person, not
    /// embedded redundantly in every play/runner/credit) -- <see cref="Events"/>' own PersonId
    /// fields index into it. See spec/api.md, "GET /games/{gameId}/events".
    /// </summary>
    public class GamePlayByPlayResponse
    {
        public IReadOnlyDictionary<int, PlayerSearchResult> People { get; set; } = new Dictionary<int, PlayerSearchResult>();

        public IEnumerable<GamePlayByPlayEntryResponse> Events { get; set; } = Array.Empty<GamePlayByPlayEntryResponse>();
    }

    /// <summary>
    /// One interleaved play-by-play record -- exactly one of <see cref="Play"/>/
    /// <see cref="Substitution"/>/<see cref="Adjustment"/>/<see cref="Comment"/> is populated.
    /// <see cref="RecordIndex"/> (not any individual record's own, type-scoped Sequence) is what
    /// puts these in true chronological order across all four record types.
    /// </summary>
    public class GamePlayByPlayEntryResponse
    {
        public int RecordIndex { get; set; }

        public GamePlayResponse? Play { get; set; }

        public GameSubstitutionResponse? Substitution { get; set; }

        public GameAdjustmentResponse? Adjustment { get; set; }

        public GameCommentResponse? Comment { get; set; }
    }

    public class GamePlayResponse
    {
        public byte Inning { get; set; }

        /// <summary>
        /// "H" or "V" -- whether the team at bat is home or visitor.
        /// </summary>
        public string TeamAtBat { get; set; } = string.Empty;

        public int BatterId { get; set; }

        public int PitcherId { get; set; }

        public byte Balls { get; set; }

        public byte Strikes { get; set; }

        public byte FoulBallsWithTwoStrikes { get; set; }

        public string? PitchSequence { get; set; }

        public string RawEventText { get; set; } = string.Empty;

        public GameEventType EventType { get; set; }

        public GameEventType? SecondaryEventType { get; set; }

        public BattedBallType? BattedBallType { get; set; }

        public bool IsSacHit { get; set; }

        public bool IsSacFly { get; set; }

        public IEnumerable<GameRunnerResponse> Runners { get; set; } = Array.Empty<GameRunnerResponse>();
    }

    public class GameRunnerResponse
    {
        public int PersonId { get; set; }

        public BaseState StartBase { get; set; }

        public BaseState EndBase { get; set; }

        public bool IsOut { get; set; }

        public bool IsRBI { get; set; }

        public bool IsEarnedRun { get; set; }

        public int? ResponsiblePitcherId { get; set; }

        public IEnumerable<GameFieldingCreditResponse> FieldingCredits { get; set; } = Array.Empty<GameFieldingCreditResponse>();
    }

    public class GameFieldingCreditResponse
    {
        public int PersonId { get; set; }

        public FieldingCreditType CreditType { get; set; }

        /// <summary>
        /// Order of this fielder's involvement in a relay (for example, an assist before a putout).
        /// </summary>
        public int Sequence { get; set; }

        public byte Position { get; set; }
    }

    public class GameSubstitutionResponse
    {
        public int PersonId { get; set; }

        /// <summary>
        /// "H" or "V" -- whether the substitution is for the home or visitor team.
        /// </summary>
        public string TeamAtBat { get; set; } = string.Empty;

        public byte BattingOrderPosition { get; set; }

        public byte FieldingPosition { get; set; }
    }

    public class GameAdjustmentResponse
    {
        public GameAdjustmentType AdjustmentType { get; set; }

        public int PersonId { get; set; }

        public string? Value { get; set; }
    }

    public class GameCommentResponse
    {
        public string CommentText { get; set; } = string.Empty;
    }
}
