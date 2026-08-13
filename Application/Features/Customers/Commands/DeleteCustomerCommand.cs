using MediatR;
using System;

namespace Application.Features.Customers.Commands
{
    public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;
}
