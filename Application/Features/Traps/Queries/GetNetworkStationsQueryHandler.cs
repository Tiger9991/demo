using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Traps.Queries
{
    public sealed class GetNetworkStationsQueryHandler : IRequestHandler<GetNetworkStationsQuery, List<NetworkStationDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetNetworkStationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NetworkStationDto>> Handle(GetNetworkStationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TrapGroups
                .Include(g => g.Customer)
                .AsNoTracking()
                .AsQueryable();

            if (request.CustomerId.HasValue)
            {
                query = query.Where(g => g.CustomerId == request.CustomerId.Value);
            }
            else
            {
                query = query.Where(g => g.CustomerId != null);
            }

            var trapGroups = await query
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);

            var distinctTrapGroups = trapGroups
                .DistinctBy(g => new { g.TrapGroup, g.TrapNumber })
                .OrderBy(g => g.TrapGroup)
                .ThenBy(g => g.TrapNumber)
                .ToList();

            // Fetch physical traps to join status and battery
            var physicalTraps = await _context.Traps
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new List<NetworkStationDto>();

            foreach (var tg in distinctTrapGroups)
            {
                // Find matching physical trap (if any)
                var physicalTrap = physicalTraps
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault(t => t.TrapGroup == tg.TrapGroup && t.TrapNumber == tg.TrapNumber);

                result.Add(new NetworkStationDto
                {
                    Id = tg.Id,
                    TrapGroup = tg.TrapGroup,
                    TrapNumber = tg.TrapNumber,
                    Description = tg.Description,
                    CustomerName = tg.Customer?.Name ?? "-",
                    CustomerNumber = tg.Customer?.CustomerNumber ?? "-",
                    IsActive = physicalTrap?.status == "Active",
                    Status = physicalTrap?.status ?? "غير متصل",
                    BatteryPercentage = physicalTrap?.BatteryPercentage,
                    CreatedAt = tg.CreatedAt
                });
            }

            return result;
        }
    }
}
