using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.TrapGroups.Queries
{
    public record GetTrapGroupByIdQuery(Guid Id) : IRequest<TrapGroupDto?>;
}
