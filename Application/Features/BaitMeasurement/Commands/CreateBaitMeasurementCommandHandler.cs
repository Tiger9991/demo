using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.BaitMeasurement.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


public class CreateBaitMeasurementCommandHandler
    : IRequestHandler<CreateBaitMeasurementCommand, BaitMeasurementDto>
{
    private readonly IApplicationDbContext _context;

    public CreateBaitMeasurementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BaitMeasurementDto> Handle(
        CreateBaitMeasurementCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate that the CaptureEvent exists
        var captureEvent = await _context.CaptureEvents
            .Include(ce => ce.Trap)  // Include Trap to get TrapNumber and TrapGroup later
            .FirstOrDefaultAsync(ce => ce.Id == request.CaptureEventId, cancellationToken);
        if (captureEvent == null)
            throw new Exception($"CaptureEvent with ID '{request.CaptureEventId}' not found.");

        // 2. Create the BaitMeasurement
        var baitMeasurement = new BaitMeasurement
        {
            Id = Guid.NewGuid(),
            TrapId = captureEvent.TrapId,
            CaptureEventId = request.CaptureEventId,
            MeasurementTime = request.MeasurementTime,
            BaitWeightGrams = request.BaitWeightGrams
        };

        // 3. Add to context and save
        await _context.BaitMeasurements.AddAsync(baitMeasurement, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // 4. Build DTO using data from the linked CaptureEvent
        return new BaitMeasurementDto
        {
            Id = baitMeasurement.Id,
            CaptureEventId = baitMeasurement.CaptureEventId,
            TrapNumber = captureEvent.Trap?.TrapNumber ?? string.Empty,
            GroupNumber = captureEvent.Trap?.TrapGroup ?? string.Empty,
            MeasurementTime = baitMeasurement.MeasurementTime,
            BaitWeightGrams = baitMeasurement.BaitWeightGrams
        };
    }
}
