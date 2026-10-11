using Retrosharp.Contract.Standing;
using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    /// <summary>
    /// Hand-rolled test double for <see cref="IStandingsService"/>. Records every season it
    /// was asked to recompute, and can be set to return a count or throw.
    /// </summary>
    public class FakeStandingsService : IStandingsService
    {
        public int FranchiseCountToReturn { get; set; }
        public Exception? ExceptionToThrow { get; set; }
        public List<short> RecomputedSeasons { get; } = new();

        public Task<int> RecomputeSeasonAsync(short seasonYear)
        {
            RecomputedSeasons.Add(seasonYear);

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(FranchiseCountToReturn);
        }

        public Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear) =>
            throw new NotSupportedException();

        public Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear) =>
            throw new NotSupportedException();
    }
}
