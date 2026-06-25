using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Data.Model;

namespace Retrosharp.Data.Context
{
    public class RetrosharpContext : DbContext
    {
        public RetrosharpContext()
        {

        }

        public RetrosharpContext(DbContextOptions<RetrosharpContext> options) : base(options)
        {
        }

        // DbSet properties for each entity
        public DbSet<BallparkModel> Ballparks { get; set; }
        public DbSet<BattingModel> Batting { get; set; }
        public DbSet<FieldingModel> Fielding { get; set; }
        public DbSet<FranchiseModel> Franchises { get; set; }
        public DbSet<GameModel> Games { get; set; }
        public DbSet<GameLineupModel> GameLineups { get; set; }
        public DbSet<GameStatisticsModel> GameStatistics { get; set; }
        public DbSet<LeagueModel> Leagues { get; set; }
        public DbSet<PersonModel> People { get; set; }
        public DbSet<PitchingModel> Pitching { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure League entity
            modelBuilder.Entity<LeagueModel>(entity =>
            {
                entity.HasIndex(e => e.LeagueCode).IsUnique();
            });

            // Configure Franchise entity
            modelBuilder.Entity<FranchiseModel>(entity =>
            {
                entity.HasIndex(e => e.FranchiseCode).IsUnique();
                entity.HasIndex(e => e.FranchiseIdentifier);

                entity.HasOne(f => f.League)
                    .WithMany(l => l.Franchises)
                    .HasForeignKey(f => f.LeagueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Ballpark entity
            modelBuilder.Entity<BallparkModel>(entity =>
            {
                entity.HasIndex(e => e.SiteCode).IsUnique();
            });

            // Configure Person entity
            modelBuilder.Entity<PersonModel>(entity =>
            {
                entity.HasIndex(e => e.RetroSheetId).IsUnique();
                entity.HasIndex(e => e.Surname);
                entity.HasIndex(e => e.UseName);
            });

            // Configure Game entity with multiple relationships to Person and Franchise
            modelBuilder.Entity<GameModel>(entity =>
            {
                entity.HasIndex(e => e.GameDate);
                entity.HasIndex(e => new { e.GameDate, e.HomeFranchiseId, e.VisitorFranchiseId });

                // Visitor Franchise relationship
                entity.HasOne(g => g.VisitorFranchise)
                    .WithMany(f => f.VisitorGames)
                    .HasForeignKey(g => g.VisitorFranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Home Franchise relationship
                entity.HasOne(g => g.HomeFranchise)
                    .WithMany(f => f.HomeGames)
                    .HasForeignKey(g => g.HomeFranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ballpark relationship
                entity.HasOne(g => g.Ballpark)
                    .WithMany(b => b.Games)
                    .HasForeignKey(g => g.BallparkId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Manager relationships
                entity.HasOne(g => g.VisitorManager)
                    .WithMany(p => p.VisitorManagerGames)
                    .HasForeignKey(g => g.VisitorManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.HomeManager)
                    .WithMany(p => p.HomeManagerGames)
                    .HasForeignKey(g => g.HomeManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Umpire relationships
                entity.HasOne(g => g.UmpireHome)
                    .WithMany(p => p.UmpireHomeGames)
                    .HasForeignKey(g => g.UmpireHomeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UmpireFirst)
                    .WithMany(p => p.UmpireFirstGames)
                    .HasForeignKey(g => g.UmpireFirstId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UmpireSecond)
                    .WithMany(p => p.UmpireSecondGames)
                    .HasForeignKey(g => g.UmpireSecondId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UmpireThird)
                    .WithMany(p => p.UmpireThirdGames)
                    .HasForeignKey(g => g.UmpireThirdId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UmpireLeft)
                    .WithMany(p => p.UmpireLeftGames)
                    .HasForeignKey(g => g.UmpireLeftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UmpireRight)
                    .WithMany(p => p.UmpireRightGames)
                    .HasForeignKey(g => g.UmpireRightId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Pitcher relationships
                entity.HasOne(g => g.WinningPitcher)
                    .WithMany(p => p.WinningPitcherGames)
                    .HasForeignKey(g => g.WinningPitcherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.LosingPitcher)
                    .WithMany(p => p.LosingPitcherGames)
                    .HasForeignKey(g => g.LosingPitcherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.SavingPitcher)
                    .WithMany(p => p.SavingPitcherGames)
                    .HasForeignKey(g => g.SavingPitcherId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Game-winning batter relationship
                entity.HasOne(g => g.GameWinningBatter)
                    .WithMany(p => p.GameWinningBatterGames)
                    .HasForeignKey(g => g.GameWinningBatterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameLineup entity
            modelBuilder.Entity<GameLineupModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.BatterId });

                entity.HasOne(gl => gl.Game)
                    .WithMany(g => g.GameLineups)
                    .HasForeignKey(gl => gl.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gl => gl.Batter)
                    .WithMany(p => p.GameLineups)
                    .HasForeignKey(gl => gl.BatterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameStatistics entity
            modelBuilder.Entity<GameStatisticsModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.FranchiseId, e.HomeVisitor });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GameStatistics)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.Franchise)
                    .WithMany(f => f.GameStatistics)
                    .HasForeignKey(gs => gs.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Batting entity
            modelBuilder.Entity<BattingModel>(entity =>
            {
                entity.HasIndex(e => new { e.PersonId, e.FranchiseId, e.SeasonYear });

                entity.HasOne(b => b.Person)
                    .WithMany(p => p.BattingRecords)
                    .HasForeignKey(b => b.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Franchise)
                    .WithMany(f => f.BattingRecords)
                    .HasForeignKey(b => b.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Pitching entity
            modelBuilder.Entity<PitchingModel>(entity =>
            {
                entity.HasIndex(e => new { e.PersonId, e.FranchiseId, e.SeasonYear });

                entity.HasOne(p => p.Person)
                    .WithMany(person => person.PitchingRecords)
                    .HasForeignKey(p => p.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Franchise)
                    .WithMany(f => f.PitchingRecords)
                    .HasForeignKey(p => p.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Fielding entity
            modelBuilder.Entity<FieldingModel>(entity =>
            {
                entity.HasIndex(e => new { e.PersonId, e.FranchiseId, e.SeasonYear });

                entity.HasOne(f => f.Person)
                    .WithMany()
                    .HasForeignKey(f => f.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Franchise)
                    .WithMany()
                    .HasForeignKey(f => f.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
