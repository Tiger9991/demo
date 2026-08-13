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
    public class GetPeakHourDetailsQueryHandler
    : IRequestHandler<GetPeakHourDetailsQuery, List<PeakHourDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPeakHourDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<PeakHourDetailDto>> Handle(
            GetPeakHourDetailsQuery request,
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

            // First, find the peak hour
            var peakHourData = await query
                .GroupBy(c => c.CaptureTime.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(h => h.Count)
                .FirstOrDefaultAsync(cancellationToken);

            if (peakHourData == null || peakHourData.Count == 0)
                return new List<PeakHourDetailDto>();

            var peakHour = peakHourData.Hour;

            // Now get details for that hour per trap
            var details = await query
                .Where(c => c.CaptureTime.Hour == peakHour)
                .GroupBy(c => new { c.TrapId })
                .Select(g => new
                {
                    TrapId = g.Key.TrapId,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            var trapIds = details.Select(d => d.TrapId).ToList();
            var traps = await _context.Traps
                .Where(t => trapIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => new { t.TrapNumber, t.TrapGroup });

            var result = details
                .Select(d => new PeakHourDetailDto
                {
                    TrapNumber = traps.TryGetValue(d.TrapId, out var t) ? t.TrapNumber : "غير معروف",
                    GroupNumber = traps.TryGetValue(d.TrapId, out var tg) ? tg.TrapGroup : "غير معروف",
                    Count = d.Count
                })
                .OrderByDescending(d => d.Count)
                .ToList();

            return result;
        }
    }
}
