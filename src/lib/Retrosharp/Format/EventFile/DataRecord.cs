namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// A post-game "data" record -- in practice, Retrosheet's official per-pitcher earned-run
    /// total ("er"). Recognized here but not used until Step 6e (earned-run reconciliation).
    /// </summary>
    public sealed class DataRecord : EventFileRecord
    {
        public required string DataType { get; init; }

        public required string RetrosheetId { get; init; }

        public required string Value { get; init; }
    }
}
