using Application.Common.Interfaces;
using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public class GetAllTrapsQueryHandler : IRequestHandler<GetAllTrapsQuery, List<TrapDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllTrapsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TrapDto>> Handle(GetAllTrapsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Traps.AsNoTracking().AsQueryable();

            var assignedTrapsQuery = _context.TrapGroups.AsNoTracking().AsQueryable();
            if (request.CustomerId.HasValue)
            {
                assignedTrapsQuery = assignedTrapsQuery.Where(g => g.CustomerId == request.CustomerId.Value);
            }
            else
            {
                assignedTrapsQuery = assignedTrapsQuery.Where(g => g.CustomerId != null);
            }

            var assignedGroupNumbers = await assignedTrapsQuery
                .Select(g => g.TrapGroup)
                .Distinct()
                .ToListAsync(cancellationToken);

            query = query.Where(t => assignedGroupNumbers.Contains(t.TrapGroup));

            var traps = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<TrapDto>>(traps);
        }
    }
}
