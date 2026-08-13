using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public sealed class UnassignTrapGroupFromCustomerCommandHandler
        : IRequestHandler<UnassignTrapGroupFromCustomerCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UnassignTrapGroupFromCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            UnassignTrapGroupFromCustomerCommand request,
            CancellationToken cancellationToken)
        {
            var group = await _context.TrapGroups
                .FirstOrDefaultAsync(g => g.Id == request.TrapGroupId, cancellationToken);

            if (group is null) return false;

            group.CustomerId = null;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
