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
            var allTrapGroups = await _context.TrapGroups.AsNoTracking()
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);

            var distinctGroups = allTrapGroups
                .DistinctBy(g => new { g.TrapGroup, g.TrapNumber })
                .ToList();

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

            foreach (var tg in distinctGroups)
            {
                var matchingTrap = allPhysicalTraps
                    .OrderByDescending(t => t.LastEntryDate ?? t.CreatedAt)
                    .FirstOrDefault(t => t.TrapGroup == tg.TrapGroup && t.TrapNumber == tg.TrapNumber);

                if (matchingTrap == null || matchingTrap.status != "Active")
                {
                    offlineTraps.Add((tg.TrapGroup, tg.TrapNumber));
                }
                else
                {
                    bool isDisconnected = false;
                    DateTime? lastActivity = null;

                    if (latestMeasurements.TryGetValue(matchingTrap.Id, out var latestBaitTime))
                    {
                        lastActivity = latestBaitTime;
                    }

                    if (latestCaptures.TryGetValue(matchingTrap.Id, out var latestCaptureTime))
                    {
                        if (lastActivity == null || latestCaptureTime > lastActivity.Value)
                        {
                            lastActivity = latestCaptureTime;
                        }
                    }

                    if (lastActivity.HasValue)
                    {
                        if ((DateTime.UtcNow - lastActivity.Value).TotalHours > 2)
                        {
                            isDisconnected = true;
                        }
                    }
                    else
                    {
                        isDisconnected = true;
                    }

                    if (isDisconnected)
                    {
                        offlineTraps.Add((matchingTrap.TrapGroup, matchingTrap.TrapNumber));
                    }
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
