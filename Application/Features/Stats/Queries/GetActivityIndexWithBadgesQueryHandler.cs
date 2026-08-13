using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetActivityIndexWithBadgesQueryHandler
    : IRequestHandler<GetActivityIndexWithBadgesQuery, List<ActivityIndexWithBadgeDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetActivityIndexWithBadgesQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<ActivityIndexWithBadgeDto>> Handle(
            GetActivityIndexWithBadgesQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Get severity for all traps
            var severityHandler = new GetAllTrapsAverageSeverityQueryHandler(_context);
            var allSeverity = await severityHandler.Handle(
                new GetAllTrapsAverageSeverityQuery(request.GroupNumber),
                cancellationToken
            );

            // 2. Convert to badge DTO
            var result = allSeverity.Select(d => new ActivityIndexWithBadgeDto
            {
                TrapNumber = d.TrapNumber,
                GroupNumber = d.GroupNumber,
                Index = Math.Round(d.SeverityScore, 2),
                Level = d.SeverityLevel,
                BadgeColor = d.SeverityLevel switch
                {
                    "منخفض" => "#28a745",
                    "متوسط" => "#ffc107",
                    "مرتفع" => "#fd7e14",
                    "حرج" => "#dc3545",
                    "حرج جداً" => "#8b0000",
                    _ => "#6c757d"
                },
                BadgeIcon = d.SeverityLevel switch
                {
                    "منخفض" => "✅",
                    "متوسط" => "⚠️",
                    "مرتفع" => "🔶",
                    "حرج" => "🔴",
                    "حرج جداً" => "🚨",
                    _ => "⚪"
                }
            }).OrderByDescending(d => d.Index).ToList();

            return result;
        }
    }
}
