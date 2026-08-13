using MediatR;
using System;

namespace Application.Features.Customers.Commands
{
    /// <summary>ربط مجموعة محطات بعميل</summary>
    public record AssignTrapGroupToCustomerCommand(Guid CustomerId, Guid TrapGroupId) : IRequest<bool>;
}
