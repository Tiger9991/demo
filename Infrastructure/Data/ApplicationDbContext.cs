using Application.Common.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Trap> Traps => Set<Trap>();
        public DbSet<CaptureEvent> CaptureEvents => Set<CaptureEvent>();
        public DbSet<BaitMeasurement> BaitMeasurements => Set<BaitMeasurement>();
        public DbSet<Customers> Customers => Set<Customers>();
        public DbSet<TrapGroups> TrapGroups => Set<TrapGroups>();

        

        public DbSet<TrapBaitMeasurement> TrapBaitMeasurement => Set<TrapBaitMeasurement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Customers
            modelBuilder.Entity<Customers>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasDefaultValueSql("NEWID()");
                entity.Property(c => c.CustomerNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(c => c.CustomerNumber).IsUnique();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.CustomerType).HasConversion<string>().HasMaxLength(20);
                entity.Property(c => c.Email).HasMaxLength(150);
                entity.Property(c => c.Phone).HasMaxLength(20);
                entity.Property(c => c.Address).HasMaxLength(250);

                // 1-to-Many: Customer → TrapGroups
                entity.HasMany(c => c.TrapGroups)
                      .WithOne(g => g.Customer)
                      .HasForeignKey(g => g.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure TrapGroup
            modelBuilder.Entity<TrapGroups>(entity =>
            {
                entity.ToTable("TrapGroups");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.TrapNumber).IsRequired().HasMaxLength(50);
                entity.Property(g => g.TrapGroup).IsRequired().HasMaxLength(50);
                entity.HasIndex(g => new { g.TrapGroup, g.TrapNumber }).IsUnique();
                entity.Property(g => g.Description).HasMaxLength(250);
            });

            modelBuilder.Entity<BaitMeasurement>(entity =>
            {
                entity.ToTable("BaitMeasurements");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.TrapId).IsRequired();
                entity.Property(b => b.CaptureEventId).IsRequired(false);
                entity.Property(b => b.MeasurementTime).IsRequired();
                entity.Property(b => b.BaitWeightGrams);

                // Relationship with Trap
                entity.HasOne(b => b.Trap)
                      .WithMany()
                      .HasForeignKey(b => b.TrapId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship with CaptureEvent
                entity.HasOne(b => b.CaptureEvent)
                      .WithMany()   // or .WithMany(ce => ce.BaitMeasurements) if you have the collection
                      .HasForeignKey(b => b.CaptureEventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrapBaitMeasurement>(entity =>
            {
                entity.ToTable("TrapBaitMeasurements");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.TrapId).IsRequired();
                entity.Property(b => b.MeasurementTime).IsRequired();
                entity.Property(b => b.BaitWeightGrams);

                // Relationship with Trap
                entity.HasOne(b => b.Trap)
                      .WithMany()
                      .HasForeignKey(b => b.TrapId)
                      .OnDelete(DeleteBehavior.Restrict);

                
            });


            // Configure CaptureEvent
            modelBuilder.Entity<CaptureEvent>(entity =>
            {
                modelBuilder.Entity<Trap>(entity =>
                {
                    entity.HasKey(t => t.Id);
                    entity.Property(t => t.Id).HasDefaultValueSql("NEWID()"); // optional
                    entity.HasIndex(t => new { t.TrapNumber, t.TrapGroup })
                          .IsUnique()
                          .HasFilter("[status] = 'Active'");
                });
                entity.ToTable("CaptureEvents");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CaptureTime).IsRequired();
                entity.Property(c => c.ActiveSensorCount);
                entity.Property(c => c.RodentType).HasConversion<string>();

                // Use value conversion for RodentWeight (stores as int)
                entity.Property(c => c.RodentWeight)
        .HasConversion(
            v => v.Grams,
            v => new RodentWeight(v))
        .HasColumnName("RodentWeightGrams");

                // Use value conversion for RodentLength (stores as intdouble)
                entity.Property(c => c.RodentLength)
                    .HasConversion(new ValueConverter<RodentLength, double>(
                        v => v.Centimeters,
                        v => new RodentLength { Centimeters = v }))
                    .HasColumnName("RodentLengthCm");
            });

            // Rest of your configuration...
        }
    }
}
