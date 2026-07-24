namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Rate stats computed from <see cref="GameFieldingStatistics"/>'s stored counting stats.
    /// </summary>
    public class TeamFieldingStatistics : GameFieldingStatistics
    {
        public float FieldingPercentage
        {
            get
            {
                var chances = Putouts + Assists + Errors;
                return chances > 0 ? (float)(Putouts + Assists) / chances : 0f;
            }
        }
    }
}
