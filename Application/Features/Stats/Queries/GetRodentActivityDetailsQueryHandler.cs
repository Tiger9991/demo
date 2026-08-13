using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Stats.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetRodentActivityDetailsQueryHandler
     : IRequestHandler<GetRodentActivityDetailsQuery, List<ActiveTrapSummaryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRodentActivityDetailsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<ActiveTrapSummaryDto>> Handle(
            GetRodentActivityDetailsQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Start with filtered traps
            var query = _context.Traps.AsQueryable();

            // 2. Apply status filter (default: "Active")
            var statusFilter = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status;
            query = query.Where(t => t.status == statusFilter);

            // 3. Apply group filter if provided
            if (!string.IsNullOrEmpty(request.GroupNumber))
                query = query.Where(t => t.TrapGroup == request.GroupNumber);

            // 4. Only include traps that have at least one capture event in the date range
            query = query.Where(t => _context.CaptureEvents.Any(c => c.TrapId == t.Id 
                && (!request.FromDate.HasValue || c.CaptureTime >= request.FromDate.Value)
                && (!request.ToDate.HasValue || c.CaptureTime <= request.ToDate.Value)));

            var traps = await query.ToListAsync(cancellationToken);
            var trapIds = traps.Select(t => t.Id).ToList();

            // Fetch capture events for these traps in date range
            var captures = await _context.CaptureEvents
                .Where(c => trapIds.Contains(c.TrapId)
                    && (!request.FromDate.HasValue || c.CaptureTime >= request.FromDate.Value)
                    && (!request.ToDate.HasValue || c.CaptureTime <= request.ToDate.Value))
                .ToListAsync(cancellationToken);

            // Fetch bait measurements for these traps (do NOT filter by date here to compute intervals correctly)
            var baitMeasurements = await _context.BaitMeasurements
                .Where(b => trapIds.Contains(b.TrapId))
                .OrderBy(b => b.MeasurementTime)
                .ToListAsync(cancellationToken);

            var capturesGrouped = captures.GroupBy(c => c.TrapId).ToDictionary(g => g.Key, g => g.ToList());
            var baitGrouped = baitMeasurements.GroupBy(b => b.TrapId).ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ActiveTrapSummaryDto>();

            foreach (var t in traps)
            {
                capturesGrouped.TryGetValue(t.Id, out var trapCaptures);
                var trapCapturesList = trapCaptures ?? new List<CaptureEvent>();

                if (trapCapturesList.Count == 0)
                    continue;

                double consumed = 0.0;
                if (baitGrouped.TryGetValue(t.Id, out var trapBaits))
                {
                    for (int i = 1; i < trapBaits.Count; i++)
                    {
                        var prev = trapBaits[i - 1];
                        var curr = trapBaits[i];

                        // Filter by date range (interval ends at curr.MeasurementTime)
                        if (request.FromDate.HasValue && curr.MeasurementTime < request.FromDate.Value)
                            continue;
                        if (request.ToDate.HasValue && curr.MeasurementTime > request.ToDate.Value)
                            continue;

                        var diff = prev.BaitWeightGrams - curr.BaitWeightGrams;
                        if (diff > 0)
                        {
                            consumed += diff;
                        }
                    }
                }

                result.Add(new ActiveTrapSummaryDto
                {
                    TrapNumber = t.TrapNumber,
                    GroupNumber = t.TrapGroup ?? "Unassigned",
                    Status = t.status,
                    LastCaptureTime = trapCapturesList.Max(c => (DateTime?)c.CaptureTime),
                    TotalCaptures = trapCapturesList.Count,
                    TotalBaitConsumed = Math.Round(consumed, 2)
                });
            }

            return result
                .OrderBy(dto => dto.GroupNumber)
                .ThenBy(dto => dto.TrapNumber)
                .DistinctBy(dto => new { dto.GroupNumber, dto.TrapNumber })
                .ToList();
        }
    }
}

