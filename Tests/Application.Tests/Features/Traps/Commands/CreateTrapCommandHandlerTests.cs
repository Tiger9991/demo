using Application.Common.Interfaces;
using Application.Features.Traps.Commands;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Features.Traps.Commands;

public sealed class CreateTrapCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_new_trap_if_not_exists()
    {
        await using var context = CreateContext();
        var handler = new CreateTrapCommandHandler(context);

        var resultId = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "123", SignalStrength: 5.5f, TrapGroup: "GroupA"),
            CancellationToken.None);

        var trap = await context.Traps.SingleOrDefaultAsync(t => t.Id == resultId);
        Assert.NotNull(trap);
        Assert.Equal("123", trap.TrapNumber);
        Assert.Equal("GroupA", trap.TrapGroup);
        Assert.Equal(5.5f, trap.SignalStrength);
        Assert.Equal("Active", trap.status);
    }

    [Fact]
    public async Task Handle_updates_existing_active_trap_instead_of_duplicating()
    {
        await using var context = CreateContext();
        var originalId = Guid.NewGuid();
        context.Traps.Add(new Trap
        {
            Id = originalId,
            TrapNumber = "123",
            TrapGroup = "GroupA",
            SignalStrength = 2.0f,
            status = "Active",
            StartTime = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var handler = new CreateTrapCommandHandler(context);
        var resultId = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "123", SignalStrength: 8.0f, TrapGroup: "GroupA"),
            CancellationToken.None);

        Assert.Equal(originalId, resultId);
        var trap = await context.Traps.SingleAsync(t => t.Id == originalId);
        Assert.Equal(8.0f, trap.SignalStrength);
        Assert.Equal(100, trap.BatteryPercentage);
    }

    [Fact]
    public async Task Handle_treats_null_and_empty_trap_group_consistently_to_prevent_duplicate_active_trap()
    {
        await using var context = CreateContext();
        
        // 1. Register a trap with TrapGroup = null (which saves as string.Empty internally)
        var handler = new CreateTrapCommandHandler(context);
        var firstId = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "123", SignalStrength: 5.0f, TrapGroup: null),
            CancellationToken.None);

        // Verify the saved TrapGroup is indeed empty string
        var savedTrap = await context.Traps.SingleAsync(t => t.Id == firstId);
        Assert.Equal(string.Empty, savedTrap.TrapGroup);

        // 2. Register again with TrapGroup = null
        var secondId = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "123", SignalStrength: 9.0f, TrapGroup: null),
            CancellationToken.None);

        // They must map to the same trap entity ID, and not create a duplicate record
        Assert.Equal(firstId, secondId);
        Assert.Single(await context.Traps.ToListAsync());
    }

    [Fact]
    public async Task Handle_generates_default_cairo_coordinates_when_not_provided()
    {
        await using var context = CreateContext();
        var handler = new CreateTrapCommandHandler(context);

        var id1 = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "1", SignalStrength: 5.0f, TrapGroup: "0"),
            CancellationToken.None);

        var id2 = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "2", SignalStrength: 5.0f, TrapGroup: "1"),
            CancellationToken.None);

        var trap1 = await context.Traps.SingleAsync(t => t.Id == id1);
        var trap2 = await context.Traps.SingleAsync(t => t.Id == id2);

        // Assert coordinates are within Cairo bounding box
        Assert.NotNull(trap1.Latitude);
        Assert.NotNull(trap1.Longitude);
        Assert.InRange(trap1.Latitude.Value, 29.5, 30.5);
        Assert.InRange(trap1.Longitude.Value, 30.8, 31.8);

        Assert.NotNull(trap2.Latitude);
        Assert.NotNull(trap2.Longitude);
        Assert.InRange(trap2.Latitude.Value, 29.5, 30.5);
        Assert.InRange(trap2.Longitude.Value, 30.8, 31.8);

        // Different traps receive different coordinates
        Assert.True(trap1.Latitude != trap2.Latitude || trap1.Longitude != trap2.Longitude);
    }

    [Fact]
    public async Task Handle_uses_explicit_coordinates_when_provided()
    {
        await using var context = CreateContext();
        var handler = new CreateTrapCommandHandler(context);

        var explicitLat = 30.123456;
        var explicitLng = 31.654321;

        var id = await handler.Handle(
            new CreateTrapCommand(TrapNumber: "99", SignalStrength: 5.0f, TrapGroup: "Test", Latitude: explicitLat, Longitude: explicitLng),
            CancellationToken.None);

        var trap = await context.Traps.SingleAsync(t => t.Id == id);
        Assert.Equal(explicitLat, trap.Latitude);
        Assert.Equal(explicitLng, trap.Longitude);
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
