using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Customers.Queries
{
    public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;
}
