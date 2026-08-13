using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetLatestAlertTimeQueryHandler
    : IRequestHandler<GetLatestAlertTimeQuery, LatestAlertTimeDto>
    {
        private readonly IApplicationDbContext _context;

        public GetLatestAlertTimeQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<LatestAlertTimeDto> Handle(
            GetLatestAlertTimeQuery request,
            CancellationToken cancellationToken)
        {
            var latestCapture = await _context.CaptureEvents
                .OrderByDescending(c => c.CaptureTime)
                .Select(c => new
                {
                    c.CaptureTime,
                    c.Trap.TrapNumber,
                    c.Trap.TrapGroup
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (latestCapture == null)
                return new LatestAlertTimeDto
                {
                    LatestCaptureTime = null,
                    Message = "لا توجد تنبيهات مسجلة."
                };

            return new LatestAlertTimeDto
            {
                LatestCaptureTime = latestCapture.CaptureTime,
                TrapNumber = latestCapture.TrapNumber,
                GroupNumber = latestCapture.TrapGroup,
                Message = $"آخر تنبيه: {latestCapture.CaptureTime:yyyy-MM-dd HH:mm} من المصيدة {latestCapture.TrapNumber}"
            };
        }
    }
}
