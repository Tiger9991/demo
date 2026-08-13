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
    public class GetActivityByHourDetailsQueryHandler
    : IRequestHandler<GetActivityByHourDetailsQuery, List<ActivityByHourDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetActivityByHourDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<ActivityByHourDetailDto>> Handle(
            GetActivityByHourDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            var captureQuery = _context.CaptureEvents
                .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= toDate);

            var joinedQuery = from c in captureQuery
                              join t in _context.Traps on c.TrapId equals t.Id
                              select new { c.TrapId, t.TrapNumber, t.TrapGroup, c.CaptureTime.Hour };

            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                joinedQuery = joinedQuery.Where(x => x.TrapGroup == request.GroupNumber);
            }

            var rawData = await joinedQuery.ToListAsync(cancellationToken);

            var result = rawData
                .GroupBy(x => new { x.TrapId, x.TrapNumber, x.TrapGroup })
                .Select(g => new ActivityByHourDetailDto
                {
                    TrapNumber = g.Key.TrapNumber,
                    GroupNumber = g.Key.TrapGroup,
                    HourlyCounts = g.GroupBy(x => x.Hour)
                                    .ToDictionary(hg => hg.Key, hg => hg.Count())
                })
                .ToList();

            return result;
        }
    }
}
