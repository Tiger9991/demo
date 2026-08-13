using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BaitMeasurement.Queries
{
  

    public class GetBaitMeasurementByIdQueryHandler
        : IRequestHandler<GetBaitMeasurementByIdQuery, BaitMeasurementDto>
    {
        private readonly IApplicationDbContext _context;

        public GetBaitMeasurementByIdQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<BaitMeasurementDto> Handle(
            GetBaitMeasurementByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Load the bait measurement with its related CaptureEvent and Trap
            var measurement = await _context.BaitMeasurements
                .Include(b => b.CaptureEvent)
                    .ThenInclude(ce => ce.Trap)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (measurement == null)
                throw new Exception($"BaitMeasurement with ID '{request.Id}' not found.");

            // Map to DTO
            return new BaitMeasurementDto
            {
                Id = measurement.Id,
                CaptureEventId = measurement.CaptureEventId,
                TrapNumber = measurement.CaptureEvent?.Trap?.TrapNumber ?? string.Empty,
                GroupNumber = measurement.CaptureEvent?.Trap?.TrapGroup ?? string.Empty,
                MeasurementTime = measurement.MeasurementTime,
                BaitWeightGrams = measurement.BaitWeightGrams
            };
        }
    }
}
