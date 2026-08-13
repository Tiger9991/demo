using Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Customers.Queries
{
    /// <summary>جلب كل العملاء مع إمكانية البحث</summary>
    public record GetAllCustomersQuery(string? Search = null) : IRequest<List<CustomerDto>>;
}
