using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Stats.Queries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Features.Stats.Queries;

public sealed class GetRodentActivityDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAggregations()
    {
        await using var context = CreateContext();

        var trap = new Trap
        {
            TrapGroup = "Group1",
            TrapNumber = "Trap1",
            status = "Active",
            StartTime = DateTime.UtcNow.AddDays(-10)
        };
        context.Traps.Add(trap);

        var captureEvent1 = new CaptureEvent
        {
            TrapId = trap.Id,
            CaptureTime = DateTime.UtcNow.AddDays(-5),
            Status = "Active",
            RodentLength = new() { Centimeters = 10 },
            RodentWeight = new(50)
        };
        var captureEvent2 = new CaptureEvent
        {
            TrapId = trap.Id,
            CaptureTime = DateTime.UtcNow.AddDays(-2),
            Status = "Active",
            RodentLength = new() { Centimeters = 12 },
            RodentWeight = new(60)
        };
        context.CaptureEvents.AddRange(captureEvent1, captureEvent2);

        var bait1 = new BaitMeasurement
        {
            TrapId = trap.Id,
            CaptureEventId = captureEvent1.Id,
            MeasurementTime = DateTime.UtcNow.AddDays(-5),
            BaitWeightGrams = 20.0
        };
        var bait2 = new BaitMeasurement
        {
            TrapId = trap.Id,
            CaptureEventId = captureEvent2.Id,
            MeasurementTime = DateTime.UtcNow.AddDays(-2),
            BaitWeightGrams = 15.0
        };
        context.BaitMeasurements.AddRange(bait1, bait2);

        await context.SaveChangesAsync();

        var handler = new GetRodentActivityDetailsQueryHandler(context);

        // Test without date filter
        var result = await handler.Handle(new GetRodentActivityDetailsQuery(), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("Trap1", dto.TrapNumber);
        Assert.Equal("Group1", dto.GroupNumber);
        Assert.Equal("Active", dto.Status);
        Assert.Equal(2, dto.TotalCaptures);
        Assert.Equal(5.0, dto.TotalBaitConsumed);
    }

    [Fact]
    public async Task Handle_WithDateFilter_FiltersBaitAndCaptures()
    {
        await using var context = CreateContext();

        var trap = new Trap
        {
            TrapGroup = "Group1",
            TrapNumber = "Trap1",
            status = "Active",
            StartTime = DateTime.UtcNow.AddDays(-10)
        };
        context.Traps.Add(trap);

        var captureEvent1 = new CaptureEvent
        {
            TrapId = trap.Id,
            CaptureTime = DateTime.UtcNow.AddDays(-5),
            Status = "Active",
            RodentLength = new() { Centimeters = 10 },
            RodentWeight = new(50)
        };
        var captureEvent2 = new CaptureEvent
        {
            TrapId = trap.Id,
            CaptureTime = DateTime.UtcNow.AddDays(-2),
            Status = "Active",
            RodentLength = new() { Centimeters = 12 },
            RodentWeight = new(60)
        };
        context.CaptureEvents.AddRange(captureEvent1, captureEvent2);

        var bait1 = new BaitMeasurement
        {
            TrapId = trap.Id,
            CaptureEventId = captureEvent1.Id,
            MeasurementTime = DateTime.UtcNow.AddDays(-5),
            BaitWeightGrams = 20.0
        };
        var bait2 = new BaitMeasurement
        {
            TrapId = trap.Id,
            CaptureEventId = captureEvent2.Id,
            MeasurementTime = DateTime.UtcNow.AddDays(-2),
            BaitWeightGrams = 15.0
        };
        context.BaitMeasurements.AddRange(bait1, bait2);

        await context.SaveChangesAsync();

        var handler = new GetRodentActivityDetailsQueryHandler(context);

        // Filter: only last 3 days (should only include captureEvent2 and the interval ending in last 3 days)
        var result = await handler.Handle(new GetRodentActivityDetailsQuery(
            FromDate: DateTime.UtcNow.AddDays(-3)
        ), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("Trap1", dto.TrapNumber);
        Assert.Equal(1, dto.TotalCaptures);
        Assert.Equal(5.0, dto.TotalBaitConsumed); // If date filter applies, it should be 10.0, not 15.0!
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
