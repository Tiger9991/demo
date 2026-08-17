using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public class GetDisconnectedTrapsByGroupQueryHandler
    : IRequestHandler<GetDisconnectedTrapsByGroupQuery, List<DisconnectedTrapsPerGroupDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDisconnectedTrapsByGroupQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DisconnectedTrapsPerGroupDto>> Handle(
            GetDisconnectedTrapsByGroupQuery request,
            CancellationToken cancellationToken)
        {
            var allPhysicalTraps = await _context.Traps.AsNoTracking().ToListAsync(cancellationToken);

            var latestMeasurements = await _context.TrapBaitMeasurement.AsNoTracking()
                .GroupBy(m => m.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(m => m.MeasurementTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var latestCaptures = await _context.CaptureEvents.AsNoTracking()
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var offlineTraps = new List<(string TrapGroup, string TrapNumber)>();

            foreach (var trap in allPhysicalTraps)
            {
                if (trap.status != "Active")
                {
                    offlineTraps.Add((trap.TrapGroup ?? "Unassigned", trap.TrapNumber));
                    continue;
                }

                var candidateDates = new[] {
                    latestMeasurements.TryGetValue(trap.Id, out var lb) ? lb : (DateTime?)null,
                    latestCaptures.TryGetValue(trap.Id, out var lc) ? lc : (DateTime?)null,
                    trap.LastEntryDate
                }.Where(d => d.HasValue).Select(d => d.Value).ToList();

                DateTime? lastActivity = candidateDates.Any() ? candidateDates.Max() : null;

                bool isConnected = false;
                if (lastActivity.HasValue)
                {
                    isConnected = (DateTime.UtcNow - lastActivity.Value).TotalHours <= 2.0;
                }
                else
                {
                    isConnected = (DateTime.UtcNow - trap.StartTime).TotalHours <= 2.0;
                }

                if (!isConnected)
                {
                    offlineTraps.Add((trap.TrapGroup ?? "Unassigned", trap.TrapNumber));
                }
            }

            var grouped = offlineTraps
                .GroupBy(t => t.TrapGroup ?? "Unassigned")
                .Select(g => new DisconnectedTrapsPerGroupDto
                {
                    GroupNumber = g.Key,
                    Count = g.Count(),
                    TrapNumbers = g
                        .Select(t => t.TrapNumber)
                        .Distinct()
                        .OrderBy(tn => tn)
                        .ToList()
                })
                .OrderBy(dto => dto.GroupNumber)
                .ToList();

            return grouped;
        }
    }
}
