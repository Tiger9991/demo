using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TrapGroups.Queries
{
    public sealed class GetTrapGroupByIdQueryHandler
        : IRequestHandler<GetTrapGroupByIdQuery, TrapGroupDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetTrapGroupByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrapGroupDto?> Handle(
            GetTrapGroupByIdQuery request,
            CancellationToken cancellationToken)
        {
            var g = await _context.TrapGroups
                .Include(x => x.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (g is null) return null;

            return new TrapGroupDto
            {
                Id = g.Id,
                TrapNumber = g.TrapNumber,
                TrapGroup = g.TrapGroup,
                Description = g.Description,
                CustomerId = g.CustomerId,
                CustomerName = g.Customer?.Name,
                CustomerNumber = g.Customer?.CustomerNumber,
                CreatedAt = g.CreatedAt
            };
        }
    }
}
