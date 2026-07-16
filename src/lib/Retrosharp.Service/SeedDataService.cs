using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Extensions.Logging;

using Retrosharp.Contract.Ballpark;
using Retrosharp.Contract.Franchise;
using Retrosharp.Data;
using Retrosharp.Format;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    /// <summary>
    /// Populates Franchise and Ballpark from Retrosheet's static reference CSV files, matched
    /// and skipped by natural key so repeated runs are idempotent. See spec/seed-data.md.
    /// </summary>
    public class SeedDataService : ISeedDataService
    {
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IBallparkRepository _ballparkRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(
            IFranchiseRepository franchiseRepository,
            IBallparkRepository ballparkRepository,
            ILeagueRepository leagueRepository,
            ILogger<SeedDataService> logger)
        {
            _franchiseRepository = franchiseRepository;
            _ballparkRepository = ballparkRepository;
            _leagueRepository = leagueRepository;
            _logger = logger;
        }

        public async Task<SeedDataResult> SeedAsync()
        {
            var result = new SeedDataResult();

            await SeedFranchisesAsync(result);
            await SeedBallparksAsync(result);

            _logger.LogInformation(
                "Seed data: {FranchisesAdded} franchises added ({FranchisesSkipped} already present), " +
                "{BallparksAdded} ballparks added ({BallparksSkipped} already present).",
                result.FranchisesAdded, result.FranchisesSkipped,
                result.BallparksAdded, result.BallparksSkipped);

            return result;
        }

        private async Task SeedFranchisesAsync(SeedDataResult result)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "franchises.csv");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Franchise seed data file was not found at '{path}'.");

            var existing = await _franchiseRepository.GetAllAsync();
            var existingKeys = existing
                .Select(f => (f.FranchiseIdentifier, f.FranchiseStart))
                .ToHashSet();

            var leagues = await _leagueRepository.GetAllAsync();
            var leagueIdsByCode = leagues.ToDictionary(l => l.LeagueCode, l => l.Id);

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            });
            csv.Context.RegisterClassMap<FranchiseSeedRowMapping>();

            await foreach (var row in csv.GetRecordsAsync<FranchiseSeedRow>())
            {
                var key = (row.FranchiseIdentifier, row.FranchiseStart);
                if (existingKeys.Contains(key))
                {
                    result.FranchisesSkipped++;
                    continue;
                }

                if (!leagueIdsByCode.TryGetValue(row.LeagueCode, out var leagueId))
                {
                    _logger.LogWarning(
                        "Franchise {FranchiseIdentifier} (era starting {FranchiseStart:yyyy-MM-dd}) references unknown league code '{LeagueCode}'. Leaving League unset.",
                        row.FranchiseIdentifier, row.FranchiseStart, row.LeagueCode);
                    leagueId = 0;
                }

                var franchise = new Franchise
                {
                    FranchiseIdentifier = row.FranchiseIdentifier,
                    FranchiseCode = row.FranchiseCode,
                    LeagueId = leagueId == 0 ? null : leagueId,
                    DivisionCode = row.DivisionCode,
                    FranchiseLocation = row.FranchiseLocation,
                    Nickname = row.Nickname,
                    AlternateNickname = row.AlternateNickname,
                    FranchiseStart = row.FranchiseStart,
                    FranchiseEnd = row.FranchiseEnd,
                    PlayingCity = row.PlayingCity,
                    PlayingState = row.PlayingState,
                    IsActive = row.FranchiseEnd == null
                };

                await _franchiseRepository.CreateAsync(franchise);
                existingKeys.Add(key);
                result.FranchisesAdded++;
            }
        }

        private async Task SeedBallparksAsync(SeedDataResult result)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "ballparks.csv");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Ballpark seed data file was not found at '{path}'.");

            var existing = await _ballparkRepository.GetAllAsync();
            var existingSiteCodes = existing
                .Select(b => b.SiteCode)
                .ToHashSet();

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });
            csv.Context.RegisterClassMap<BallparkSeedRowMapping>();

            await foreach (var row in csv.GetRecordsAsync<BallparkSeedRow>())
            {
                if (existingSiteCodes.Contains(row.SiteCode))
                {
                    result.BallparksSkipped++;
                    continue;
                }

                var ballpark = new Ballpark
                {
                    SiteCode = row.SiteCode,
                    ParkName = row.ParkName,
                    City = row.City,
                    StateProvinceCountry = row.StateProvinceCountry,
                    FirstGame = row.FirstGame,
                    LastGame = row.LastGame
                };

                await _ballparkRepository.CreateAsync(ballpark);
                existingSiteCodes.Add(row.SiteCode);
                result.BallparksAdded++;
            }
        }
    }
}
