using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public sealed class GetCustomerByIdQueryHandler
        : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto?> Handle(
            GetCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var c = await _context.Customers
                .Include(x => x.TrapGroups)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (c is null) return null;

            return new CustomerDto
            {
                Id = c.Id,
                CustomerNumber = c.CustomerNumber,
                Name = c.Name,
                CustomerType = c.CustomerType,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                TrapGroupCount = c.TrapGroups.Count,
                TrapGroupNumbers = c.TrapGroups.Select(g => g.TrapGroup).Distinct().ToList(),
                CreatedAt = c.CreatedAt
            };
        }
    }
}
