using MediatR;
using System;

namespace Application.Features.Customers.Commands
{
    /// <summary>فك ربط مجموعة محطات من العميل</summary>
    public record UnassignTrapGroupFromCustomerCommand(Guid TrapGroupId) : IRequest<bool>;
}
