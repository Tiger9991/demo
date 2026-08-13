using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Queries
{
    public class CalculateAverageBaitConsumptionQueryHandler : IRequestHandler<CalculateAverageBaitConsumptionQuery, BaitConsumptionDto>
    {
        private readonly IApplicationDbContext _context;

        public CalculateAverageBaitConsumptionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaitConsumptionDto> Handle(CalculateAverageBaitConsumptionQuery request, CancellationToken cancellationToken)
        {
            // 1. Validate inputs
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

            // 2. Find the trap by its number to get the Guid TrapId
            var trap = await _context.Traps
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber, cancellationToken);

            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.TrapNumber);

            Guid trapId = trap.Id;

            // 3. Calculate average consumption
            double average = request.BaitWeightGrams / (double)request.NumberOfTransmissions;

            // 4. (Optional) Save to BaitMeasurements table
            DateTime measurementTime = request.MeasurementTime ?? DateTime.UtcNow;

            //if (request.SaveToDatabase)
            //{
            //    var baitMeasurement = new BaitMeasurement
            //    {
            //        Id = Guid.NewGuid(),
            //        TrapId = trapId,
            //        MeasurementTime = measurementTime,
            //        BaitWeightGrams = request.BaitWeightGrams
            //    };
            //    await _context.BaitMeasurements.AddAsync(baitMeasurement, cancellationToken);
            //    await _context.SaveChangesAsync(cancellationToken);
            //}

            // 5. Build and return DTO
            return new BaitConsumptionDto
            {
                TrapNumber = request.TrapNumber,
                BaitConsumedGrams = request.BaitWeightGrams,
                NumberOfTransmissions = request.NumberOfTransmissions,
                AverageConsumptionPerRodent = average,
             //   MeasurementTime = request.SaveToDatabase ? measurementTime : null,
                Message = $"Average bait consumption per rodent: {average:F2} grams (based on {request.NumberOfTransmissions} transmissions)."
            };
        }
    }
}
