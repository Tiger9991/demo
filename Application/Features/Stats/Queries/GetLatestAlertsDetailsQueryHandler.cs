using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Application.Features.Stats.Queries
{
   

    public class GetLatestAlertsDetailsQueryHandler
        : IRequestHandler<GetLatestAlertsDetailsQuery, List<LatestAlertDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetLatestAlertsDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<LatestAlertDetailDto>> Handle(
            GetLatestAlertsDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var events = await _context.CaptureEvents
                .Include(c => c.Trap)
                .OrderByDescending(c => c.CaptureTime)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            var alerts = events.Select(c => new LatestAlertDetailDto
            {
                CaptureTime = c.CaptureTime,
                TrapNumber = c.Trap.TrapNumber,
                GroupNumber = c.Trap.TrapGroup,
                RodentType = c.RodentType.GetDisplayName(),
                Weight = c.RodentWeight.Grams,
                Length = c.RodentLength.Centimeters,
                SignalStrength = (float)c.SignalStrength,
                SignalQuality = Trap.CalculateSignalQuality(c.SignalStrength),
                NumberOfTransmissions = c.NumberOfTransmissions
            }).ToList();

            return alerts;
        }
    }
}
