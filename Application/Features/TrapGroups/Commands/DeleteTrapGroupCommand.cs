using MediatR;
using System;

namespace Application.Features.TrapGroups.Commands
{
    public record DeleteTrapGroupCommand(Guid Id) : IRequest<bool>;
}
