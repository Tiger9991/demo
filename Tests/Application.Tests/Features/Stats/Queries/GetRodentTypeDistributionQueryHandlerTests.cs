using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Stats.Queries;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Features.Stats.Queries
{
    public sealed class GetRodentTypeDistributionQueryHandlerTests
    {
        [Fact]
        public async Task Handle_returns_consecutive_types_with_zeros_when_no_rodents()
        {
            await using var context = CreateContext();

            var handler = new GetRodentTypeDistributionQueryHandler(context);
            var result = await handler.Handle(new GetRodentTypeDistributionQuery(), CancellationToken.None);

            Assert.Equal(4, result.Count);
            
            Assert.Equal("House mouse", result[0].RodentType);
            Assert.Equal(0, result[0].Count);
            Assert.Equal(0.0, result[0].Percentage);

            Assert.Equal("Climbing rat", result[1].RodentType);
            Assert.Equal(0, result[1].Count);
            Assert.Equal(0.0, result[1].Percentage);

            Assert.Equal("Norwegian rat", result[2].RodentType);
            Assert.Equal(0, result[2].Count);
            Assert.Equal(0.0, result[2].Percentage);

            Assert.Equal("Unknown", result[3].RodentType);
            Assert.Equal(0, result[3].Count);
            Assert.Equal(0.0, result[3].Percentage);
        }

        [Fact]
        public async Task Handle_returns_all_types_with_ratios_when_data_is_present()
        {
            await using var context = CreateContext();

            var trap = new Trap
            {
                Id = Guid.NewGuid(),
                TrapNumber = "T1",
                TrapGroup = "GroupA",
                status = "Active"
            };
            context.Traps.Add(trap);

            // Add capture events: 3 NormalRat, 1 ClimbingRat, 0 NorwegianRat, 1 Unknown (Total = 5)
            context.CaptureEvents.AddRange(
                new CaptureEvent
                {
                    TrapId = trap.Id,
                    RodentWeight = new Domain.ValueObjects.RodentWeight(20),
                    RodentLength = new Domain.ValueObjects.RodentLength { Centimeters = 8 },
                    RodentType = RodentType.NormalRat,
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap.Id,
                    RodentWeight = new Domain.ValueObjects.RodentWeight(25),
                    RodentLength = new Domain.ValueObjects.RodentLength { Centimeters = 9 },
                    RodentType = RodentType.NormalRat,
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap.Id,
                    RodentWeight = new Domain.ValueObjects.RodentWeight(18),
                    RodentLength = new Domain.ValueObjects.RodentLength { Centimeters = 8 },
                    RodentType = RodentType.NormalRat,
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap.Id,
                    RodentWeight = new Domain.ValueObjects.RodentWeight(200),
                    RodentLength = new Domain.ValueObjects.RodentLength { Centimeters = 19 },
                    RodentType = RodentType.ClimbingRat,
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap.Id,
                    RodentWeight = new Domain.ValueObjects.RodentWeight(100),
                    RodentLength = new Domain.ValueObjects.RodentLength { Centimeters = 50 },
                    RodentType = RodentType.Unknown,
                    Status = "Active"
                }
            );

            await context.SaveChangesAsync();

            var handler = new GetRodentTypeDistributionQueryHandler(context);
            var result = await handler.Handle(new GetRodentTypeDistributionQuery(), CancellationToken.None);

            // Total = 5
            // NormalRat (House mouse) = 3 (60%)
            // ClimbingRat (Climbing rat) = 1 (20%)
            // Unknown = 1 (20%)
            // NorwegianRat (Norwegian rat) = 0 (0%)
            
            Assert.Equal(4, result.Count);

            // Verify they are sorted by descending percentage
            Assert.Equal("House mouse", result[0].RodentType);
            Assert.Equal(3, result[0].Count);
            Assert.Equal(60.0, result[0].Percentage);

            // Since climbing rat and unknown have the same percentage (20%), their order depends on execution but they both should be next
            var middleTypes = result.Skip(1).Take(2).ToList();
            Assert.Contains(middleTypes, d => d.RodentType == "Climbing rat" && d.Count == 1 && d.Percentage == 20.0);
            Assert.Contains(middleTypes, d => d.RodentType == "Unknown" && d.Count == 1 && d.Percentage == 20.0);

            // Norwegian rat is 0% and should be last
            Assert.Equal("Norwegian rat", result[3].RodentType);
            Assert.Equal(0, result[3].Count);
            Assert.Equal(0.0, result[3].Percentage);
        }

        private static TestApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        private sealed class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
            : DbContext(options), IApplicationDbContext
        {
            public DbSet<Trap> Traps => Set<Trap>();
            public DbSet<CaptureEvent> CaptureEvents => Set<CaptureEvent>();
            public DbSet<BaitMeasurement> BaitMeasurements => Set<BaitMeasurement>();
            public DbSet<Domain.Entities.Customers> Customers => Set<Domain.Entities.Customers>();
            public DbSet<TrapGroups> TrapGroups => Set<TrapGroups>();
            

            public DbSet<TrapBaitMeasurement> TrapBaitMeasurement => Set<TrapBaitMeasurement>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Trap>().HasKey(t => t.Id);
                modelBuilder.Entity<CaptureEvent>().HasKey(c => c.Id);
                modelBuilder.Entity<CaptureEvent>().OwnsOne(c => c.RodentLength);
                modelBuilder.Entity<CaptureEvent>().OwnsOne(c => c.RodentWeight);
                modelBuilder.Entity<BaitMeasurement>().HasKey(b => b.Id);
                modelBuilder.Entity<Domain.Entities.Customers>().HasKey(c => c.Id);
                modelBuilder.Entity<TrapGroups>().HasKey(g => g.Id);
            }
        }
    }
}
