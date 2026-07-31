using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Person;
using Retrosharp.Data;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class GamePlayByPlayService : IGamePlayByPlayService
    {
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IGameSubstitutionRepository _gameSubstitutionRepository;
        private readonly IGameAdjustmentRepository _gameAdjustmentRepository;
        private readonly IGameCommentRepository _gameCommentRepository;
        private readonly IPersonRepository _personRepository;

        public GamePlayByPlayService(
            IGameEventRepository gameEventRepository,
            IGameSubstitutionRepository gameSubstitutionRepository,
            IGameAdjustmentRepository gameAdjustmentRepository,
            IGameCommentRepository gameCommentRepository,
            IPersonRepository personRepository)
        {
            _gameEventRepository = gameEventRepository;
            _gameSubstitutionRepository = gameSubstitutionRepository;
            _gameAdjustmentRepository = gameAdjustmentRepository;
            _gameCommentRepository = gameCommentRepository;
            _personRepository = personRepository;
        }

        public async Task<(IReadOnlyList<GamePlayByPlayEntry> Entries, IReadOnlyDictionary<int, Person> People)> GetPlayByPlayAsync(int gameId)
        {
            var plays = await _gameEventRepository.GetGameEventsForDisplayAsync(gameId);
            var substitutions = await _gameSubstitutionRepository.GetByGameIdAsync(gameId);
            var adjustments = await _gameAdjustmentRepository.GetByGameIdAsync(gameId);
            var comments = await _gameCommentRepository.GetByGameIdAsync(gameId);

            var entries = new List<GamePlayByPlayEntry>();
            entries.AddRange(plays.Select(p => new GamePlayByPlayEntry { RecordIndex = p.Event.RecordIndex, Play = p }));
            entries.AddRange(substitutions.Select(s => new GamePlayByPlayEntry { RecordIndex = s.RecordIndex, Substitution = s }));
            entries.AddRange(adjustments.Select(a => new GamePlayByPlayEntry { RecordIndex = a.RecordIndex, Adjustment = a }));
            entries.AddRange(comments.Select(c => new GamePlayByPlayEntry { RecordIndex = c.RecordIndex, Comment = c }));

            entries = entries.OrderBy(e => e.RecordIndex).ToList();

            var personIds = new HashSet<int>();
            foreach (var play in plays)
            {
                personIds.Add(play.Event.BatterId);
                personIds.Add(play.Event.PitcherId);
                foreach (var runner in play.Runners)
                {
                    personIds.Add(runner.Runner.PersonId);
                    if (runner.Runner.ResponsiblePitcherId.HasValue)
                        personIds.Add(runner.Runner.ResponsiblePitcherId.Value);

                    foreach (var credit in runner.FieldingCredits)
                        personIds.Add(credit.PersonId);
                }
            }

            foreach (var substitution in substitutions)
                personIds.Add(substitution.PersonId);

            foreach (var adjustment in adjustments)
                personIds.Add(adjustment.PersonId);

            var people = new Dictionary<int, Person>();
            foreach (var personId in personIds)
            {
                var person = await _personRepository.GetByIdAsync(personId);
                if (person != null)
                    people[personId] = person;
            }

            return (entries, people);
        }
    }
}
