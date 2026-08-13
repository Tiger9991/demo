using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Traps.Queries
{
    public class GetDistinctGroupNumbersQueryHandler
    : IRequestHandler<GetDistinctGroupNumbersQuery, List<string>>
    {
        private readonly IApplicationDbContext _context;

        public GetDistinctGroupNumbersQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<List<string>> Handle(
            GetDistinctGroupNumbersQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Traps
                .Select(t => t.TrapGroup)
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync(cancellationToken);
        }
    }
}
