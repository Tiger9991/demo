using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetInactiveTrapsQueryHandler
    : IRequestHandler<GetInactiveTrapsQuery, List<InactiveTrapDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetInactiveTrapsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InactiveTrapDto>> Handle(
            GetInactiveTrapsQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Traps
                .Where(t => t.status != "Active")
                .Select(t => new InactiveTrapDto
                {
                    TrapNumber = t.TrapNumber,
                    GroupNumber = t.TrapGroup ?? "Unassigned"
                })
                .OrderBy(t => t.GroupNumber)
                .ThenBy(t => t.TrapNumber)
                .ToListAsync(cancellationToken);
        }
    }
}
