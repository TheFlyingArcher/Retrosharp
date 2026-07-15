using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// The Retrosheet adjustment record type represented by a <see cref="GameAdjustment"/> row.
    /// </summary>
    public enum GameAdjustmentType
    {
        /// <summary>
        /// badj - non-standard batting handedness for this game.
        /// </summary>
        BattingHandedness,

        /// <summary>
        /// padj - non-standard pitching handedness for this game.
        /// </summary>
        PitchingHandedness,

        /// <summary>
        /// ladj - batting out of order.
        /// </summary>
        LineupPosition,

        /// <summary>
        /// radj - extra-inning runner placement.
        /// </summary>
        RunnerPlacement,

        /// <summary>
        /// presadj - pitcher responsibility adjustment for inherited runners.
        /// </summary>
        PitcherResponsibility
    }
}
