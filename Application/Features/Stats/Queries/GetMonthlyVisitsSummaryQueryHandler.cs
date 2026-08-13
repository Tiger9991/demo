using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetMonthlyVisitsSummaryQueryHandler
    : IRequestHandler<GetMonthlyVisitsSummaryQuery, MonthlyVisitsSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetMonthlyVisitsSummaryQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<MonthlyVisitsSummaryDto> Handle(
            GetMonthlyVisitsSummaryQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Calculate month range
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(request.MonthOffset ?? 0);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // 2. Start with capture events
            var query = _context.CaptureEvents
                .Where(c => c.CaptureTime >= monthStart && c.CaptureTime <= monthEnd);

            // 3. Filter by group if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                query = from c in query
                        join t in _context.Traps on c.TrapId equals t.Id
                        where t.TrapGroup == request.GroupNumber
                        select c;
            }

            // 4. Get daily counts
            var dailyData = await query
                .GroupBy(c => c.CaptureTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // 5. Compute summary
            var totalVisits = dailyData.Sum(d => d.Count);
            var totalDays = (monthEnd - monthStart).Days + 1;
            var averagePerDay = totalDays > 0 ? totalVisits / totalDays : 0;

            var maxDay = dailyData.OrderByDescending(d => d.Count).FirstOrDefault();

            return new MonthlyVisitsSummaryDto
            {
                TotalVisits = totalVisits,
                MonthStart = monthStart,
                MonthEnd = monthEnd,
                AveragePerDay = averagePerDay,
                MaxDayVisits = maxDay?.Count ?? 0,
                MaxDay = maxDay?.Date ?? monthStart,
                Message = $"إجمالي الزيارات: {totalVisits} زيارة خلال {totalDays} يوم"
            };
        }
    }
}
