﻿using Microsoft.EntityFrameworkCore;
using P01_HospitalDatabase.Data.Common;
using P01_HospitalDatabase.Data.Models;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;

namespace P01_HospitalDatabase.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext()
        {

        }

        public HospitalContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Visitation> Visitations { get; set; }

        public DbSet<Diagnose> Diagnoses { get; set; }

        public DbSet<Medicament> Medicaments { get; set; }

        public DbSet<PatientMedicament> PatientsMedicaments { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


            if (!optionsBuilder.IsConfigured)
            {
                // Set default connection string
                optionsBuilder.UseSqlServer(DbConfig.ConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<PatientMedicament>(entity =>
            {
                entity.HasKey(pm => new { pm.PatientId, pm.MedicamentId });
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(p => p.Email)
                    .IsUnicode(false);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
