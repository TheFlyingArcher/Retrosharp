using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a person affected as a baserunner by a play, including the batter — a batter
    /// reaching base is simply a runner whose <see cref="StartBase"/> is the batter's box.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    [Table("GameEventRunner")]
    public class GameEventRunnerModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game event (play).
        /// </summary>
        [ForeignKey("GameEvent")]
        [Required]
        public int GameEventId { get; set; }

        /// <summary>
        /// Foreign key to the person (the runner, or the batter acting as a runner).
        /// </summary>
        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }

        /// <summary>
        /// Base the runner started the play on.
        /// </summary>
        [Required]
        public BaseState StartBase { get; set; }

        /// <summary>
        /// Base the runner was attempting to reach, or reached, as a result of the play.
        /// </summary>
        [Required]
        public BaseState EndBase { get; set; }

        /// <summary>
        /// Whether the runner was put out attempting to reach <see cref="EndBase"/>.
        /// </summary>
        [Required]
        public bool IsOut { get; set; }

        /// <summary>
        /// Whether this runner's advance to home is credited as an RBI to the batter.
        /// Sourced directly from Retrosheet's own (RBI)/(NORBI)/(NR) play-code annotations
        /// rather than independently derived by re-applying official scoring rules.
        /// </summary>
        [Required]
        public bool IsRBI { get; set; }

        /// <summary>
        /// Whether this runner's run, if scored, is earned.
        /// </summary>
        [Required]
        public bool IsEarnedRun { get; set; }

        /// <summary>
        /// Foreign key to the pitcher charged with this runner if they score
        /// (accounts for inherited runners). Null unless the runner scores.
        /// </summary>
        [ForeignKey("ResponsiblePitcher")]
        public int? ResponsiblePitcherId { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game event (play).
        /// </summary>
        public GameEventModel GameEvent { get; set; }

        /// <summary>
        /// Navigation property for the person (the runner).
        /// </summary>
        public PersonModel Person { get; set; }

        /// <summary>
        /// Navigation property for the responsible pitcher.
        /// </summary>
        public PersonModel ResponsiblePitcher { get; set; }

        /// <summary>
        /// Navigation collection for this runner's fielding credits. Required for EF Core to
        /// fix up <see cref="GameEventFieldingCreditModel.GameEventRunnerId"/> when saving a
        /// whole play graph in a single SaveChangesAsync (see
        /// GameEventRepository.BulkInsertAsync). Note: a credit's *other* required FK,
        /// GameEventId, is a separate relationship this collection does not fix up -- see the
        /// explicit assignment in GameEventRepository.BulkInsertAsync.
        /// </summary>
        public ICollection<GameEventFieldingCreditModel> FieldingCredits { get; set; } = new List<GameEventFieldingCreditModel>();
    }
}
