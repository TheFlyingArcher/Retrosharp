using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// One runner's (including the batter's) movement on a play. Deliberately carries no
    /// <c>PersonId</c> -- only which base the runner started and ended on -- since knowing
    /// which physical person that is requires lineup/substitution state this parser doesn't
    /// have. See spec/game-event.md, "Data Model".
    /// </summary>
    public sealed class ParsedRunnerAdvance
    {
        public required BaseState StartBase { get; init; }

        public required BaseState EndBase { get; init; }

        public bool IsOut { get; init; }

        /// <summary>
        /// True when this runner's advance to home is credited as an RBI. Retrosheet's
        /// convention treats a scored run as an RBI by default unless annotated
        /// "(NR)"/"(NORBI)" -- see <see cref="PlayCodeParser"/>.
        /// </summary>
        public bool IsRBI { get; init; }

        /// <summary>
        /// True when this runner's run, if scored, is earned. Retrosheet's convention treats
        /// a scored run as earned by default unless annotated "(UR)".
        /// </summary>
        public bool IsEarnedRun { get; init; }

        public required IReadOnlyList<ParsedFieldingCredit> FieldingCredits { get; init; }
    }
}
