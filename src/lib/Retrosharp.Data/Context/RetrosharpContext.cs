using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Data.Model;

namespace Retrosharp.Data.Context
{
    public class RetrosharpContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BallparkModel>();
            modelBuilder.Entity<BattingModel>();
            modelBuilder.Entity<FranchiseModel>();
            modelBuilder.Entity<GameModel>();
            modelBuilder.Entity<GameLineupModel>();
            modelBuilder.Entity<GameStatisticsModel>();
            modelBuilder.Entity<LeagueModel>();
            modelBuilder.Entity<PersonModel>();
            modelBuilder.Entity<PitchingModel>();
        }
    }
}
