using Microsoft.Extensions.Logging;
using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class PlayerStatisticsService : IPlayerStatisticsService
    {
        private readonly IBattingRepository _battingRepository;
        private readonly IPitchingRepository _pitchingRepository;
        private readonly IFieldingRepository _fieldingRepository;
        private readonly ILogger<PlayerStatisticsService> _logger;

        public PlayerStatisticsService(
            IBattingRepository battingRepository,
            IPitchingRepository pitchingRepository,
            IFieldingRepository fieldingRepository,
            ILogger<PlayerStatisticsService> logger)
        {
            _battingRepository = battingRepository;
            _pitchingRepository = pitchingRepository;
            _fieldingRepository = fieldingRepository;
            _logger = logger;
        }

        public async Task<BattingStatistics> CalculateCareerBattingStatsAsync(int personId)
        {
            var allBatting = await _battingRepository.GetByPersonIdAsync(personId);

            if (!allBatting.Any())
                return null;

            // Aggregate all career statistics
            var careerStats = new BattingStatistics
            {
                PersonId = personId,
                FranchiseId = 0, // Career stats across all franchises
                SeasonYear = null,
                PlateAppearances = (short)allBatting.Sum(b => b.PlateAppearances),
                AtBats = (short)allBatting.Sum(b => b.AtBats),
                Hits = (short)allBatting.Sum(b => b.Hits),
                Doubles = (short)allBatting.Sum(b => b.Doubles),
                Triples = (short)allBatting.Sum(b => b.Triples),
                Homeruns = (short)allBatting.Sum(b => b.Homeruns),
                BaseOnBalls = (short)allBatting.Sum(b => b.BaseOnBalls),
                Strikeouts = (short)allBatting.Sum(b => b.Strikeouts),
                SacrificeFlies = (short)allBatting.Sum(b => b.SacrificeFlies),
                SacrificeBunts = (short)allBatting.Sum(b => b.SacrificeBunts),
                IntentionalBb = (short)allBatting.Sum(b => b.IntentionalBb),
                HitByPitches = (short)allBatting.Sum(b => b.HitByPitches),
                StolenBases = (short)allBatting.Sum(b => b.StolenBases),
                TimesCaughtStealing = (short)allBatting.Sum(b => b.TimesCaughtStealing),
                Runs = (short)allBatting.Sum(b => b.Runs),
                GroundedIntoDoublePlay = (short)allBatting.Sum(b => b.GroundedIntoDoublePlay),
                Positions = (short)allBatting.Sum(b => b.Positions)
            };

            // Advanced statistics are calculated automatically via properties in BattingStatistics
            _logger.LogInformation("Calculated career batting stats for person {PersonId}: AVG={Avg:F3}, OBP={Obp:F3}, SLG={Slg:F3}", 
                personId, careerStats.BattingAverage, careerStats.OnBasePercentage, careerStats.SluggingPercentage);

            return careerStats;
        }

        public async Task<IEnumerable<Batting>> GetBattingStatsAsync(int personId, short? seasonYear = null)
        {
            var battingStats = await _battingRepository.GetByPersonIdAsync(personId);

            if (seasonYear.HasValue)
            {
                battingStats = battingStats.Where(b => b.SeasonYear == seasonYear.Value);
            }

            return battingStats;
        }

        public async Task<IEnumerable<Fielding>> GetFieldingStatsAsync(int personId, short? seasonYear = null)
        {
            var fieldingStats = await _fieldingRepository.GetByPersonIdAsync(personId);

            if (seasonYear.HasValue)
            {
                fieldingStats = fieldingStats.Where(f => f.SeasonYear == seasonYear.Value);
            }

            return fieldingStats;
        }

        public async Task<IEnumerable<Pitching>> GetPitchingStatsAsync(int personId, short? seasonYear = null)
        {
            var pitchingStats = await _pitchingRepository.GetByPersonIdAsync(personId);

            if (seasonYear.HasValue)
            {
                pitchingStats = pitchingStats.Where(p => p.SeasonYear == seasonYear.Value);
            }

            return pitchingStats;
        }
    }
}
