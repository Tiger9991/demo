using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.BaitMeasurement.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

public class GetAllBaitMeasurementsQueryHandler
    : IRequestHandler<GetAllBaitMeasurementsQuery, List<BaitMeasurementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBaitMeasurementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BaitMeasurementDto>> Handle(
        GetAllBaitMeasurementsQuery request,
        CancellationToken cancellationToken)
    {
        var measurements = await _context.BaitMeasurements
            .Include(b => b.CaptureEvent)
                .ThenInclude(ce => ce.Trap)
            .OrderByDescending(b => b.MeasurementTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Manual mapping to DTO (you can also use AutoMapper)
        return measurements.Select(b => new BaitMeasurementDto
        {
            Id = b.Id,
            CaptureEventId = b.CaptureEventId,
            TrapNumber = b.CaptureEvent?.Trap?.TrapNumber ?? string.Empty,
            GroupNumber = b.CaptureEvent?.Trap?.TrapGroup ?? string.Empty,
            MeasurementTime = b.MeasurementTime,
            BaitWeightGrams = b.BaitWeightGrams
        }).ToList();
    }
}
