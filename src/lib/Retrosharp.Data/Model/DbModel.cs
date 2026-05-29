using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Base class for all database model entities.
    /// Provides common properties and conventions for Entity Framework Core.
    /// </summary>
    public abstract class DbModel
    {
        /// <summary>
        /// Primary key for the entity.
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}
