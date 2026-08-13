using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Trap> Traps { get; }
        DbSet<CaptureEvent> CaptureEvents { get; }
        DbSet<BaitMeasurement> BaitMeasurements { get; }
        DbSet<Customers> Customers { get; }
        DbSet<TrapGroups> TrapGroups { get; }

        DbSet<TrapBaitMeasurement> TrapBaitMeasurement { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
