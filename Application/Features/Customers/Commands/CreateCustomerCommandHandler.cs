using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public sealed class CreateCustomerCommandHandler
        : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(
            CreateCustomerCommand request,
            CancellationToken cancellationToken)
        {
            // توليد رقم العميل التسلسلي: CUS-YYYY-NNNN
            var year = DateTime.UtcNow.Year;
            var count = await _context.Customers.CountAsync(cancellationToken);
            var customerNumber = $"CUS-{year}-{(count + 1):D4}";

            var customer = new Domain.Entities.Customers
            {
                Id = Guid.NewGuid(),
                CustomerNumber = customerNumber,
                Name = request.Data.Name.Trim(),
                CustomerType = request.Data.CustomerType,
                Email = request.Data.Email?.Trim(),
                Phone = request.Data.Phone?.Trim(),
                Address = request.Data.Address?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            return customer.Id;
        }
    }
}
