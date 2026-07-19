using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// One fielder's involvement in retiring (or misplaying) a specific runner. <see cref="Position"/>
    /// is the raw fielder number (1-9) from the play code, not a resolved <c>PersonId</c> --
    /// see <see cref="ParsedRunnerAdvance"/>.
    /// </summary>
    public sealed class ParsedFieldingCredit
    {
        public required byte Position { get; init; }

        public required FieldingCreditType CreditType { get; init; }

        public required int Sequence { get; init; }
    }
}
