using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.EntityFrameworkCore;


namespace Application.Features.Stats.Queries
{
    public class GetRodentTypeDistributionQueryHandler
    : IRequestHandler<GetRodentTypeDistributionQuery, List<RodentTypeDistributionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRodentTypeDistributionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RodentTypeDistributionDto>> Handle(
            GetRodentTypeDistributionQuery request,
            CancellationToken cancellationToken)
        {
            // Start with all capture events
            var captureQuery = _context.CaptureEvents.AsQueryable();

            // If a group number is provided, join with Traps to filter by TrapGroup
            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                captureQuery = from c in captureQuery
                               join t in _context.Traps on c.TrapId equals t.Id
                               where t.TrapGroup == request.GroupNumber
                               select c;
            }

            // Group by RodentType and count
            var groups = await captureQuery
                .GroupBy(c => c.RodentType)
                .Select(g => new { RodentType = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var total = groups.Sum(g => g.Count);

            var targetTypes = new[]
            {
                (RodentType.NormalRat, "House mouse"),
                (RodentType.ClimbingRat, "Climbing rat"),
                (RodentType.NorwegianRat, "Norwegian rat"),
                (RodentType.Unknown, "Unknown")
            };

            var distribution = new List<RodentTypeDistributionDto>();
            foreach (var (type, name) in targetTypes)
            {
                var group = groups.FirstOrDefault(g => g.RodentType == type);
                var count = group?.Count ?? 0;
                var percentage = total > 0 ? Math.Round((count * 100.0) / total, 1) : 0.0;
                distribution.Add(new RodentTypeDistributionDto
                {
                    RodentType = name,
                    Count = count,
                    Percentage = percentage
                });
            }

            if (total > 0)
            {
                distribution = distribution.OrderByDescending(d => d.Percentage).ToList();
            }

            return distribution;
        }
    }
}
