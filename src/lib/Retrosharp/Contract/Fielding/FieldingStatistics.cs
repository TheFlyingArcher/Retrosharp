namespace Retrosharp.Contract.Fielding
{
    /// <summary>
    /// Rate stats computed from <see cref="Fielding"/>'s stored counting stats.
    /// </summary>
    public class FieldingStatistics : Fielding
    {
        /// <summary>
        /// Fielding percentage: the share of fielding chances (putouts, assists, and errors)
        /// that did not result in an error. Null putouts/assists/errors are treated as zero.
        /// </summary>
        public float FieldingPercentage
        {
            get
            {
                var putouts = Putouts ?? 0;
                var assists = Assists ?? 0;
                var errors = Errors ?? 0;
                var chances = putouts + assists + errors;

                return chances > 0 ? (float)(putouts + assists) / chances : 0f;
            }
        }
    }
}
