using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetPeakHourSummaryQueryHandler
    : IRequestHandler<GetPeakHourSummaryQuery, PeakHourSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetPeakHourSummaryQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<PeakHourSummaryDto> Handle(
            GetPeakHourSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            var query = _context.CaptureEvents
                .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= toDate);

            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                query = from c in query
                        join t in _context.Traps on c.TrapId equals t.Id
                        where t.TrapGroup == request.GroupNumber
                        select c;
            }

            var hourlyData = await query
                .GroupBy(c => c.CaptureTime.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var totalCaptures = hourlyData.Sum(h => h.Count);
            var peak = hourlyData.OrderByDescending(h => h.Count).FirstOrDefault();
            var percentage = totalCaptures > 0 && peak != null ? Math.Round((double)peak.Count * 100 / totalCaptures, 1) : 0;

            var peakHour = peak?.Hour ?? 0;
            var peakHourCount = peak?.Count ?? 0;

            return new PeakHourSummaryDto
            {
                PeakHour = peakHour,
                PeakHourCount = peakHourCount,
                TotalCaptures = totalCaptures,
                PercentageOfTotal = percentage,
                Message = $"ساعة الذروة هي {peakHour:00}:00 بنسبة {percentage}% من إجمالي النشاط"
            };
        }
    }
}
