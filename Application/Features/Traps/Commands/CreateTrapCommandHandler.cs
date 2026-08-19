using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;


namespace Application.Features.Traps.Commands
{
    public class CreateTrapCommandHandler : IRequestHandler<CreateTrapCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateTrapCommandHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<Guid> Handle(CreateTrapCommand request, CancellationToken cancellationToken)
        {
            // 1. Find an existing active trap with the same number and group
            var trapGroupSearch = request.TrapGroup ?? string.Empty;
            var existingTrap = await _context.Traps
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber
                                       && t.TrapGroup == trapGroupSearch
                                       && t.status == "Active", cancellationToken);

            if (existingTrap != null)
            {
                // ---- Update existing active trap (only SignalStrength and Battery) ----

                existingTrap.SignalStrength = request.SignalStrength;
                existingTrap.StartTime = DateTime.UtcNow;
                existingTrap.BatteryPercentage = 100;
                existingTrap.TotalTransmissions = 0;

                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    existingTrap.Latitude = request.Latitude.Value;
                    existingTrap.Longitude = request.Longitude.Value;
                }

                // Record bait measurement if a value was provided
                //if (request.BaitWeightGrams.HasValue)
                //{
                //    var baitRecord = new Domain.Entities.TrapBaitMeasurement
                //    {
                //        Id = Guid.NewGuid(),
                //        TrapId = existingTrap.Id,
                        
                //        MeasurementTime = DateTime.UtcNow,
                //        BaitWeightGrams = request.BaitWeightGrams.Value
                //    };
                //    await _context.TrapBaitMeasurement.AddAsync(baitRecord, cancellationToken);
                //}

                await _context.SaveChangesAsync(cancellationToken);
                return existingTrap.Id;
            }
            else
            {
                // ---- Create a new trap with default values ----
                var (defaultLat, defaultLng) = CairoLocationHelper.GenerateDistributedCairoCoordinate(
                    request.TrapGroup,
                    request.TrapNumber
                );

                var newTrap = new Trap
                {
                    Id = Guid.NewGuid(),
                    TrapNumber = request.TrapNumber,
                    TrapGroup = request.TrapGroup ?? string.Empty,
                    SignalStrength = request.SignalStrength,
                    status = "Active",
                    StartTime = DateTime.UtcNow,
                    BatteryPercentage = 100,
                    IndicatorStatus = IndicatorStatus.Green,
                    LastEntryDate = null,
                    TotalTransmissions = 0,
                    OperatingDays = 0,
                    Latitude = request.Latitude ?? defaultLat,
                    Longitude = request.Longitude ?? defaultLng,
                };

                await _context.Traps.AddAsync(newTrap, cancellationToken);

                // Record bait measurement if a value was provided
                //if (request.BaitWeightGrams.HasValue)
                //{
                //    var baitRecord = new Domain.Entities.TrapBaitMeasurement
                //    {
                //        Id = Guid.NewGuid(),
                //        TrapId = newTrap.Id,
                        
                //        MeasurementTime = DateTime.UtcNow,
                //        BaitWeightGrams = request.BaitWeightGrams.Value
                //    };
                //    await _context.TrapBaitMeasurement.AddAsync(baitRecord, cancellationToken);
                //}

                await _context.SaveChangesAsync(cancellationToken);

                return newTrap.Id;
            }
        }
    }
}
