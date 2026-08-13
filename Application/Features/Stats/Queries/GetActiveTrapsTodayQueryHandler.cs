using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Stats.Queries
{
    public class GetActiveTrapsTodayQueryHandler : IRequestHandler<GetActiveTrapsTodayQuery, ActiveTrapsTodayDto>
    {
        private readonly IApplicationDbContext _context;

        public GetActiveTrapsTodayQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActiveTrapsTodayDto> Handle(GetActiveTrapsTodayQuery request, CancellationToken cancellationToken)
        {
            var targetDate = request.Date?.Date ?? DateTime.UtcNow.Date;

            // Find all captures on the target date
            var query = _context.CaptureEvents
                .Where(c => c.CaptureTime.Date == targetDate);

            // Filter by TrapGroup if requested
            if (!string.IsNullOrEmpty(request.GroupNumber))
            {
                query = query.Where(c => c.Trap.TrapGroup == request.GroupNumber);
            }

            // Get unique TrapIds that registered activity today
            var activeTrapIds = await query
                .Select(c => c.TrapId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Project details
            var trapsDetails = await _context.Traps
                .Where(t => activeTrapIds.Contains(t.Id))
                .Select(t => new ActiveTrapTodayDetailDto
                {
                    TrapId = t.Id,
                    TrapNumber = t.TrapNumber,
                    TrapGroup = t.TrapGroup ?? string.Empty,
                    Status = t.status,
                    BatteryPercentage = t.BatteryPercentage,
                    SignalStrength = t.SignalStrength,
                    SignalQuality = t.SignalQuality,
                    LastCaptureTime = _context.CaptureEvents
                        .Where(c => c.TrapId == t.Id && c.CaptureTime.Date == targetDate)
                        .Max(c => (DateTime?)c.CaptureTime),
                    TotalCapturesToday = _context.CaptureEvents
                        .Count(c => c.TrapId == t.Id && c.CaptureTime.Date == targetDate)
                })
                .OrderBy(dto => dto.TrapGroup)
                .ThenBy(dto => dto.TrapNumber)
                .ToListAsync(cancellationToken);

            return new ActiveTrapsTodayDto
            {
                TotalActiveTrapsCount = trapsDetails.Count,
                ActiveTrapsDetails = trapsDetails
            };
        }
    }
}
