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
    public class GetActivityByHourSummaryQueryHandler
    : IRequestHandler<GetActivityByHourSummaryQuery, ActivityByHourSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetActivityByHourSummaryQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<ActivityByHourSummaryDto> Handle(
            GetActivityByHourSummaryQuery request,
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

            var total = hourlyData.Sum(h => h.Count);
            var peak = hourlyData.OrderByDescending(h => h.Count).FirstOrDefault();

            return new ActivityByHourSummaryDto
            {
                TotalCaptures = total,
                PeakHour = peak?.Hour ?? 0,
                PeakHourCount = peak?.Count ?? 0,
                AveragePerHour = Math.Round((double)total / 24, 2),
                Message = $"إجمالي النشاط: {total} زيارة، الذروة عند الساعة {peak?.Hour:00}:00 ({peak?.Count} زيارة)"
            };
        }
    }
}
