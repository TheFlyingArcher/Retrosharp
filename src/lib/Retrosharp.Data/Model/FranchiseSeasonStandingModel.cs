using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// One franchise's precomputed regular-season standing for one season.
    /// </summary>
    [Table("FranchiseSeasonStanding")]
    public class FranchiseSeasonStandingModel : DbModel
    {
        [ForeignKey("Franchise")]
        [Required]
        public int FranchiseId { get; set; }

        [Required]
        public short SeasonYear { get; set; }

        [Required]
        public short Wins { get; set; }

        [Required]
        public short Losses { get; set; }

        [Required]
        public short Ties { get; set; }

        [Required]
        public byte Rank { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal GamesBehind { get; set; }

        [Required]
        public bool DivisionChampion { get; set; }

        [Required]
        public bool LeagueBestRecord { get; set; }

        // Navigation Properties

        public FranchiseModel Franchise { get; set; }
    }
}
