using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetDailyVisitsQueryHandler
    : IRequestHandler<GetDailyVisitsQuery, List<DailyVisitDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDailyVisitsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<DailyVisitDto>> Handle(GetDailyVisitsQuery request, CancellationToken cancellationToken)
        {
            var endDate = request.ToDate ?? DateTime.UtcNow;
            var startDate = request.FromDate ?? endDate.AddDays(-(request.Days ?? 30));

            var query = _context.CaptureEvents
                .Where(c => c.CaptureTime >= startDate && c.CaptureTime <= endDate);

            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                query = from c in query
                        join t in _context.Traps on c.TrapId equals t.Id
                        where t.TrapGroup == request.GroupNumber
                        select c;
            }

            var daily = await query
                .GroupBy(c => c.CaptureTime.Date)
                .Select(g => new DailyVisitDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);

            // Fill missing dates
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(d => startDate.AddDays(d).Date)
                .ToList();

            return allDates
                .Select(date => new DailyVisitDto
                {
                    Date = date,
                    Count = daily.FirstOrDefault(d => d.Date == date)?.Count ?? 0
                })
                .ToList();
        }
    }
}
