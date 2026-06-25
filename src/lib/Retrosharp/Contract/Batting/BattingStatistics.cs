using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Contract.Batting
{
    public class BattingStatistics : Batting
    {
        /// <summary>
        /// Calculates the batting average, which is the ratio of a player's hits to their at-bats.
        /// </summary>
        public float BattingAverage => AtBats > 0 ? (float)Hits / AtBats : 0f;

        /// <summary>
        /// Calculates the on-base percentage, which measures how frequently a player reaches base per plate appearance.
        /// </summary>
        public float OnBasePercentage
        {
            get
            {
                int denominator = AtBats + BaseOnBalls + HitByPitches + SacrificeFlies;
                return denominator > 0 ? (float)(Hits + BaseOnBalls + HitByPitches) / denominator : 0f;
            }
        }

        /// <summary>
        /// Calculates the slugging percentage, which measures the total number of bases a player records per at-bat.
        /// </summary>
        public float SluggingPercentage
        {
            get
            {
                int totalBases = Hits + (2 * Doubles) + (3 * Triples) + (4 * Homeruns);
                return AtBats > 0 ? (float)totalBases / AtBats : 0f;
            }
        }

        /// <summary>
        /// Calculates the On-base Plus Slugging (OPS), which is a combined measure of a player's ability to get on base and hit for power.
        /// </summary>
        public float OnBasePlusSlugging => OnBasePercentage + SluggingPercentage;

        /// <summary>
        /// Calculates the batting average on balls in play (BABIP),
        /// which measures a player's batting average on balls that are hit into the field of play,
        /// excluding home runs and strikeouts.
        /// </summary>
        public float BattingAverageOnBallsInPlay
        {
            get
            {
                int denominator = AtBats - Strikeouts - Homeruns + SacrificeFlies;
                int numerator = Hits - Homeruns;

                return denominator > 0 ? (float)numerator / denominator : 0f;
            }
        }

        /// <summary>
        /// Calculates the isolated power (ISO), which measures a player's raw power by subtracting their batting average from their slugging percentage.
        /// </summary>
        public float IsolatedPower => SluggingPercentage - BattingAverage;

        /// <summary>
        /// Calculates the total bases, which is the total number of bases a player has earned from hits.
        /// </summary>
        public int TotalBases => Hits + (2 * Doubles) + (3 * Triples) + (4 * Homeruns);
    }
}
