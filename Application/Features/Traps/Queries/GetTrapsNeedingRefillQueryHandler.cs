using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetTrapsNeedingRefillQueryHandler
     : IRequestHandler<GetTrapsNeedingRefillQuery, List<TrapsNeedingRefillDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTrapsNeedingRefillQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<TrapsNeedingRefillDto>> Handle(
            GetTrapsNeedingRefillQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Filter traps (by status and group)
            var trapQuery = _context.Traps
                .Where(t => (string.IsNullOrEmpty(request.Status) || t.status == request.Status)
                            && (string.IsNullOrEmpty(request.GroupNumber) || t.TrapGroup == request.GroupNumber));

            var traps = await trapQuery.ToListAsync(cancellationToken);
            var trapIds = traps.Select(t => t.Id).ToList();

            // 2. Get all bait measurements for these traps ordered by time
            var baitMeasurements = await _context.BaitMeasurements
                .Where(b => trapIds.Contains(b.TrapId))
                .OrderBy(b => b.MeasurementTime)
                .ToListAsync(cancellationToken);

            var result = new List<TrapsNeedingRefillDto>();

            // 3. Process each trap to find the last refill point and check the refill condition
            foreach (var trap in traps)
            {
                var trapMeasurements = baitMeasurements
                    .Where(b => b.TrapId == trap.Id)
                    .ToList(); // Already ordered by MeasurementTime

                DateTime? previousRefillDate = null;
                double? weightAfterRefill = null;
                double latestBaitWeight = 0;
                DateTime lastMeasurementTime = DateTime.MinValue;
                DateTime? previousMeasurementTime = null;
                double? previousBaitWeight = null;

                if (trapMeasurements.Any())
                {
                    // The first measurement acts as the initial setup (acting as a "refill" baseline)
                    var first = trapMeasurements[0];
                    previousRefillDate = first.MeasurementTime;
                    weightAfterRefill = first.BaitWeightGrams;
                    latestBaitWeight = first.BaitWeightGrams;
                    lastMeasurementTime = first.MeasurementTime;

                    for (int i = 1; i < trapMeasurements.Count; i++)
                    {
                        var prev = trapMeasurements[i - 1];
                        var curr = trapMeasurements[i];

                        // If bait weight increased, a refill happened
                        if (curr.BaitWeightGrams > prev.BaitWeightGrams)
                        {
                            previousRefillDate = curr.MeasurementTime;
                            weightAfterRefill = curr.BaitWeightGrams;
                        }

                        latestBaitWeight = curr.BaitWeightGrams;
                        lastMeasurementTime = curr.MeasurementTime;
                    }

                    if (trapMeasurements.Count >= 2)
                    {
                        var secondLast = trapMeasurements[trapMeasurements.Count - 2];
                        previousMeasurementTime = secondLast.MeasurementTime;
                        previousBaitWeight = secondLast.BaitWeightGrams;
                    }
                }

                // Refill condition: weight has dropped to the threshold or less,
                // or if there are no measurements (meaning no bait has been recorded yet).
                bool needsRefill = !weightAfterRefill.HasValue || (latestBaitWeight < request.Threshold);

                if (needsRefill)
                {
                    result.Add(new TrapsNeedingRefillDto
                    {
                        TrapNumber = trap.TrapNumber,
                        GroupNumber = trap.TrapGroup ?? "Unassigned",
                        Status = trap.status,
                        LatestBaitWeight = latestBaitWeight,
                        LastMeasurementTime = lastMeasurementTime,
                        DaysSinceLastMeasurement = lastMeasurementTime != DateTime.MinValue
                            ? (int)(DateTime.UtcNow - lastMeasurementTime).TotalDays
                            : 999,
                        PreviousRefillDate = previousRefillDate,
                        WeightAfterRefill = weightAfterRefill,
                        PreviousMeasurementTime = previousMeasurementTime,
                        PreviousBaitWeight = previousBaitWeight
                    });
                }
            }

            var finalResult = result
                .OrderBy(dto => dto.LatestBaitWeight)
                .DistinctBy(dto => new { dto.GroupNumber, dto.TrapNumber })
                .ToList();

            return finalResult;
        }
    }
}
