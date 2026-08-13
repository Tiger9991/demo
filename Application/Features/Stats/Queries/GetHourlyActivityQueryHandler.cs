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
    public class GetHourlyActivityQueryHandler
    : IRequestHandler<GetHourlyActivityQuery, List<HourlyActivityDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetHourlyActivityQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<HourlyActivityDto>> Handle(
            GetHourlyActivityQuery request,
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

            var data = await query
                .GroupBy(c => c.CaptureTime.Hour)
                .Select(g => new HourlyActivityDto
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToListAsync(cancellationToken);

            // Fill missing hours with 0
            var allHours = Enumerable.Range(0, 24)
                .Select(h => new HourlyActivityDto
                {
                    Hour = h,
                    Count = data.FirstOrDefault(d => d.Hour == h)?.Count ?? 0
                })
                .ToList();

            return allHours;
        }
    }
}
