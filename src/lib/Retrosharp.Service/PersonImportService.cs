using Microsoft.Extensions.Logging;

using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Format;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service
{
    /// <summary>
    /// Parses Retrosheet's biofile and populates Person, matched and upserted by Retrosheet ID
    /// as a single atomic batch. See spec/person.md.
    /// </summary>
    public class PersonImportService : IPersonImportService
    {
        private readonly IRetrosheetFileService<BioFile> _bioFileService;
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<PersonImportService> _logger;

        public PersonImportService(
            IRetrosheetFileService<BioFile> bioFileService,
            IPersonRepository personRepository,
            ILogger<PersonImportService> logger)
        {
            _bioFileService = bioFileService;
            _personRepository = personRepository;
            _logger = logger;
        }

        public async Task<PersonImportResult> ImportAsync(string filePath)
        {
            var bioFiles = await _bioFileService.ParseFileAsync(filePath);
            var people = bioFiles.Select(MapToPerson);

            var (added, updated) = await _personRepository.BulkUpsertAsync(people);

            var result = new PersonImportResult
            {
                PeopleAdded = added,
                PeopleUpdated = updated
            };

            _logger.LogInformation(
                "Person import: {PeopleAdded} people added, {PeopleUpdated} people updated.",
                result.PeopleAdded, result.PeopleUpdated);

            return result;
        }

        private static Person MapToPerson(BioFile bioFile)
        {
            return new Person
            {
                RetroSheetId = bioFile.RetrosheetId,
                Surname = bioFile.LastName,
                UseName = bioFile.UseName,
                FullName = bioFile.FullName,
                BirthDate = bioFile.BirthDate,
                BirthCity = bioFile.BirthCity,
                BirthStateProvince = bioFile.BirthState,
                BirthCountry = bioFile.BirthCountry,
                DeathDate = bioFile.DeathDate,
                DeathCity = bioFile.DeathCity,
                DeathStateProvince = bioFile.DeathState,
                DeathCountry = bioFile.DeathCountry,
                Cemetery = bioFile.CemetaryName,
                CemeteryCity = bioFile.CemetaryCity,
                CemeteryStateProv = bioFile.CemetaryState,
                CemeteryCountry = bioFile.CemetaryCountry,
                CemeteryNote = bioFile.CemetaryNote,
                BirthName = bioFile.BirthName,
                AlternateName = bioFile.AlternateName,
                PlayerDebutDate = bioFile.PlayerDebut,
                PlayerLastDate = bioFile.PlayerFinalGame,
                CoachDebutDate = bioFile.CoachDebut,
                CoachLastDate = bioFile.FinalCoachGame,
                ManagerDebutDate = bioFile.ManagerDebut,
                ManagerLastDate = bioFile.FinalManagerGame,
                UmpireDebutDate = bioFile.UmpireDebut,
                UmpireLastDate = bioFile.FinalUmpireGame,
                Bats = bioFile.Bats?.ToString(),
                Throws = bioFile.Throws?.ToString(),
                Height = bioFile.Height,
                Weight = bioFile.Weight,
                IsHof = bioFile.InHallOfFame
            };
        }
    }
}
