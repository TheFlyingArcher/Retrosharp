using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Retrosharp.Data.Model;

namespace Retrosharp.Data.Context
{
    internal class RetrosharpContext : DbContext, IRetrosharpContext
    {
        public DbSet<BallparkModel> Ballparks { get; set; }
        public DbSet<BattingModel> Batting { get; set; }
        public DbSet<FranchiseModel> Franchises { get; set; }
        public DbSet<GameModel> Games { get; set; }
        public DbSet<GameLineupModel> GameLineups { get; set; }
        public DbSet<GameStatisticsModel> GameStatistics { get; set; }
        public DbSet<LeagueModel> Leagues { get; set; }
        public DbSet<PitchingModel> Pitching { get; set; }
        public DbSet<PersonModel> People { get; set; }
    }
}
