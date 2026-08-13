using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.BaitMeasurement.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.TapBaitMeasurement.Commands
{
    public class CreateTrapBaitMeasurementCommandHandler : IRequestHandler<CreateTrapBaitMeasurementCommand, TrapBaitMeasurementDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateTrapBaitMeasurementCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrapBaitMeasurementDto> Handle(
            CreateTrapBaitMeasurementCommand request,
            CancellationToken cancellationToken)
        {
            var existingTrap = await _context.Traps
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber
                                       && t.TrapGroup == request.TrapGroup
                                       && t.status == "Active", cancellationToken);

            if (existingTrap == null)
                throw new Exception($"Active trap with number '{request.TrapNumber}' and group '{request.TrapGroup}' not found.");

            // 2. Create the BaitMeasurement
            var TrapbaitMeasurement = new TrapBaitMeasurement
            {
                Id = Guid.NewGuid(),
                TrapId = existingTrap.Id,
                SignalStrength = request.SignalStrength,
                MeasurementTime = DateTime.UtcNow,
                BaitWeightGrams = request.BWeight
            };

            // 3. Update trap statistics
            existingTrap.LastEntryDate = TrapbaitMeasurement.MeasurementTime;
            existingTrap.TotalTransmissions += 1;
            existingTrap.OperatingDays = (int)(DateTime.UtcNow - existingTrap.StartTime).TotalDays;
            existingTrap.SignalStrength = request.SignalStrength;
            existingTrap.UpdateBattery();
            existingTrap.UpdateIndicatorStatus();

            // 4. Add to context and save
            await _context.TrapBaitMeasurement.AddAsync(TrapbaitMeasurement, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            // 4. Build DTO using data from the linked CaptureEvent
            return new TrapBaitMeasurementDto
            {
                Id = TrapbaitMeasurement.Id

            };
        }


    }
}

