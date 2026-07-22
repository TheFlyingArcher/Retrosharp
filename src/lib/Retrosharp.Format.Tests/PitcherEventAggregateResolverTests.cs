using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="PitcherEventAggregateResolver"/> against hand-built
    /// <see cref="PitcherGameEventRecord"/> lists -- pure aggregation logic, no I/O.
    /// </summary>
    public class PitcherEventAggregateResolverTests
    {
        private const int PersonId = 20;
        private const int FranchiseId = 1;
        private const short SeasonYear = 2025;

        private static PitcherGameEventRecord Evt(
            GameEventType eventType, BattedBallType? battedBallType = null, bool isSacHit = false, bool isSacFly = false,
            int franchiseId = FranchiseId, short seasonYear = SeasonYear) =>
            new()
            {
                FranchiseId = franchiseId,
                SeasonYear = seasonYear,
                EventType = eventType,
                BattedBallType = battedBallType,
                IsSacHit = isSacHit,
                IsSacFly = isSacFly
            };

        [Fact]
        public void Resolve_HomeRun_CountsAsHomerunAllowedAndAtBat()
        {
            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, [Evt(GameEventType.HomeRun, BattedBallType.FlyBall)]);

            var agg = Assert.Single(aggregates);
            Assert.Equal(1, agg.HomerunsAllowed);
            Assert.Equal(1, agg.FlyBallsAllowed);
            Assert.Equal(1, agg.AtBatsAgainst);
            Assert.Equal(0, agg.SacrificeFliesAgainst);
        }

        [Fact]
        public void Resolve_FlyOut_CountsAsFlyBallButNotHomeRun()
        {
            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, [Evt(GameEventType.FlyOut, BattedBallType.FlyBall)]);

            var agg = Assert.Single(aggregates);
            Assert.Equal(0, agg.HomerunsAllowed);
            Assert.Equal(1, agg.FlyBallsAllowed);
            Assert.Equal(1, agg.AtBatsAgainst);
        }

        [Fact]
        public void Resolve_Walk_DoesNotCountAsAtBat()
        {
            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, [Evt(GameEventType.Walk)]);

            var agg = Assert.Single(aggregates);
            Assert.Equal(0, agg.AtBatsAgainst);
        }

        [Fact]
        public void Resolve_SacrificeFly_CountsAsSacFlyButNotAtBat()
        {
            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, [Evt(GameEventType.FlyOut, BattedBallType.FlyBall, isSacFly: true)]);

            var agg = Assert.Single(aggregates);
            Assert.Equal(1, agg.SacrificeFliesAgainst);
            Assert.Equal(0, agg.AtBatsAgainst);
            Assert.Equal(1, agg.FlyBallsAllowed);
        }

        [Fact]
        public void Resolve_GroundBallOut_DoesNotCountAsFlyBall()
        {
            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, [Evt(GameEventType.GroundOut, BattedBallType.GroundBall)]);

            var agg = Assert.Single(aggregates);
            Assert.Equal(0, agg.FlyBallsAllowed);
            Assert.Equal(1, agg.AtBatsAgainst);
        }

        [Fact]
        public void Resolve_EventsAcrossTwoFranchiseSeasons_GroupedSeparately()
        {
            var events = new[]
            {
                Evt(GameEventType.HomeRun, BattedBallType.FlyBall, franchiseId: 1, seasonYear: 2025),
                Evt(GameEventType.Single, franchiseId: 2, seasonYear: 2025)
            };

            var aggregates = PitcherEventAggregateResolver.Resolve(PersonId, events);

            Assert.Equal(2, aggregates.Count);
            Assert.Equal(1, aggregates.Single(a => a.FranchiseId == 1).HomerunsAllowed);
            Assert.Equal(0, aggregates.Single(a => a.FranchiseId == 2).HomerunsAllowed);
        }
    }
}
