using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetCombinedSeverityScoreQueryHandler
    : IRequestHandler<GetCombinedSeverityScoreQuery, CombinedSeverityDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCombinedSeverityScoreQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<CombinedSeverityDto> Handle(
            GetCombinedSeverityScoreQuery request,
            CancellationToken cancellationToken)
        {
            // Reuse the handler that computes per‑trap severity (list)
            var perTrapHandler = new GetAllTrapsAverageSeverityQueryHandler(_context);
            var perTrapData = await perTrapHandler.Handle(
                new GetAllTrapsAverageSeverityQuery(request.GroupNumber, request.FromDate, request.ToDate),
                cancellationToken
            );

            // If no traps, return 0
            if (!perTrapData.Any())
                return new CombinedSeverityDto
                {
                    AverageSeverityScore = 0,
                    TotalTraps = 0,
                    Message = "No traps found in the specified period."
                };

            // Compute the average of all severity scores
            var averageScore = perTrapData.Average(d => d.SeverityScore);

            return new CombinedSeverityDto
            {
                AverageSeverityScore = Math.Round(averageScore, 2),
                TotalTraps = perTrapData.Count,
                Message = $"Average severity score across {perTrapData.Count} traps."
            };
        }
    }
}
