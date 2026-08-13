using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Commands
{
    public class CalculateAverageBaitConsumptionCommandHandler : IRequestHandler<CalculateAverageBaitConsumptionCommand, BaitConsumptionDto>
    {
        private readonly IApplicationDbContext _context;

        public CalculateAverageBaitConsumptionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaitConsumptionDto> Handle(CalculateAverageBaitConsumptionCommand request, CancellationToken cancellationToken)
        {
            if (request.NumberOfTransmissions <= 0)
            {
                return new BaitConsumptionDto
                {
                    TrapNumber = request.TrapNumber,
                    BaitConsumedGrams = request.BaitWeightGrams,
                    NumberOfTransmissions = request.NumberOfTransmissions,
                    AverageConsumptionPerRodent = 0,
                    Message = "No transmissions recorded. Cannot calculate average."
                };
            }

            if (request.BaitWeightGrams <= 0)
            {
                return new BaitConsumptionDto
                {
                    TrapNumber = request.TrapNumber,
                    BaitConsumedGrams = request.BaitWeightGrams,
                    NumberOfTransmissions = request.NumberOfTransmissions,
                    AverageConsumptionPerRodent = 0,
                    Message = "Bait consumption is zero or negative. No consumption recorded."
                };
            }

            // Lookup trap by number
            var trap = await _context.Traps
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber, cancellationToken);
            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.TrapNumber);

            double avg = request.BaitWeightGrams / request.NumberOfTransmissions;
            DateTime measurementTime = request.MeasurementTime ?? DateTime.UtcNow;

            //if (request.SaveToDatabase)
            //{
            //    var measurement = new BaitMeasurement
            //    {
            //        Id = Guid.NewGuid(),
            //        TrapId = trap.Id,
            //        MeasurementTime = measurementTime,
            //        BaitWeightGrams = request.BaitWeightGrams
            //    };
            //    await _context.BaitMeasurements.AddAsync(measurement, cancellationToken);
            //    await _context.SaveChangesAsync(cancellationToken);
            //}

            return new BaitConsumptionDto
            {
                TrapNumber = request.TrapNumber,
                BaitConsumedGrams = request.BaitWeightGrams,
                NumberOfTransmissions = request.NumberOfTransmissions,
                AverageConsumptionPerRodent = avg,
              //  MeasurementTime = request.SaveToDatabase ? measurementTime : null,
                Message = $"Average bait consumption per rodent: {avg:F2} grams (based on {request.NumberOfTransmissions} transmissions)."
            };
        }
    }
}
