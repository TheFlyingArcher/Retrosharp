using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a baseball stadium/ballpark.
    /// </summary>
    [Table("Ballpark")]
    public class BallparkModel : DbModel
    {
        /// <summary>
        /// Site code identifier for the ballpark.
        /// </summary>
        [Required]
        [StringLength(6)]
        public string SiteCode { get; set; }

        /// <summary>
        /// Official name of the ballpark. Not always known for older, minor sites.
        /// </summary>
        [StringLength(64)]
        public string? ParkName { get; set; }

        /// <summary>
        /// City where the ballpark is located.
        /// </summary>
        [Required]
        [StringLength(32)]
        public string City { get; set; }

        /// <summary>
        /// State, province, or country where the ballpark is located.
        /// </summary>
        [StringLength(32)]
        public string? StateProvinceCountry { get; set; }

        /// <summary>
        /// Date of the first game played at this ballpark. Not always known — Retrosheet's
        /// ballpark data leaves this blank for many sites, particularly older or minor ones.
        /// </summary>
        public DateTime? FirstGame { get; set; }

        /// <summary>
        /// Date of the last game played at this ballpark (if no longer in use).
        /// </summary>
        public DateTime? LastGame { get; set; }

        /// <summary>
        /// Navigation property for games played at this ballpark.
        /// </summary>
        public ICollection<GameModel> Games { get; set; } = new List<GameModel>();
    }
}
