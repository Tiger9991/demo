using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetTotalTrapsQueryHandler : IRequestHandler<GetTotalTrapsQuery, TrapsTotalDto>
    {
        private readonly IApplicationDbContext _context;

        public GetTotalTrapsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrapsTotalDto> Handle(GetTotalTrapsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Traps.AsNoTracking().AsQueryable();

            // جلب المجموعات وأرقام المحطات التي تتبع لعميل (أو لعميل محدد إذا تم تمرير CustomerId)
            var assignedTrapsQuery = _context.TrapGroups.AsNoTracking().AsQueryable();
            if (request.CustomerId.HasValue)
            {
                assignedTrapsQuery = assignedTrapsQuery.Where(g => g.CustomerId == request.CustomerId.Value);
            }
            else
            {
                assignedTrapsQuery = assignedTrapsQuery.Where(g => g.CustomerId != null);
            }

            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
            {
                assignedTrapsQuery = assignedTrapsQuery.Where(g => g.TrapGroup == request.GroupNumber);
            }

            var count = await assignedTrapsQuery
                .Select(g => new { g.TrapGroup, g.TrapNumber })
                .Distinct()
                .CountAsync(cancellationToken);

            return new TrapsTotalDto { Total = count };
        }
    }
}
