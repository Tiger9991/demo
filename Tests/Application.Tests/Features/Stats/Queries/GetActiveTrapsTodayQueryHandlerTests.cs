using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Stats.Queries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Features.Stats.Queries
{
    public sealed class GetActiveTrapsTodayQueryHandlerTests
    {
        [Fact]
        public async Task Handle_returns_correct_totals_and_details_of_traps_active_today()
        {
            await using var context = CreateContext();

            var trap1 = new Trap
            {
                Id = Guid.NewGuid(),
                TrapNumber = "T1",
                TrapGroup = "GroupA",
                status = "Active",
                BatteryPercentage = 90,
                SignalStrength = 4.0f
            };
            var trap2 = new Trap
            {
                Id = Guid.NewGuid(),
                TrapNumber = "T2",
                TrapGroup = "GroupA",
                status = "Active",
                BatteryPercentage = 80,
                SignalStrength = 3.5f
            };
            var trap3 = new Trap
            {
                Id = Guid.NewGuid(),
                TrapNumber = "T3",
                TrapGroup = "GroupB",
                status = "Active",
                BatteryPercentage = 70,
                SignalStrength = 5.0f
            };

            context.Traps.AddRange(trap1, trap2, trap3);

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Add CaptureEvents today for T1 and T3
            context.CaptureEvents.AddRange(
                new CaptureEvent
                {
                    TrapId = trap1.Id,
                    CaptureTime = today.AddHours(2),
                    RodentWeight = new Domain.ValueObjects.RodentWeight(10),
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap1.Id,
                    CaptureTime = today.AddHours(4),
                    RodentWeight = new Domain.ValueObjects.RodentWeight(12),
                    Status = "Active"
                },
                new CaptureEvent
                {
                    TrapId = trap3.Id,
                    CaptureTime = today.AddHours(3),
                    RodentWeight = new Domain.ValueObjects.RodentWeight(15),
                    Status = "Active"
                },
                // Capture event yesterday for T2
                new CaptureEvent
                {
                    TrapId = trap2.Id,
                    CaptureTime = yesterday.AddHours(5),
                    RodentWeight = new Domain.ValueObjects.RodentWeight(14),
                    Status = "Active"
                }
            );

            await context.SaveChangesAsync();

            var handler = new GetActiveTrapsTodayQueryHandler(context);

            // 1. Query for today (T1 and T3 should be active)
            var result = await handler.Handle(new GetActiveTrapsTodayQuery(Date: today), CancellationToken.None);

            Assert.Equal(2, result.TotalActiveTrapsCount);
            Assert.Equal(2, result.ActiveTrapsDetails.Count);

            var t1Detail = result.ActiveTrapsDetails.Single(d => d.TrapNumber == "T1");
            Assert.Equal("GroupA", t1Detail.TrapGroup);
            Assert.Equal(90, t1Detail.BatteryPercentage);
            Assert.Equal(4.0f, t1Detail.SignalStrength);
            Assert.Equal(2, t1Detail.TotalCapturesToday);
            Assert.Equal(today.AddHours(4), t1Detail.LastCaptureTime);

            var t3Detail = result.ActiveTrapsDetails.Single(d => d.TrapNumber == "T3");
            Assert.Equal("GroupB", t3Detail.TrapGroup);
            Assert.Equal(70, t3Detail.BatteryPercentage);
            Assert.Equal(5.0f, t3Detail.SignalStrength);
            Assert.Equal(1, t3Detail.TotalCapturesToday);
            Assert.Equal(today.AddHours(3), t3Detail.LastCaptureTime);

            // 2. Query with group filter (GroupB)
            var resultGroupB = await handler.Handle(new GetActiveTrapsTodayQuery(Date: today, GroupNumber: "GroupB"), CancellationToken.None);
            Assert.Equal(1, resultGroupB.TotalActiveTrapsCount);
            Assert.Equal("T3", resultGroupB.ActiveTrapsDetails.Single().TrapNumber);
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
                modelBuilder.Entity<TrapBaitMeasurement>().HasKey(b => b.Id);
            }
        }
    }
}
