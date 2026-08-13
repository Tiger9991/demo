using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetAllTrapsAverageSeverityQueryHandler
    : IRequestHandler<GetAllTrapsAverageSeverityQuery, List<AverageSeverityDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllTrapsAverageSeverityQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<AverageSeverityDto>> Handle(
            GetAllTrapsAverageSeverityQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Date range (default: last 7 days)
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-7);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            // 2. Get traps (optionally filtered by group)
            var trapQuery = _context.Traps.AsQueryable();
            if (!string.IsNullOrEmpty(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);
            var traps = await trapQuery.ToListAsync(cancellationToken);

            // If no traps, return empty list
            if (!traps.Any())
                return new List<AverageSeverityDto>();

            // 3. Get captures and bait measurements in date range
            var captures = await _context.CaptureEvents
                .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= toDate)
                .ToListAsync(cancellationToken);

            var baitMeasurements = await _context.BaitMeasurements
                .Where(b => b.MeasurementTime >= fromDate && b.MeasurementTime <= toDate)
                .ToListAsync(cancellationToken);

            // 4. Compute raw metrics per trap
            var results = new List<AverageSeverityDto>();
            foreach (var trap in traps)
            {
                var trapCaptures = captures.Where(c => c.TrapId == trap.Id).ToList();
                var trapBaits = baitMeasurements.Where(b => b.TrapId == trap.Id).ToList();

                int captureCount = trapCaptures.Count;
                double baitConsumption = trapBaits.Sum(b => b.BaitWeightGrams);

                double repeatRate = 0;
                if (captureCount > 0)
                {
                    var mostFrequent = trapCaptures
                        .GroupBy(c => c.RodentType)
                        .OrderByDescending(g => g.Count())
                        .First();
                    repeatRate = (double)mostFrequent.Count() / captureCount;
                }

                double timeSpent = 0;
                var capturesWithDuration = trapCaptures.Where(c => c.Duration.HasValue).ToList();
                if (capturesWithDuration.Any())
                {
                    timeSpent = capturesWithDuration.Average(c => c.Duration.GetValueOrDefault());
                }
                else if (captureCount > 0)
                {
                    timeSpent = 5.0; // default fallback duration for a capture event
                }

                results.Add(new AverageSeverityDto
                {
                    TrapNumber = trap.TrapNumber,
                    GroupNumber = trap.TrapGroup ?? "Unassigned",
                    CaptureCount = captureCount,
                    BaitConsumption = baitConsumption,
                    RepeatRate = repeatRate,
                    TimeSpent = timeSpent
                });
            }

            // 5. Normalize using global maxima (avoiding division by zero)
            var maxCaptures = results.Any() ? Math.Max(1, results.Max(r => r.CaptureCount)) : 1;
            var maxBait = results.Any() ? Math.Max(1.0, results.Max(r => r.BaitConsumption)) : 1;
            var maxRepeat = results.Any() ? Math.Max(1.0, results.Max(r => r.RepeatRate)) : 1;
            var maxTime = results.Any() ? Math.Max(1.0, results.Max(r => r.TimeSpent)) : 1;

            foreach (var r in results)
            {
                r.NormalizedCaptures = Math.Clamp((double)r.CaptureCount / maxCaptures * 100, 0, 100);
                r.NormalizedBait = Math.Clamp(r.BaitConsumption / maxBait * 100, 0, 100);
                r.NormalizedRepeat = Math.Clamp(r.RepeatRate / maxRepeat * 100, 0, 100);
                r.NormalizedTime = Math.Clamp(r.TimeSpent / maxTime * 100, 0, 100);

                // 6. Average Severity Score (equal weights)
                r.SeverityScore = Math.Clamp(
                    (r.NormalizedCaptures + r.NormalizedBait + r.NormalizedRepeat + r.NormalizedTime) / 4,
                    0, 100
                );

                // 7. Classify
                r.SeverityLevel = r.SeverityScore switch
                {
                    <= 30 => "منخفض",
                    <= 50 => "متوسط",
                    <= 80 => "مرتفع",
                    <= 90 => "حرج",
                    _ => "حرج جداً"
                };
            }

            return results.OrderByDescending(r => r.SeverityScore).ToList();
        }
    }
}
