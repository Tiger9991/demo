using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetRodentActivityQueryHandler
        : IRequestHandler<GetRodentActivityQuery, RodentActivityDto>
    {
        private readonly IApplicationDbContext _context;

        public GetRodentActivityQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<RodentActivityDto> Handle(
            GetRodentActivityQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Start with CaptureEvents and Traps
            var captureQuery = _context.CaptureEvents.AsQueryable();
            var trapQuery = _context.Traps.AsQueryable();

            // 2. Apply status filter (default: "Active")
            var statusFilter = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status;
            trapQuery = trapQuery.Where(t => t.status == statusFilter);

            // 3. Apply group filter if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);

            // 4. Join CaptureEvents with filtered Traps using TrapId (FK)
            var joinedQuery = from c in captureQuery
                              join t in trapQuery on c.TrapId equals t.Id
                              select new { Capture = c, Trap = t };

            // 5. Apply date filters
            if (request.FromDate.HasValue)
                joinedQuery = joinedQuery.Where(x => x.Capture.CaptureTime >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                joinedQuery = joinedQuery.Where(x => x.Capture.CaptureTime <= request.ToDate.Value);

            // 6. Load data into memory (aggregations in memory are fine for moderate data)
            var captures = await joinedQuery
                .Select(x => new
                {
                    x.Capture.RodentType,
                    x.Capture.CaptureTime,
                    x.Capture.TrapId
                })
                .ToListAsync(cancellationToken);

            // 7. Compute totals
            int totalCaptures = captures.Count;
            int trapsWithCaptures = captures.Select(c => c.TrapId).Distinct().Count();

            // 8. Group by RodentType
            var byType = captures
                .GroupBy(c => c.RodentType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // 9. Group by Date (as DateTime, without time) – matches DTO
            var byDate = captures
                .GroupBy(c => c.CaptureTime.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            // 10. Build message
            var groupDisplay = string.IsNullOrEmpty(request.GroupNumber) ? "all groups" : $"group '{request.GroupNumber}'";
            var statusDisplay = statusFilter;
            var dateRange = "";
            if (request.FromDate.HasValue && request.ToDate.HasValue)
                dateRange = $" from {request.FromDate.Value:yyyy-MM-dd} to {request.ToDate.Value:yyyy-MM-dd}";
            else if (request.FromDate.HasValue)
                dateRange = $" from {request.FromDate.Value:yyyy-MM-dd}";
            else if (request.ToDate.HasValue)
                dateRange = $" up to {request.ToDate.Value:yyyy-MM-dd}";

            var message = $"Total captures: {totalCaptures} from {trapsWithCaptures} traps ({groupDisplay}, status '{statusDisplay}'){dateRange}.";

            return new RodentActivityDto
            {
                TotalCaptures = totalCaptures,
                TrapsWithCaptures = trapsWithCaptures,
                CapturesByType = byType,
                CapturesByDate = byDate,
                GroupNumber = request.GroupNumber,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Message = message
            };
        }
    }
}

