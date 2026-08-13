using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TrapGroups.Queries
{
    public sealed class GetAllTrapGroupsQueryHandler
        : IRequestHandler<GetAllTrapGroupsQuery, List<TrapGroupDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllTrapGroupsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrapGroupDto>> Handle(
            GetAllTrapGroupsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.TrapGroups
                .Include(g => g.Customer)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(g =>
                    g.TrapNumber.ToLower().Contains(search) ||
                    g.TrapGroup.ToLower().Contains(search) ||
                    (g.Description != null && g.Description.ToLower().Contains(search)) ||
                    (g.Customer != null && g.Customer.Name.ToLower().Contains(search)));
            }

            var groups = await query
                .OrderBy(g => g.TrapGroup)
                .ThenBy(g => g.TrapNumber)
                .ToListAsync(cancellationToken);

            return groups.Select(g => new TrapGroupDto
            {
                Id = g.Id,
                TrapNumber = g.TrapNumber,
                TrapGroup = g.TrapGroup,
                Description = g.Description,
                CustomerId = g.CustomerId,
                CustomerName = g.Customer?.Name,
                CustomerNumber = g.Customer?.CustomerNumber,
                CreatedAt = g.CreatedAt
            }).ToList();
        }
    }
}
