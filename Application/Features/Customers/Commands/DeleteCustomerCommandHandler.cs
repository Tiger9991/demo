using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public sealed class DeleteCustomerCommandHandler
        : IRequestHandler<DeleteCustomerCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            DeleteCustomerCommand request,
            CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .Include(c => c.TrapGroups)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (customer is null) return false;

            // فك ربط المجموعات قبل الحذف (لا نحذف المجموعات)
            foreach (var group in customer.TrapGroups)
            {
                group.CustomerId = null;
                group.Customer = null;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
