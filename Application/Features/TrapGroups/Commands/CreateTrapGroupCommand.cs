using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.TrapGroups.Commands
{
    public record CreateTrapGroupCommand(TrapGroupUpsertDto Data) : IRequest<Guid>;
}
