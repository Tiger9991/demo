using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Battery.Commands
{
    public class RecalculateBatteryCommandHandler : IRequestHandler<RecalculateBatteryCommand, BatteryStatusDto>
    {
        private readonly IApplicationDbContext _context;

        public RecalculateBatteryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BatteryStatusDto> Handle(RecalculateBatteryCommand request, CancellationToken cancellationToken)
        {
            var trap = await _context.Traps
                .FindAsync(new object[] { request.TrapId }, cancellationToken);

            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.TrapId);

            // Store current battery before recalculation
            int previousBattery = trap.BatteryPercentage;

            // Update battery and indicator status using domain logic
            trap.UpdateBattery(forceCalculate: true);
            trap.UpdateIndicatorStatus();
            await _context.SaveChangesAsync(cancellationToken);

            return new BatteryStatusDto
            {
                TrapId = trap.Id,
                TrapNumber = trap.TrapNumber,
                CurrentBatteryPercentage = previousBattery,
                CalculatedBatteryPercentage = trap.BatteryPercentage,
                TotalTransmissions = trap.TotalTransmissions,
                OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - trap.StartTime).TotalDays)
            };
        }
    }
}
