using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetActivityIndexByLocationQueryHandler
    : IRequestHandler<GetActivityIndexByLocationQuery, List<ActivityIndexByLocationDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetActivityIndexByLocationQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<ActivityIndexByLocationDto>> Handle(
            GetActivityIndexByLocationQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Get severity for all traps (reuse the existing handler)
            var severityHandler = new GetAllTrapsAverageSeverityQueryHandler(_context);
            var allSeverity = await severityHandler.Handle(
                new GetAllTrapsAverageSeverityQuery(request.GroupNumber),
                cancellationToken
            );

            // 2. Group by GroupNumber and compute average severity
            var grouped = allSeverity
                .GroupBy(d => d.GroupNumber)
                .Select(g => new ActivityIndexByLocationDto
                {
                    GroupNumber = g.Key,
                    Index = Math.Round(g.Average(d => d.SeverityScore), 2)
                })
                .OrderBy(d => d.GroupNumber)
                .ToList();

            // 3. Assign colors based on index value
            foreach (var item in grouped)
            {
                item.Color = item.Index switch
                {
                    <= 30 => "#28a745",   // Green – Low
                    <= 50 => "#ffc107",   // Yellow – Medium
                    <= 80 => "#fd7e14",   // Orange – High
                    <= 90 => "#dc3545",   // Red – Critical
                    _ => "#8b0000"        // Dark Red – Very Critical
                };
            }

            return grouped;
        }
    }
}
