using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public interface IRetrosharpContext
    {
        DbSet<BallparkModel> Ballparks { get; set; }

        DbSet<BattingModel> Batting { get; set; }

        DbSet<FranchiseModel> Franchises { get; set; }

        DbSet<GameModel> Games { get; set; }

        DbSet<GameLineupModel> GameLineups { get; set; }

        DbSet<GameStatisticsModel> GameStatistics { get; set; }

        DbSet<LeagueModel> Leagues { get; set; }

        DbSet<PitchingModel> Pitching { get; set; }

        DbSet<PersonModel> People { get; set; }

        ChangeTracker ChangeTracker { get; }

        DatabaseFacade Database { get; }

        IModel Model { get; }
    }
}
