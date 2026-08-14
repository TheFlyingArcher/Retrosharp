namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Result of a standings recomputation. See spec/api.md, "POST /standings/compute".
    /// </summary>
    public class StandingsComputeResult
    {
        public short SeasonYear { get; set; }

        public int FranchiseCount { get; set; }
    }
}
