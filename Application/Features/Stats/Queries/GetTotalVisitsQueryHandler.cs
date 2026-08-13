using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetTotalVisitsQueryHandler
    : IRequestHandler<GetTotalVisitsQuery, int>
    {
        private readonly IApplicationDbContext _context;

        public GetTotalVisitsQueryHandler(IApplicationDbContext context)
            => _context = context;

        public async Task<int> Handle(GetTotalVisitsQuery request, CancellationToken ct)
        {
            // Use AsQueryable() to ensure EF Core CountAsync is used
            return await _context.CaptureEvents.AsQueryable().CountAsync(ct);
        }
    }
}
