using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Retrosharp.Data.Model
{
    [Table("Fielding")]
    public class FieldingModel : DbModel
    {
        /// <summary>
        /// Foreign key to the person (player).
        /// </summary>
        [ForeignKey("Person"), Required]
        public int PersonId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        [ForeignKey("Franchise"), Required]
        public int FranchiseId { get; set; }

        /// <summary>
        /// Season year for these statistics.
        /// </summary>
        public short? SeasonYear { get; set; }

        /// <summary>
        /// Total putouts. This represents the number of times a player successfully made an out by catching a batted ball, tagging a runner, or being involved in a force out.
        /// </summary>
        public int? Putouts { get; set; }

        /// <summary>
        /// Total assists. This represents the number of times a player helped to make an out by throwing to another base.
        /// </summary>
        public int? Assists { get; set; }

        /// <summary>
        /// Total errors. This represents the number of times a player made a mistake that allowed a runner to advance or a batter to reach base when they should have been out.
        /// </summary>
        public int? Errors { get; set; }

        /// <summary>
        /// Total passed balls. This represents the number of times a catcher failed to catch a pitch that should have been caught, allowing a runner to advance.
        /// This statistic is specific to catchers and indicates missed opportunities to prevent runners from advancing.
        /// </summary>
        public int? PassedBalls { get; set; }

        /// <summary>
        /// Total double plays. This represents the number of times a player was involved in making two outs on the same play.
        /// </summary>
        public int? DoublePlays { get; set; }

        /// <summary>
        /// Total triple plays. This represents the number of times a player was involved in making three outs on the same play.
        /// </summary>
        public int? TriplePlays { get; set; }

        /// <summary>
        /// Navigation property to the related FranchiseModel entity.
        /// </summary>
        public FranchiseModel Franchise { get; set; }

        /// <summary>
        /// Navigation property to the related PersonModel entity.
        /// </summary>
        public PersonModel Person { get; set; }
    }
}
