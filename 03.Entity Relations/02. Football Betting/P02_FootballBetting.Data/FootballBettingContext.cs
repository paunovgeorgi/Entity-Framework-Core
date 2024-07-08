﻿using Microsoft.EntityFrameworkCore;
using P02_FootballBetting.Data.Common;
using P02_FootballBetting.Data.Models;

namespace P02_FootballBetting.Data;
    public class FootballBettingContext : DbContext
    {
        // Use this constructor when developing the application 
        // When we test the application locally on or PC
        public FootballBettingContext()
        {

        }
        
        // Used by Judge
        // Loading of the DbContext with DI - useful in real applications
        public FootballBettingContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Team> Teams { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<Town> Towns { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<Position> Positions { get; set; }

        public DbSet<PlayerStatistic> PlayersStatistics { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Bet> Bets { get; set; }

        public DbSet<User> Users { get; set; }

        // Connection configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                // Set default connection string
                optionsBuilder.UseSqlServer(DbConfig.ConnectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

        
        // Fluent API and Entities configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        //modelBuilder.Entity<Team>(e =>
        //{
        //    e.HasKey(t => t.TeamId);

        //    e.Property(t => t.Name)
        //        .IsRequired();
        //});

        modelBuilder.Entity<PlayerStatistic>(e =>
        {
            e.HasKey(ps => new { ps.GameId, ps.PlayerId });
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity
                .HasOne(t => t.PrimaryKitColor)
                .WithMany(c => c.PrimaryKitTeams)
                .HasForeignKey(t => t.PrimaryKitColorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity
                .HasOne(t => t.SecondaryKitColor)
                .WithMany(c => c.SecondaryKitTeams)
                .HasForeignKey(t => t.SecondaryKitColorId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity
                .HasOne(g => g.HomeTeam)
                .WithMany(t => t.HomeGames)
                .HasForeignKey(g => g.HomeTeamId)
                .OnDelete(DeleteBehavior.NoAction);

            entity
                .HasOne(g => g.AwayTeam)
                .WithMany(t => t.AwayGames)
                .HasForeignKey(g => g.AwayTeamId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity
                .HasOne(p => p.Town)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TownId)
                .OnDelete(DeleteBehavior.NoAction);

        });

        base.OnModelCreating(modelBuilder);
        }
    }
