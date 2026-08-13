using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Customers.Commands
{
    public record CreateCustomerCommand(CustomerUpsertDto Data) : IRequest<Guid>;
}
