using Mapster;
using Microsoft.AspNetCore.Mvc;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Data Viewing endpoints for games: search, summary, and play-by-play. See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private const int DefaultLimit = 25;
        private const int MaxLimit = 100;

        private readonly IGameService _gameService;
        private readonly IGameSummaryService _gameSummaryService;
        private readonly IGamePlayByPlayService _gamePlayByPlayService;
        private readonly ITeamService _teamService;

        public GamesController(
            IGameService gameService,
            IGameSummaryService gameSummaryService,
            IGamePlayByPlayService gamePlayByPlayService,
            ITeamService teamService)
        {
            _gameService = gameService;
            _gameSummaryService = gameSummaryService;
            _gamePlayByPlayService = gamePlayByPlayService;
            _teamService = teamService;
        }

        /// <summary>
        /// Searches games by date, season, and/or participating franchise.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<GameSearchResult>>> Search(
            [FromQuery] DateTime? date,
            [FromQuery] short? season,
            [FromQuery] int? franchiseId,
            [FromQuery] int limit = DefaultLimit,
            [FromQuery] int offset = 0)
        {
            if (limit <= 0 || limit > MaxLimit)
                return BadRequest($"limit must be between 1 and {MaxLimit}.");

            if (offset < 0)
                return BadRequest("offset must be non-negative.");

            var (games, totalCount) = await _gameService.SearchAsync(date, season, franchiseId, limit, offset);

            var franchiseCache = new Dictionary<int, string>();
            async Task<string> ResolveFranchiseCodeAsync(int id)
            {
                if (!franchiseCache.TryGetValue(id, out var code))
                {
                    var franchise = await _teamService.GetByIdAsync(id);
                    code = franchise?.FranchiseCode ?? string.Empty;
                    franchiseCache[id] = code;
                }

                return code;
            }

            var items = new List<GameSearchResult>();
            foreach (var game in games)
            {
                var result = game.Adapt<GameSearchResult>();
                result.HomeFranchiseCode = await ResolveFranchiseCodeAsync(game.HomeFranchiseId);
                result.VisitorFranchiseCode = await ResolveFranchiseCodeAsync(game.VisitorFranchiseId);
                items.Add(result);
            }

            return new PagedResult<GameSearchResult>
            {
                Items = items,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Gets a game's summary: final score, both teams' box-score totals, both starting
        /// lineups, decisions, umpires, and ballpark.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameSummaryResponse>> GetById(int id)
        {
            var summary = await _gameSummaryService.GetSummaryAsync(id);
            if (summary == null)
                return NotFound();

            return new GameSummaryResponse
            {
                Id = summary.Id,
                GameDate = summary.GameDate,
                GameNumber = summary.GameNumber,
                GameDayNight = summary.GameDayNight,
                GameLengthMinutes = summary.GameLengthMinutes,
                ParkAttendance = summary.ParkAttendance,
                GameNotes = summary.GameNotes,
                BallparkName = summary.Ballpark?.ParkName,
                BallparkCity = summary.Ballpark?.City,
                Home = summary.HomeTeam.Adapt<GameTeamBoxScoreResponse>(),
                Visitor = summary.VisitorTeam.Adapt<GameTeamBoxScoreResponse>(),
                HomeLineup = summary.HomeLineup.Adapt<IEnumerable<GameLineupEntryResponse>>(),
                VisitorLineup = summary.VisitorLineup.Adapt<IEnumerable<GameLineupEntryResponse>>(),
                WinningPitcher = summary.WinningPitcher?.Adapt<PlayerSearchResult>(),
                LosingPitcher = summary.LosingPitcher?.Adapt<PlayerSearchResult>(),
                SavingPitcher = summary.SavingPitcher?.Adapt<PlayerSearchResult>(),
                GameWinningBatter = summary.GameWinningBatter?.Adapt<PlayerSearchResult>(),
                UmpireHome = summary.UmpireHome?.Adapt<PlayerSearchResult>(),
                UmpireFirst = summary.UmpireFirst?.Adapt<PlayerSearchResult>(),
                UmpireSecond = summary.UmpireSecond?.Adapt<PlayerSearchResult>(),
                UmpireThird = summary.UmpireThird?.Adapt<PlayerSearchResult>(),
                UmpireLeft = summary.UmpireLeft?.Adapt<PlayerSearchResult>(),
                UmpireRight = summary.UmpireRight?.Adapt<PlayerSearchResult>()
            };
        }

        /// <summary>
        /// Gets a game's full play-by-play: GameEvent rows (with nested runners and fielding
        /// credits) interleaved with GameSubstitution/GameAdjustment/GameComment context
        /// records, in true chronological order.
        /// </summary>
        [HttpGet("{id}/events")]
        public async Task<ActionResult<GamePlayByPlayResponse>> GetEvents(int id)
        {
            if (await _gameService.GetByIdAsync(id) == null)
                return NotFound();

            var (entries, people) = await _gamePlayByPlayService.GetPlayByPlayAsync(id);

            return new GamePlayByPlayResponse
            {
                People = people.ToDictionary(kv => kv.Key, kv => kv.Value.Adapt<PlayerSearchResult>()),
                // GameEventPlayRecord.Event is nested (Event/Runners), unlike GamePlayResponse's
                // flat shape, so a blind Adapt<>() leaves every field at its default -- the same
                // class of bug as Step 7e's Hit/Hits mismatch, just a structural mismatch instead
                // of a name mismatch. Built explicitly here instead.
                Events = entries.Select(e => new GamePlayByPlayEntryResponse
                {
                    RecordIndex = e.RecordIndex,
                    Play = e.Play == null ? null : new GamePlayResponse
                    {
                        Inning = e.Play.Event.Inning,
                        TeamAtBat = e.Play.Event.TeamAtBat,
                        BatterId = e.Play.Event.BatterId,
                        PitcherId = e.Play.Event.PitcherId,
                        Balls = e.Play.Event.Balls,
                        Strikes = e.Play.Event.Strikes,
                        FoulBallsWithTwoStrikes = e.Play.Event.FoulBallsWithTwoStrikes,
                        PitchSequence = e.Play.Event.PitchSequence,
                        RawEventText = e.Play.Event.RawEventText,
                        EventType = e.Play.Event.EventType,
                        SecondaryEventType = e.Play.Event.SecondaryEventType,
                        BattedBallType = e.Play.Event.BattedBallType,
                        IsSacHit = e.Play.Event.IsSacHit,
                        IsSacFly = e.Play.Event.IsSacFly,
                        Runners = e.Play.Runners.Select(r => new GameRunnerResponse
                        {
                            PersonId = r.Runner.PersonId,
                            StartBase = r.Runner.StartBase,
                            EndBase = r.Runner.EndBase,
                            IsOut = r.Runner.IsOut,
                            IsRBI = r.Runner.IsRBI,
                            IsEarnedRun = r.Runner.IsEarnedRun,
                            ResponsiblePitcherId = r.Runner.ResponsiblePitcherId,
                            FieldingCredits = r.FieldingCredits.Adapt<IEnumerable<GameFieldingCreditResponse>>()
                        }).ToList()
                    },
                    Substitution = e.Substitution?.Adapt<GameSubstitutionResponse>(),
                    Adjustment = e.Adjustment?.Adapt<GameAdjustmentResponse>(),
                    Comment = e.Comment?.Adapt<GameCommentResponse>()
                }).ToList()
            };
        }
    }
}
