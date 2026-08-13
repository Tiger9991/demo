using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetTrapsNeedingRefillCountQueryHandler
       : IRequestHandler<GetTrapsNeedingRefillCountQuery, RefillCountDto>
    {
        private readonly IApplicationDbContext _context;

        public GetTrapsNeedingRefillCountQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<RefillCountDto> Handle(
            GetTrapsNeedingRefillCountQuery request,
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

            int count = 0;

            // 3. Process each trap to check the refill condition
            foreach (var trap in traps)
            {
                var trapMeasurements = baitMeasurements
                    .Where(b => b.TrapId == trap.Id)
                    .ToList(); // Already ordered by MeasurementTime

                double? weightAfterRefill = null;
                double latestBaitWeight = 0;

                if (trapMeasurements.Any())
                {
                    // The first measurement acts as the initial setup (acting as a "refill" baseline)
                    var first = trapMeasurements[0];
                    weightAfterRefill = first.BaitWeightGrams;
                    latestBaitWeight = first.BaitWeightGrams;

                    for (int i = 1; i < trapMeasurements.Count; i++)
                    {
                        var prev = trapMeasurements[i - 1];
                        var curr = trapMeasurements[i];

                        // If bait weight increased, a refill happened
                        if (curr.BaitWeightGrams > prev.BaitWeightGrams)
                        {
                            weightAfterRefill = curr.BaitWeightGrams;
                        }

                        latestBaitWeight = curr.BaitWeightGrams;
                    }
                }

                // Refill condition: weight has dropped to the threshold or less,
                // or if there are no measurements (meaning no bait has been recorded yet).
                bool needsRefill = !weightAfterRefill.HasValue || (latestBaitWeight < request.Threshold);

                if (needsRefill)
                {
                    count++;
                }
            }

            return new RefillCountDto { Count = count };
        }
    }
}
