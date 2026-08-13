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
    public class CalculateBatteryFromTransmissionsQueryHandler : IRequestHandler<CalculateBatteryFromTransmissionsQuery, BatteryCalculationDto>
    {
        private readonly IApplicationDbContext _context;

        public CalculateBatteryFromTransmissionsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BatteryCalculationDto> Handle(CalculateBatteryFromTransmissionsQuery request, CancellationToken cancellationToken)
        {
            var trap = await _context.Traps
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber, cancellationToken);

            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.TrapNumber);

            // Determine which transmission count to use
            int usedTransmissions = request.TransmissionsCount ?? trap.TotalTransmissions;

            // Calculate operating days (from trap start time to now)
            int operatingDays = Math.Max(0, (int)(DateTime.UtcNow - trap.StartTime).TotalDays);

            // Apply battery formula
            int calculatedBattery = Trap.CalculateBatteryPercentage(
                trap.status, 
                trap.BatteryPercentage, 
                trap.StartTime, 
                usedTransmissions, 
                forceCalculate: true);

            string message;
            if (request.TransmissionsCount.HasValue)
                message = $"Battery calculated using provided transmission count ({usedTransmissions}) and {operatingDays} operating days.";
            else
                message = $"Battery calculated using actual stored transmission count ({usedTransmissions}) and {operatingDays} operating days.";

            return new BatteryCalculationDto
            {
                TrapNumber = trap.TrapNumber,
                UsedTransmissions = usedTransmissions,
                OperatingDays = operatingDays,
                CalculatedBatteryPercentage = calculatedBattery,
                CurrentStoredBatteryPercentage = trap.BatteryPercentage,
                Message = message
            };
        }
    }
}