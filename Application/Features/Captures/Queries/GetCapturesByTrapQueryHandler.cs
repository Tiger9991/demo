using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Queries
{
    public class GetCapturesByTrapQueryHandler
    : IRequestHandler<GetCapturesByTrapQuery, List<CaptureEventDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCapturesByTrapQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CaptureEventDto>> Handle(
            GetCapturesByTrapQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Find the trap by its number
            var trap = await _context.Traps
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrapNumber == request.TrapNumber, cancellationToken);
            if (trap == null)
                throw new Exception($"Trap with number '{request.TrapNumber}' not found.");

            // 2. Optional: validate group if provided
            if (!string.IsNullOrEmpty(request.GroupNumber) && trap.TrapGroup != request.GroupNumber)
                throw new Exception($"Trap '{request.TrapNumber}' does not belong to group '{request.GroupNumber}'.");

            // 3. Query capture events for this trap
            var captureQuery = _context.CaptureEvents
                .Where(c => c.TrapId == trap.Id)
                .OrderByDescending(c => c.CaptureTime);

            // 4. Project to DTO
            var captures = await captureQuery
                .Select(c => new CaptureEventDto
                {
                   
                    TrapNumber = trap.TrapNumber,
                    trapGroup = trap.TrapGroup,
                    CaptureTime = c.CaptureTime,
                    
                    SignalStrength = (float)c.SignalStrength,
                    SignalQuality = Trap.CalculateSignalQuality(c.SignalStrength),
                    NumberOfTransmissions = c.NumberOfTransmissions,
                   
                })
                .ToListAsync(cancellationToken);

            return captures;
        }
    }
}
