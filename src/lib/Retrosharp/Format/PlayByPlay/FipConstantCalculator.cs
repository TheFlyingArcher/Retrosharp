namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// One league-season's FIP-normalizing constant, and the two intermediate values it was
    /// derived from (for transparency in the API response). See spec/api.md, "FIP includes a
    /// league-normalizing constant, computed from Retrosharp's own data".
    /// </summary>
    public sealed record FipConstantResult(double LeagueEra, double LeagueRawComponent, double FipConstant);

    /// <summary>
    /// Computes FIP's league-normalizing constant from league-wide totals. Pure logic, no I/O --
    /// this is what makes the constant's own correctness directly unit-testable, independent of
    /// how much real league data happens to be imported at any given time. See spec/api.md.
    /// </summary>
    public static class FipConstantCalculator
    {
        public static FipConstantResult Calculate(
            int teamEarnedRuns,
            int homerunsAllowed,
            int baseOnBalls,
            int hitBatsmen,
            int strikeouts,
            int inningsPitchedOuts)
        {
            if (inningsPitchedOuts <= 0)
                return new FipConstantResult(0, 0, 0);

            var inningsPitched = inningsPitchedOuts / 3.0;
            var leagueEra = 9 * teamEarnedRuns / inningsPitched;
            var leagueRawComponent = (13 * homerunsAllowed + 3 * (baseOnBalls + hitBatsmen) - 2 * strikeouts) / inningsPitched;

            return new FipConstantResult(leagueEra, leagueRawComponent, leagueEra - leagueRawComponent);
        }
    }
}
