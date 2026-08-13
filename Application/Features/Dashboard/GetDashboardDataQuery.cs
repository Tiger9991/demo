using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;


namespace Application.Features.Dashboard
{
    public record GetDashboardDataQuery : IRequest<DashboardDto>;
    public class DashboardDto
    {
        public int TotalTraps { get; set; }
        public double AverageBattery { get; set; }
        public int ActiveTraps { get; set; }
        public int TotalCapturesToday { get; set; }
        public Dictionary<RodentType, int> CapturesByType { get; set; } = new();
    }

    public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashboardDto>
    {
        private readonly IApplicationDbContext _context;
        public GetDashboardDataQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<DashboardDto> Handle(GetDashboardDataQuery request, CancellationToken ct)
        {
            var traps = await _context.Traps.ToListAsync(ct);
            var capturesToday = await _context.CaptureEvents
                .Where(c => c.CaptureTime.Date == DateTime.UtcNow.Date)
                .ToListAsync(ct);

            return new DashboardDto
            {
                TotalTraps = traps.Count,
                AverageBattery = traps.Any()
                    ? traps.Average(t => Trap.CalculateBatteryPercentage(t.status, t.BatteryPercentage, t.StartTime, t.TotalTransmissions))
                    : 0,
                ActiveTraps = traps.Count(t => t.status == "Active"),
                TotalCapturesToday = capturesToday.Count,
                CapturesByType = capturesToday
                    .GroupBy(c => c.RodentType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }
}
