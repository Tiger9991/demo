using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Battery.Queries
{
    public class GetBatteryStatusQueryHandler : IRequestHandler<GetBatteryStatusQuery, BatteryStatusDto>
    {
        private readonly IApplicationDbContext _context;

        public GetBatteryStatusQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BatteryStatusDto> Handle(GetBatteryStatusQuery request, CancellationToken cancellationToken)
        {
            var trap = await _context.Traps
                .FirstOrDefaultAsync(t => t.Id == request.TrapId, cancellationToken);

            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.TrapId);

            // Calculate current battery based on rules (even if not persisted)
            int calculatedBattery = Trap.CalculateBatteryPercentage(trap.status, trap.BatteryPercentage, trap.StartTime, trap.TotalTransmissions);

            return new BatteryStatusDto
            {
                TrapId = trap.Id,
                TrapNumber = trap.TrapNumber,
                CurrentBatteryPercentage = trap.BatteryPercentage,
                CalculatedBatteryPercentage = calculatedBattery,
                TotalTransmissions = trap.TotalTransmissions,
                OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - trap.StartTime).TotalDays)
            };
        }
    }
}
