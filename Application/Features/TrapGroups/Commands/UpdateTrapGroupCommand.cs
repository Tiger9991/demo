using Application.DTOs;
using MediatR;

namespace Application.Features.TrapGroups.Commands
{
    public record UpdateTrapGroupCommand(TrapGroupUpsertDto Data) : IRequest<bool>;
}
