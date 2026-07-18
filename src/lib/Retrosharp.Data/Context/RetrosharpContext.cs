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
        public DbSet<GameBattingStatisticsModel> GameBattingStatistics { get; set; }
        public DbSet<GamePitchingStatisticsModel> GamePitchingStatistics { get; set; }
        public DbSet<GameFieldingStatisticsModel> GameFieldingStatistics { get; set; }
        public DbSet<GameEventModel> GameEvents { get; set; }
        public DbSet<GameEventRunnerModel> GameEventRunners { get; set; }
        public DbSet<GameEventFieldingCreditModel> GameEventFieldingCredits { get; set; }
        public DbSet<GameEventGameStatusModel> GameEventGameStatuses { get; set; }
        public DbSet<GameSubstitutionModel> GameSubstitutions { get; set; }
        public DbSet<GameAdjustmentModel> GameAdjustments { get; set; }
        public DbSet<GameCommentModel> GameComments { get; set; }
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

                // Seed data: leagues are a small, effectively permanent list, so they are
                // baked directly into a migration rather than imported at runtime.
                // See spec/seed-data.md.
                entity.HasData(
                    new LeagueModel { Id = 1, LeagueCode = "AA", LeagueName = "American Association" },
                    new LeagueModel { Id = 2, LeagueCode = "AL", LeagueName = "American League" },
                    new LeagueModel { Id = 3, LeagueCode = "FL", LeagueName = "Federal League" },
                    new LeagueModel { Id = 4, LeagueCode = "NA", LeagueName = "National Association" },
                    new LeagueModel { Id = 5, LeagueCode = "NL", LeagueName = "National League" },
                    new LeagueModel { Id = 6, LeagueCode = "PL", LeagueName = "Players League" },
                    new LeagueModel { Id = 7, LeagueCode = "UA", LeagueName = "Union Association" }
                );
            });

            // Configure Franchise entity
            modelBuilder.Entity<FranchiseModel>(entity =>
            {
                // FranchiseCode is not unique on its own: Retrosheet reuses the same code
                // across consecutive eras of the same franchise (for example, the Brooklyn
                // Atlantics/Grays both used "BR3"). The natural key is the combination of the
                // franchise's stable identifier and the effective start date of that era.
                entity.HasIndex(e => e.FranchiseCode);
                entity.HasIndex(e => e.FranchiseIdentifier);
                entity.HasIndex(e => new { e.FranchiseIdentifier, e.FranchiseStart }).IsUnique();

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

                // The natural key for a game is date + game-number + matchup, not just date +
                // matchup: doubleheaders share the same date and franchises but differ only by
                // GameNumber (see spec/game-log.md, Format field 2). Enforced unique so the Game
                // Log Parser's idempotency check is guaranteed by the database, not just by
                // application-level logic.
                entity.HasIndex(e => new { e.GameDate, e.GameNumber, e.HomeFranchiseId, e.VisitorFranchiseId }).IsUnique();

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

            // Configure GameBattingStatistics entity
            modelBuilder.Entity<GameBattingStatisticsModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.FranchiseId, e.HomeVisitor });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GameBattingStatistics)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.Franchise)
                    .WithMany(f => f.GameBattingStatistics)
                    .HasForeignKey(gs => gs.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GamePitchingStatistics entity
            modelBuilder.Entity<GamePitchingStatisticsModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.FranchiseId, e.HomeVisitor });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GamePitchingStatistics)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.Franchise)
                    .WithMany(f => f.GamePitchingStatistics)
                    .HasForeignKey(gs => gs.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameFieldingStatistics entity
            modelBuilder.Entity<GameFieldingStatisticsModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.FranchiseId, e.HomeVisitor });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GameFieldingStatistics)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.Franchise)
                    .WithMany(f => f.GameFieldingStatistics)
                    .HasForeignKey(gs => gs.FranchiseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameEvent entity
            modelBuilder.Entity<GameEventModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.Sequence });

                entity.HasOne(ge => ge.Game)
                    .WithMany(g => g.GameEvents)
                    .HasForeignKey(ge => ge.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ge => ge.Batter)
                    .WithMany()
                    .HasForeignKey(ge => ge.BatterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ge => ge.Pitcher)
                    .WithMany()
                    .HasForeignKey(ge => ge.PitcherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameEventRunner entity
            // Note: relationships back to GameEvent are Restrict, not Cascade, to avoid a
            // multiple-cascade-paths conflict with GameEventFieldingCredit (see below), which
            // has foreign keys to both GameEvent and GameEventRunner.
            modelBuilder.Entity<GameEventRunnerModel>(entity =>
            {
                entity.HasIndex(e => e.GameEventId);

                entity.HasOne(ger => ger.GameEvent)
                    .WithMany()
                    .HasForeignKey(ger => ger.GameEventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ger => ger.Person)
                    .WithMany()
                    .HasForeignKey(ger => ger.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ger => ger.ResponsiblePitcher)
                    .WithMany()
                    .HasForeignKey(ger => ger.ResponsiblePitcherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameEventFieldingCredit entity
            modelBuilder.Entity<GameEventFieldingCreditModel>(entity =>
            {
                entity.HasIndex(e => e.GameEventId);
                entity.HasIndex(e => e.GameEventRunnerId);

                entity.HasOne(gefc => gefc.GameEvent)
                    .WithMany()
                    .HasForeignKey(gefc => gefc.GameEventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gefc => gefc.GameEventRunner)
                    .WithMany()
                    .HasForeignKey(gefc => gefc.GameEventRunnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gefc => gefc.Person)
                    .WithMany()
                    .HasForeignKey(gefc => gefc.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameEventGameStatus entity
            // GameId is both the primary key and the foreign key to Game, making this a
            // shared-primary-key one-to-one relationship. The atomic insert of this row is
            // what claims a game for statistics application (see spec/game-event.md).
            modelBuilder.Entity<GameEventGameStatusModel>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.HasOne(s => s.Game)
                    .WithOne(g => g.GameEventGameStatus)
                    .HasForeignKey<GameEventGameStatusModel>(s => s.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameSubstitution entity
            modelBuilder.Entity<GameSubstitutionModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.Sequence });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GameSubstitutions)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.Person)
                    .WithMany()
                    .HasForeignKey(gs => gs.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameAdjustment entity
            modelBuilder.Entity<GameAdjustmentModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.Sequence });

                entity.HasOne(ga => ga.Game)
                    .WithMany(g => g.GameAdjustments)
                    .HasForeignKey(ga => ga.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ga => ga.Person)
                    .WithMany()
                    .HasForeignKey(ga => ga.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameComment entity
            modelBuilder.Entity<GameCommentModel>(entity =>
            {
                entity.HasIndex(e => new { e.GameId, e.Sequence });

                entity.HasOne(gc => gc.Game)
                    .WithMany(g => g.GameComments)
                    .HasForeignKey(gc => gc.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
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
                entity.HasIndex(e => new { e.PersonId, e.FranchiseId, e.SeasonYear, e.Position });

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
