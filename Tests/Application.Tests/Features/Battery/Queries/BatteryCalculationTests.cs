using Application.Common.Interfaces;
using Application.Features.Battery.Queries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Features.Battery.Queries;

public sealed class BatteryCalculationTests
{
    [Fact]
    public async Task GetBatteryStatus_CalculatesCorrectBatteryPercentage()
    {
        // Arrange
        await using var context = CreateContext();
        var startTime = DateTime.UtcNow.AddHours(-10);
        var trap = new Trap
        {
            TrapNumber = "T01",
            TrapGroup = "Group A",
            status = "Active",
            StartTime = startTime,
            BatteryPercentage = 100,
            TotalTransmissions = 40,
            OperatingDays = 0
        };
        context.Traps.Add(trap);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetBatteryStatusQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetBatteryStatusQuery(trap.Id), CancellationToken.None);

        // Assert
        // Transmissions: 40 => deduction = 40 * 0.05 = 2%
        // Time: 10 hours => deduction = (10 / 24.0) * 1.85 = 0.77%
        // Expected battery: 100 - (2 + 0.77) = 97.23% => rounded to 97%
        Assert.Equal(97, result.CalculatedBatteryPercentage);
    }

    [Fact]
    public async Task CalculateBatteryFromTransmissions_CalculatesCorrectPercentageWithOverride()
    {
        // Arrange
        await using var context = CreateContext();
        var startTime = DateTime.UtcNow.AddHours(-20);
        var trap = new Trap
        {
            TrapNumber = "T02",
            TrapGroup = "Group A",
            status = "Active",
            StartTime = startTime,
            BatteryPercentage = 100,
            TotalTransmissions = 10,
            OperatingDays = 0
        };
        context.Traps.Add(trap);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CalculateBatteryFromTransmissionsQueryHandler(context);

        // Act - override transmissions count to 60
        var query = new CalculateBatteryFromTransmissionsQuery
        {
            TrapNumber = "T02",
            TransmissionsCount = 60
        };
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // Transmissions: 60 => deduction = 60 * 0.05 = 3%
        // Time: 20 hours => deduction = (20 / 24.0) * 1.85 = 1.54%
        // Expected battery: 100 - (3 + 1.54) = 95.46% => rounded to 95%
        Assert.Equal(95, result.CalculatedBatteryPercentage);
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
