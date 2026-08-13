using Application.Common.Interfaces;
using Application.Features.Captures.Commands;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Features.Captures.Commands
{
    public sealed class RecordCaptureCommandHandlerTests
    {
        [Fact]
        public async Task Handle_records_capture_event_successfully_and_returns_mapped_dto()
        {
            await using var context = CreateContext();
            var originalTrapId = Guid.NewGuid();
            var trap = new Trap
            {
                Id = originalTrapId,
                TrapNumber = "TRAP-01",
                TrapGroup = "GROUP-A",
                status = "Active",
                SignalStrength = 4.5f,
                TotalTransmissions = 5,
                StartTime = DateTime.UtcNow.AddDays(-2)
            };
            context.Traps.Add(trap);
            await context.SaveChangesAsync();

            var handler = new RecordCaptureCommandHandler(context);
            var command = new RecordCaptureCommand
            {
                TrapNumber = "TRAP-01",
                trapGroup = "GROUP-A",
                ir = 3,
                weight = 20.0,
                bWeight = 15.0,
                SignalStrength = 5.0f
            };

            var result = await handler.Handle(command, CancellationToken.None);

            // Assert returning "ok" on success
            Assert.Equal("ok", result);

            // Assert database updates
            var updatedTrap = await context.Traps.SingleAsync(t => t.Id == originalTrapId);
            Assert.Equal(6, updatedTrap.TotalTransmissions);
            Assert.Equal(2, updatedTrap.OperatingDays);
            Assert.Equal(5.0f, updatedTrap.SignalStrength);

            var capture = await context.CaptureEvents.SingleAsync(c => c.TrapId == originalTrapId);
            Assert.Equal(originalTrapId, capture.TrapId);
            Assert.Equal(20.0, capture.RodentWeight.Grams);
            Assert.Equal(13.0, capture.RodentLength.Centimeters); // ir = 3 maps to 13cm
            Assert.Equal(5.0, capture.SignalStrength);

            var bait = await context.BaitMeasurements.SingleAsync(b => b.CaptureEventId == capture.Id);
            Assert.Equal(15.0, bait.BaitWeightGrams);
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
