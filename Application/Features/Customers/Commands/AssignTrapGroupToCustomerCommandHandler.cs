using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public sealed class AssignTrapGroupToCustomerCommandHandler
        : IRequestHandler<AssignTrapGroupToCustomerCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public AssignTrapGroupToCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            AssignTrapGroupToCustomerCommand request,
            CancellationToken cancellationToken)
        {
            var group = await _context.TrapGroups
                .FirstOrDefaultAsync(g => g.Id == request.TrapGroupId, cancellationToken);

            if (group is null) return false;

            // تحقق أن العميل موجود
            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists) return false;

            group.CustomerId = request.CustomerId;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
