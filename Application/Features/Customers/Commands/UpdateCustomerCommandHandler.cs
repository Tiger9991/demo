using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public sealed class UpdateCustomerCommandHandler
        : IRequestHandler<UpdateCustomerCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            UpdateCustomerCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Data.Id is null) return false;

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.Data.Id, cancellationToken);

            if (customer is null) return false;

            customer.Name = request.Data.Name.Trim();
            customer.CustomerType = request.Data.CustomerType;
            customer.Email = request.Data.Email?.Trim();
            customer.Phone = request.Data.Phone?.Trim();
            customer.Address = request.Data.Address?.Trim();
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
