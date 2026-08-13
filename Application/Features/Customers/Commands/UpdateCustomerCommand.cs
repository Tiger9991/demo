using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Customers.Commands
{
    public record UpdateCustomerCommand(CustomerUpsertDto Data) : IRequest<bool>;
}
