using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetLowBatteryCountQueryHandler
     : IRequestHandler<GetLowBatteryCountQuery, LowBatteryCountDto>
    {
        private readonly IApplicationDbContext _context;

        public GetLowBatteryCountQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LowBatteryCountDto> Handle(
            GetLowBatteryCountQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Traps.AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(t => t.status == request.Status);

            if (!string.IsNullOrEmpty(request.GroupNumber))
                query = query.Where(t => t.TrapGroup == request.GroupNumber);

            var traps = await query.ToListAsync(cancellationToken);

            // Compute dynamic real-time battery percentage in memory and count matching traps
            var count = traps
                .Select(t => Trap.CalculateBatteryPercentage(t.status, t.BatteryPercentage, t.StartTime, t.TotalTransmissions))
                .Count(battery => battery <= request.Threshold);

            var statusDisplay = string.IsNullOrEmpty(request.Status) ? "all statuses" : $"status '{request.Status}'";

            return new LowBatteryCountDto
            {
                Count = count,
                Threshold = request.Threshold,
                Status = request.Status,
                Message = $"Found {count} trap(s) with battery at or below {request.Threshold}% and {statusDisplay}."
            };
        }
    }
}
