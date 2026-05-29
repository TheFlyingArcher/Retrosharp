using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a baseball league (e.g., American League, National League).
    /// </summary>
    [Table("League")]
    public class LeagueModel : DbModel
    {
        /// <summary>
        /// League code (e.g., "AL", "NL").
        /// </summary>
        [Required]
        [StringLength(3)]
        public string LeagueCode { get; set; }

        /// <summary>
        /// Full name of the league.
        /// </summary>
        [Required]
        [StringLength(24)]
        public string LeagueName { get; set; }

        /// <summary>
        /// Navigation property for franchises in this league.
        /// </summary>
        public ICollection<FranchiseModel> Franchises { get; set; } = new List<FranchiseModel>();
    }
}
