using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="FipConstantCalculator"/> against constructed league totals with
    /// hand-computed expected outputs -- pure logic, no I/O, and independent of how much real
    /// league data happens to be imported. See spec/api.md, "FIP includes a league-normalizing
    /// constant, computed from Retrosharp's own data".
    /// </summary>
    public class FipConstantCalculatorTests
    {
        [Fact]
        public void Calculate_KnownTotals_MatchesHandComputedConstant()
        {
            // 900 innings pitched (2700 outs), 350 team earned runs, 100 HR, 300 BB+HBP combined
            // (250 BB + 50 HBP), 700 K.
            // leagueEra = 9 * 350 / 900 = 3.5
            // leagueRawComponent = (13*100 + 3*300 - 2*700) / 900 = (1300+900-1400)/900 = 800/900 = 0.8888...
            // fipConstant = 3.5 - 0.8888... = 2.6111...
            var result = FipConstantCalculator.Calculate(
                teamEarnedRuns: 350,
                homerunsAllowed: 100,
                baseOnBalls: 250,
                hitBatsmen: 50,
                strikeouts: 700,
                inningsPitchedOuts: 2700);

            Assert.Equal(3.5, result.LeagueEra, precision: 4);
            Assert.Equal(0.8889, result.LeagueRawComponent, precision: 4);
            Assert.Equal(2.6111, result.FipConstant, precision: 4);
        }

        [Fact]
        public void Calculate_ZeroInnings_ReturnsZeroRatherThanDivideByZero()
        {
            var result = FipConstantCalculator.Calculate(0, 0, 0, 0, 0, 0);

            Assert.Equal(0, result.LeagueEra);
            Assert.Equal(0, result.LeagueRawComponent);
            Assert.Equal(0, result.FipConstant);
        }

        [Fact]
        public void Calculate_RawComponentAboveEra_ProducesNegativeConstant()
        {
            // A league that allows far more HR/BB/HBP than its ERA would suggest should still
            // produce a (negative) constant, rather than throwing or clamping.
            var result = FipConstantCalculator.Calculate(
                teamEarnedRuns: 100,
                homerunsAllowed: 500,
                baseOnBalls: 500,
                hitBatsmen: 0,
                strikeouts: 0,
                inningsPitchedOuts: 2700);

            Assert.True(result.FipConstant < 0);
        }
    }
}
