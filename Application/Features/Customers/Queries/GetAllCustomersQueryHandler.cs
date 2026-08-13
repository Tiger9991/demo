using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public sealed class GetAllCustomersQueryHandler
        : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllCustomersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerDto>> Handle(
            GetAllCustomersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Customers
                .Include(c => c.TrapGroups)
                .AsNoTracking()
                .AsQueryable();

            // بحث بالاسم أو رقم العميل أو الهاتف
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.CustomerNumber.ToLower().Contains(search) ||
                    (c.Phone != null && c.Phone.Contains(search)) ||
                    (c.Email != null && c.Email.ToLower().Contains(search)));
            }

            var customers = await query
                .OrderBy(c => c.CustomerNumber)
                .ToListAsync(cancellationToken);

            return customers.Select(c => new CustomerDto
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
            }).ToList();
        }
    }
}
