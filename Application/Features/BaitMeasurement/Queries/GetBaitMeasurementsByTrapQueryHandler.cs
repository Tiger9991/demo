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
    //public class GetBaitMeasurementByIdQueryHandler
    //  : IRequestHandler<GetBaitMeasurementByIdQuery, BaitMeasurementDto>
    //{
    //    private readonly IApplicationDbContext _context;

    //    public GetBaitMeasurementByIdQueryHandler(IApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<BaitMeasurementDto> Handle(
    //        GetBaitMeasurementByIdQuery request,
    //        CancellationToken cancellationToken)
    //    {
    //        var measurement = await _context.BaitMeasurements
    //            .Include(b => b.CaptureEvent)
    //                .ThenInclude(ce => ce.Trap)
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

    //        if (measurement == null)
    //            throw new Exception($"BaitMeasurement with ID '{request.Id}' not found.");

    //        // Map to DTO manually (or use AutoMapper if set up)
    //        return new BaitMeasurementDto
    //        {
    //            Id = measurement.Id,
    //            CaptureEventId = measurement.CaptureEventId,
    //            TrapNumber = measurement.CaptureEvent?.Trap?.TrapNumber ?? string.Empty,
    //            GroupNumber = measurement.CaptureEvent?.Trap?.TrapGroup ?? string.Empty,
    //            MeasurementTime = measurement.MeasurementTime,
    //            BaitWeightGrams = measurement.BaitWeightGrams
    //        };
    //    }

    //}
}
