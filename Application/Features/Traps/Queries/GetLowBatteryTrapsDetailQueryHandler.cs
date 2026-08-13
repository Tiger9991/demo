using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetLowBatteryTrapsDetailQueryHandler
    : IRequestHandler<GetLowBatteryTrapsDetailQuery, List<LowBatteryTrapDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetLowBatteryTrapsDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LowBatteryTrapDto>> Handle(
            GetLowBatteryTrapsDetailQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Traps.AsQueryable();

            // Filter by group if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
                query = query.Where(t => t.TrapGroup == request.GroupNumber);

            var traps = await query.ToListAsync(cancellationToken);

            var trapIds = traps.Select(t => t.Id).ToList();

            var latestMeasurements = await _context.TrapBaitMeasurement.AsNoTracking()
                .Where(m => trapIds.Contains(m.TrapId))
                .GroupBy(m => m.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(m => m.MeasurementTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var latestCaptures = await _context.CaptureEvents.AsNoTracking()
                .Where(c => trapIds.Contains(c.TrapId))
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            // Compute dynamic real-time battery percentage in memory and filter
            var result = traps
                .Select(t => new
                {
                    Trap = t,
                    CalculatedBattery = Trap.CalculateBatteryPercentage(t.status, t.BatteryPercentage, t.StartTime, t.TotalTransmissions)
                })
                .Where(x => x.CalculatedBattery <= request.Threshold)
                .Select(x =>
                {
                    bool isConnected = false;
                    DateTime? lastActivity = null;

                    if (latestMeasurements.TryGetValue(x.Trap.Id, out var latestBaitTime))
                    {
                        lastActivity = latestBaitTime;
                    }

                    if (latestCaptures.TryGetValue(x.Trap.Id, out var latestCaptureTime))
                    {
                        if (lastActivity == null || latestCaptureTime > lastActivity.Value)
                        {
                            lastActivity = latestCaptureTime;
                        }
                    }

                    if (lastActivity.HasValue)
                    {
                        if ((DateTime.UtcNow - lastActivity.Value).TotalHours <= 2)
                        {
                            isConnected = true;
                        }
                    }

                    return new LowBatteryTrapDto
                    {
                        TrapNumber = x.Trap.TrapNumber,
                        GroupNumber = x.Trap.TrapGroup ?? "Unassigned",
                        BatteryPercentage = x.CalculatedBattery,
                        Status = x.Trap.status,
                        IsConnected = x.Trap.status == "Active" && isConnected
                    };
                })
                .OrderBy(t => t.BatteryPercentage)
                .ToList();

            return result;
        }
    }
}

