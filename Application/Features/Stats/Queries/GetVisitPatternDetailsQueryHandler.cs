using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetVisitPatternDetailsQueryHandler
    : IRequestHandler<GetVisitPatternDetailsQuery, List<VisitPatternDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetVisitPatternDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<VisitPatternDetailDto>> Handle(
            GetVisitPatternDetailsQuery request,
            CancellationToken cancellationToken)
        {
            // Start with CaptureEvents and include Trap
            var query = _context.CaptureEvents
                .Include(c => c.Trap)
                .AsQueryable();

            // Filter by group if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
                query = query.Where(c => c.Trap.TrapGroup == request.GroupNumber);

            // Filter by date range if provided
            if (request.FromDate.HasValue)
                query = query.Where(c => c.CaptureTime >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(c => c.CaptureTime <= request.ToDate.Value);

            // Group by Trap and compute statistics
            var result = await query
                .GroupBy(c => new { c.Trap.TrapNumber, c.Trap.TrapGroup })
                .Select(g => new VisitPatternDetailDto
                {
                    TrapNumber = g.Key.TrapNumber,
                    GroupNumber = g.Key.TrapGroup ?? "Unassigned",
                    TotalVisits = g.Count(),
                    FirstVisit = g.Min(c => c.CaptureTime),
                    LastVisit = g.Max(c => c.CaptureTime),
                    AverageVisitsPerDay = g.Count() / Math.Max(1, (g.Max(c => c.CaptureTime) - g.Min(c => c.CaptureTime)).TotalDays)
                })
                .OrderBy(dto => dto.GroupNumber)
                .ThenBy(dto => dto.TrapNumber)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
