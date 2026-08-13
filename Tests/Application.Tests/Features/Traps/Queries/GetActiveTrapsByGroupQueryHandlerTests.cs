using Application.Common.Interfaces;
using Application.Features.Traps.Queries;
using Application.Settings;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Application.Tests.Features.Traps.Queries;

public sealed class GetActiveTrapsByGroupQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_active_traps_ordered_by_group_and_trap_number()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        context.TrapGroups.AddRange(
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "B", TrapNumber = "002", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "003", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId });
        var active1 = CreateTrap("B", "002", "Active");
        var active2 = CreateTrap("A", "001", "Active");
        context.Traps.AddRange(
            active1,
            CreateTrap("A", "003", "Inactive"),
            active2);
        context.TrapBaitMeasurement.AddRange(
            new TrapBaitMeasurement { TrapId = active1.Id, MeasurementTime = DateTime.UtcNow },
            new TrapBaitMeasurement { TrapId = active2.Id, MeasurementTime = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);

        var result = await handler.Handle(new GetActiveTrapsByGroupQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("A", first.TrapGroup);
                Assert.Equal("001", first.TrapNumber);
            },
            second =>
            {
                Assert.Equal("B", second.TrapGroup);
                Assert.Equal("002", second.TrapNumber);
            });
    }

    [Fact]
    public async Task Handle_filters_active_traps_by_group_when_group_input_is_sent()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        context.TrapGroups.AddRange(
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "B", TrapNumber = "002", CustomerId = customerId });
        var active1 = CreateTrap("A", "001", "Active");
        var active2 = CreateTrap("B", "002", "Active");
        context.Traps.AddRange(active1, active2);
        context.TrapBaitMeasurement.AddRange(
            new TrapBaitMeasurement { TrapId = active1.Id, MeasurementTime = DateTime.UtcNow },
            new TrapBaitMeasurement { TrapId = active2.Id, MeasurementTime = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);

        var result = await handler.Handle(new GetActiveTrapsByGroupQuery(GroupNumber: "B"), CancellationToken.None);

        var trap = Assert.Single(result);
        Assert.Equal("B", trap.TrapGroup);
        Assert.Equal("002", trap.TrapNumber);
    }

    [Fact]
    public async Task Handle_returns_offline_traps_when_status_is_inactive()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        context.TrapGroups.AddRange(
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "002", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "003", CustomerId = customerId });
        var activeTrap = CreateTrap("A", "001", "Active");
        context.Traps.AddRange(
            activeTrap,
            CreateTrap("A", "002", "Inactive"));
        context.TrapBaitMeasurement.AddRange(
            new TrapBaitMeasurement { TrapId = activeTrap.Id, MeasurementTime = DateTime.UtcNow });
        // 003 is NOT in Traps at all!
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);

        var result = await handler.Handle(new GetActiveTrapsByGroupQuery(Status: "Inactive"), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("002", result[0].TrapNumber);
        Assert.Equal("003", result[1].TrapNumber);
    }

    [Fact]
    public async Task Handle_categorizes_traps_based_on_bait_measurement_age()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        
        // Trap 001: Active, measurement is fresh (1 hour ago) -> Connected
        // Trap 002: Active, measurement is old (3 hours ago) -> Nonconnected
        // Trap 003: Active, no measurements -> Nonconnected
        
        context.TrapGroups.AddRange(
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "002", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "003", CustomerId = customerId });

        var trap1 = CreateTrap("A", "001", "Active");
        var trap2 = CreateTrap("A", "002", "Active");
        var trap3 = CreateTrap("A", "003", "Active");
        
        context.Traps.AddRange(trap1, trap2, trap3);
        
        context.TrapBaitMeasurement.AddRange(
            new TrapBaitMeasurement { TrapId = trap1.Id, MeasurementTime = DateTime.UtcNow.AddHours(-1) },
            new TrapBaitMeasurement { TrapId = trap2.Id, MeasurementTime = DateTime.UtcNow.AddHours(-3) });
            
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);

        // 1. Check Active (Connected) list
        var connectedResult = await handler.Handle(new GetActiveTrapsByGroupQuery(), CancellationToken.None);
        var connectedTrap = Assert.Single(connectedResult);
        Assert.Equal("001", connectedTrap.TrapNumber);

        // 2. Check Inactive (Nonconnected) list
        var nonConnectedResult = await handler.Handle(new GetActiveTrapsByGroupQuery(Status: "Inactive"), CancellationToken.None);
        Assert.Equal(2, nonConnectedResult.Count);
        Assert.Equal("002", nonConnectedResult[0].TrapNumber);
        Assert.Equal("003", nonConnectedResult[1].TrapNumber);
    }

    [Fact]
    public async Task Handle_categorizes_traps_based_on_capture_event_age_when_bait_measurement_is_old_or_missing()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        
        // Trap 001: Active, bait measurement is old (3 hours ago) but has fresh capture event (1 hour ago) -> Connected
        // Trap 002: Active, no bait measurement but has fresh capture event (1 hour ago) -> Connected
        // Trap 003: Active, fresh bait measurement (1 hour ago) but old capture event (3 hours ago) -> Connected
        // Trap 004: Active, old bait measurement (3 hours ago) and old capture event (3 hours ago) -> Nonconnected

        context.TrapGroups.AddRange(
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "002", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "003", CustomerId = customerId },
            new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "004", CustomerId = customerId });

        var trap1 = CreateTrap("A", "001", "Active");
        var trap2 = CreateTrap("A", "002", "Active");
        var trap3 = CreateTrap("A", "003", "Active");
        var trap4 = CreateTrap("A", "004", "Active");
        
        context.Traps.AddRange(trap1, trap2, trap3, trap4);
        
        context.TrapBaitMeasurement.AddRange(
            new TrapBaitMeasurement { TrapId = trap1.Id, MeasurementTime = DateTime.UtcNow.AddHours(-3) },
            new TrapBaitMeasurement { TrapId = trap3.Id, MeasurementTime = DateTime.UtcNow.AddHours(-1) },
            new TrapBaitMeasurement { TrapId = trap4.Id, MeasurementTime = DateTime.UtcNow.AddHours(-3) });

        context.CaptureEvents.AddRange(
            new CaptureEvent { TrapId = trap1.Id, CaptureTime = DateTime.UtcNow.AddHours(-1), RodentLength = new(), RodentWeight = new Domain.ValueObjects.RodentWeight(50.0) },
            new CaptureEvent { TrapId = trap2.Id, CaptureTime = DateTime.UtcNow.AddHours(-1), RodentLength = new(), RodentWeight = new Domain.ValueObjects.RodentWeight(50.0) },
            new CaptureEvent { TrapId = trap3.Id, CaptureTime = DateTime.UtcNow.AddHours(-3), RodentLength = new(), RodentWeight = new Domain.ValueObjects.RodentWeight(50.0) },
            new CaptureEvent { TrapId = trap4.Id, CaptureTime = DateTime.UtcNow.AddHours(-3), RodentLength = new(), RodentWeight = new Domain.ValueObjects.RodentWeight(50.0) });
            
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);

        // 1. Check Active (Connected) list
        var connectedResult = await handler.Handle(new GetActiveTrapsByGroupQuery(), CancellationToken.None);
        Assert.Equal(3, connectedResult.Count);
        Assert.Contains(connectedResult, t => t.TrapNumber == "001");
        Assert.Contains(connectedResult, t => t.TrapNumber == "002");
        Assert.Contains(connectedResult, t => t.TrapNumber == "003");

        // 2. Check Inactive (Nonconnected) list
        var nonConnectedResult = await handler.Handle(new GetActiveTrapsByGroupQuery(Status: "Inactive"), CancellationToken.None);
        var nonConnectedTrap = Assert.Single(nonConnectedResult);
        Assert.Equal("004", nonConnectedTrap.TrapNumber);
    }

    [Fact]
    public async Task Handle_returns_connected_for_new_trap_within_grace_period_and_no_measurements()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        context.TrapGroups.Add(new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId });
        
        var trap = CreateTrap("A", "001", "Active");
        trap.StartTime = DateTime.UtcNow.AddMinutes(-10); // Within default 30 min grace period
        context.Traps.Add(trap);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context, new TrapSettings { NewTrapGracePeriodHours = 0.5 });

        var result = await handler.Handle(new GetActiveTrapsByGroupQuery(), CancellationToken.None);

        var trapDto = Assert.Single(result);
        Assert.True(trapDto.IsActive);
        Assert.Equal("محطة جديدة، في انتظار أول قياس", trapDto.DisconnectReason);
    }

    [Fact]
    public async Task Handle_returns_disconnected_for_new_trap_beyond_grace_period_and_no_measurements()
    {
        await using var context = CreateContext();
        var customerId = Guid.NewGuid();
        context.TrapGroups.Add(new TrapGroups { Id = Guid.NewGuid(), TrapGroup = "A", TrapNumber = "001", CustomerId = customerId });
        
        var trap = CreateTrap("A", "001", "Active");
        trap.StartTime = DateTime.UtcNow.AddMinutes(-40); // Beyond default 30 min grace period
        context.Traps.Add(trap);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context, new TrapSettings { NewTrapGracePeriodHours = 0.5 });

        var result = await handler.Handle(new GetActiveTrapsByGroupQuery(Status: "Inactive"), CancellationToken.None);

        var trapDto = Assert.Single(result);
        Assert.False(trapDto.IsActive);
        Assert.Equal("لا يوجد نشاط مسجل (لا قياسات طعم ولا دخول قارض)", trapDto.DisconnectReason);
    }

    private static GetActiveTrapsByGroupQueryHandler CreateHandler(IApplicationDbContext context, TrapSettings settings = null)
    {
        settings ??= new TrapSettings();
        return new GetActiveTrapsByGroupQueryHandler(
            context,
            Options.Create(settings),
            NullLogger<GetActiveTrapsByGroupQueryHandler>.Instance);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static Trap CreateTrap(string group, string number, string status)
        => new()
        {
            TrapGroup = group,
            TrapNumber = number,
            status = status,
            StartTime = DateTime.UtcNow.AddDays(-1),
            BatteryPercentage = 80,
            SignalStrength = 10,
        };

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
