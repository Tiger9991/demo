using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetBaitConsumptionDetailsQueryHandler
    : IRequestHandler<GetBaitConsumptionDetailsQuery, List<BaitConsumptionDetailsDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBaitConsumptionDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<BaitConsumptionDetailsDto>> Handle(
            GetBaitConsumptionDetailsQuery request,
            CancellationToken cancellationToken)
        {
            // Start with BaitMeasurements, join with Trap
            var query = _context.BaitMeasurements
                .Include(b => b.Trap)
                .AsQueryable();

            // Filter by group if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
                query = query.Where(b => b.Trap.TrapGroup == request.GroupNumber);

            // Filter by date range if provided
            if (request.FromDate.HasValue)
                query = query.Where(b => b.MeasurementTime >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(b => b.MeasurementTime <= request.ToDate.Value);

            var measurements = await query
                .OrderBy(b => b.TrapId)
                .ThenBy(b => b.MeasurementTime)
                .ToListAsync(cancellationToken);

            var result = new List<BaitConsumptionDetailsDto>();

            foreach (var group in measurements.GroupBy(b => b.TrapId))
            {
                var list = group.ToList();
                var first = list.First();
                double consumed = 0.0;
                double totalWeight = 0.0;

                for (int i = 1; i < list.Count; i++)
                {
                    var diff = list[i - 1].BaitWeightGrams - list[i].BaitWeightGrams;
                    if (diff > 0)
                    {
                        consumed += diff;
                    }
                }

                foreach (var b in list)
                {
                    totalWeight += b.BaitWeightGrams;
                }

                result.Add(new BaitConsumptionDetailsDto
                {
                    TrapNumber = first.Trap.TrapNumber,
                    GroupNumber = first.Trap.TrapGroup ?? "Unassigned",
                    TotalConsumed = Math.Round(consumed, 2),
                    MeasurementCount = list.Count,
                    AveragePerMeasurement = Math.Round(totalWeight / list.Count, 2)
                });
            }

            return result
                .OrderBy(dto => dto.GroupNumber)
                .ThenBy(dto => dto.TrapNumber)
                .ToList();
        }
    }
}
