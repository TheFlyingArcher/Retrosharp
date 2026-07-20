namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// One of Retrosheet's less common adjustment records: "badj" (batting handedness),
    /// "padj" (pitching handedness), "ladj" (batting out of order), "radj" (extra-inning
    /// runner placement -- the international tiebreaker rule), or "presadj" (pitcher
    /// responsibility override for an inherited runner). "radj" must be interpreted by the
    /// Step 6b resolver (it manufactures a baserunner with no preceding plate appearance);
    /// the rest are recognized here but not acted on until Step 6c (GameAdjustment) or, for
    /// "presadj", a future refinement of Step 6b's inherited-runner tracking if real data is
    /// ever found to need it (absent from both 2025 reference files).
    /// </summary>
    public sealed class AdjustmentRecord : EventFileRecord
    {
        /// <summary>
        /// The raw record-type token: "badj", "padj", "ladj", "radj", or "presadj".
        /// </summary>
        public required string AdjustmentTypeCode { get; init; }

        public required string RetrosheetId { get; init; }

        public required string Value { get; init; }
    }
}
