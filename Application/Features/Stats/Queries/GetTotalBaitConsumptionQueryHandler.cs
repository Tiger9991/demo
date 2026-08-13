using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public class GetTotalBaitConsumptionQueryHandler
    : IRequestHandler<GetTotalBaitConsumptionQuery, double>
    {
        private readonly IApplicationDbContext _context;
        public GetTotalBaitConsumptionQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<double> Handle(GetTotalBaitConsumptionQuery request, CancellationToken ct)
        {
            var measurements = await _context.BaitMeasurements
                .OrderBy(b => b.TrapId)
                .ThenBy(b => b.MeasurementTime)
                .ToListAsync(ct);

            double totalConsumed = 0;
            foreach (var group in measurements.GroupBy(b => b.TrapId))
            {
                var list = group.ToList();
                for (int i = 1; i < list.Count; i++)
                {
                    var diff = list[i - 1].BaitWeightGrams - list[i].BaitWeightGrams;
                    if (diff > 0)
                    {
                        totalConsumed += diff;
                    }
                }
            }

            return Math.Round(totalConsumed, 2);
        }
    }
}
