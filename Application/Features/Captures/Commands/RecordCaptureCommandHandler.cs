using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace Application.Features.Captures.Commands
{
    public class RecordCaptureCommandHandler : IRequestHandler<RecordCaptureCommand, string>
    {
        private readonly IApplicationDbContext _context;

        public RecordCaptureCommandHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<string> Handle(RecordCaptureCommand request, CancellationToken cancellationToken)
        {
            // 1. Find the active trap (by number and group)
            var trapGroupSearch = request.trapGroup ?? string.Empty;
            var trap = await _context.Traps
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber
                                       && t.TrapGroup == trapGroupSearch
                                       && t.status == "Active", cancellationToken);
            if (trap == null)
                throw new Exception($"Active trap with number '{request.TrapNumber}' and group '{trapGroupSearch}' not found.");

            // 2. Create the CaptureEvent
            var capture = new CaptureEvent
            {
                TrapId = trap.Id,
                CaptureTime = DateTime.UtcNow,
                RodentWeight = new RodentWeight(request.weight),
                SignalStrength = request.SignalStrength,
                NumberOfTransmissions = trap.TotalTransmissions + 1,
                Status = "Active"
            };

            // 3. Set length (from sensor or direct value)
            //if (request.RodentLengthCm.HasValue && request.RodentLengthCm.Value > 0)
            //    capture.SetLengthFromValue(request.RodentLengthCm.Value);
            //else
                capture.SetLengthFromSensors(request.ir);

            capture.DetermineRodentType();

            // 4. Update trap statistics
            trap.LastEntryDate = capture.CaptureTime;
            trap.TotalTransmissions = capture.NumberOfTransmissions;
            trap.OperatingDays = (int)(DateTime.UtcNow - trap.StartTime).TotalDays;
            trap.SignalStrength = request.SignalStrength;
            trap.UpdateBattery();
            trap.UpdateIndicatorStatus();

            // 5. Optionally create BaitMeasurement
            if (request.bWeight > 0)
            {
                var bait = new Domain.Entities.BaitMeasurement
                {
                    Id = Guid.NewGuid(),
                    TrapId = trap.Id,
                    CaptureEventId = capture.Id,
                    MeasurementTime = capture.CaptureTime,
                    BaitWeightGrams = request.bWeight
                };
                _context.BaitMeasurements.Add(bait);
            }

            // 6. Save
            _context.CaptureEvents.Add(capture);
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Return "ok" on success
            return "ok";
        }
    }
}