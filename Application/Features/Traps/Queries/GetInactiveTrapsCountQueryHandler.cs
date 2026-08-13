using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetInactiveTrapsCountQueryHandler
    : IRequestHandler<GetInactiveTrapsCountQuery, int>
    {
        private readonly IApplicationDbContext _context;

        public GetInactiveTrapsCountQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(GetInactiveTrapsCountQuery request, CancellationToken cancellationToken)
        {
            return await _context.Traps
                .CountAsync(t => t.status != "Active", cancellationToken);
        }
    }
}
