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
    public class GetAverageDailyVisitsSummaryQueryHandler
    : IRequestHandler<GetAverageDailyVisitsSummaryQuery, AverageDailyVisitsSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetAverageDailyVisitsSummaryQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<AverageDailyVisitsSummaryDto> Handle(
            GetAverageDailyVisitsSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var days = request.Days ?? 30;
            var fromDate = now.AddDays(-days);

            var query = _context.CaptureEvents
                .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= now);

            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                query = from c in query
                        join t in _context.Traps on c.TrapId equals t.Id
                        where t.TrapGroup == request.GroupNumber
                        select c;
            }

            var dailyData = await query
                .GroupBy(c => c.CaptureTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var totalVisits = dailyData.Sum(d => d.Count);
            var average = days > 0 ? Math.Round((double)totalVisits / days, 2) : 0;
            var maxDay = dailyData.OrderByDescending(d => d.Count).FirstOrDefault();

            return new AverageDailyVisitsSummaryDto
            {
                Average = average,
                TotalVisits = totalVisits,
                TotalDays = days,
                MaxDayVisits = maxDay?.Count ?? 0,
                MaxDay = maxDay?.Date ?? fromDate,
                Message = $"متوسط الزيارات اليومية: {average} زيارات خلال {days} يوماً"
            };
        }
    }
}
