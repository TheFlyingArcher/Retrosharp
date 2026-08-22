namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Rate stats computed from <see cref="GameBattingStatistics"/>'s stored counting stats --
    /// same formulas as <see cref="Batting.BattingStatistics"/>, but referencing
    /// <see cref="GameBattingStatistics.Hit"/> (correctly named from the start, unlike
    /// <c>Batting.Hits</c>'s own history -- see spec/phase-1-build-plan.md Step 7b).
    /// </summary>
    public class TeamBattingStatistics : GameBattingStatistics
    {
        public int TotalBases => Hit + Doubles + (2 * Triples) + (3 * Homeruns);

        public float BattingAverage => AtBats > 0 ? (float)Hit / AtBats : 0f;

        public float OnBasePercentage
        {
            get
            {
                var denominator = AtBats + BaseOnBalls + HitByPitches + SacrificeFlies;
                return denominator > 0 ? (float)(Hit + BaseOnBalls + HitByPitches) / denominator : 0f;
            }
        }

        public float SluggingPercentage => AtBats > 0 ? (float)TotalBases / AtBats : 0f;

        public float OnBasePlusSlugging => OnBasePercentage + SluggingPercentage;

        public float BattingAverageOnBallsInPlay
        {
            get
            {
                var denominator = AtBats - Strikeouts - Homeruns + SacrificeFlies;
                var numerator = Hit - Homeruns;
                return denominator > 0 ? (float)numerator / denominator : 0f;
            }
        }
    }
}
