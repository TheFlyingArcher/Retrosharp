using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Categorized outcome of a single play. Represents the primary result of the play,
    /// independent of batted-ball trajectory (see <see cref="BattedBallType"/>).
    /// </summary>
    public enum GameEventType
    {
        Single,
        Double,
        Triple,
        HomeRun,
        Walk,
        IntentionalWalk,
        HitByPitch,
        Strikeout,
        GroundOut,
        FlyOut,
        Error,
        FieldersChoice,
        StolenBase,
        CaughtStealing,
        WildPitch,
        PassedBall,
        Balk,
        Pickoff,
        PickoffCaughtStealing,
        CatcherInterference,
        NoPlay,

        /// <summary>
        /// DI -- the defense makes no attempt to retire a runner taking an extra base.
        /// </summary>
        DefensiveIndifference,

        /// <summary>
        /// OA -- a baserunning advance not otherwise classified by one of the other event types.
        /// </summary>
        OtherAdvance,

        /// <summary>
        /// "FLE$" -- a foul ball dropped for an error. Distinct from <see cref="Error"/>
        /// (a genuine reached-on-error play): per Retrosheet's own "FLE$ Error on foul fly
        /// ball" definition and PlayCodeParser's handling, the batter never becomes a runner
        /// and the plate appearance continues -- unlike <see cref="Error"/>, which always ends
        /// it. Kept as its own value (appended, not inserted, so existing persisted rows'
        /// integer values are unaffected) specifically so
        /// GameStatisticsResolver.PlateAppearanceEndingEvents can exclude it by construction
        /// instead of needing to infer "was this Error actually a foul ball" after the fact.
        /// See spec/defects.md, "PlateAppearances/AtBats overcounted on a foul ball dropped for
        /// an error."
        /// </summary>
        FoulBallError
    }
}
