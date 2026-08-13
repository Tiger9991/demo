using Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.TrapGroups.Queries
{
    /// <summary>جلب كل مجموعات المحطات</summary>
    public record GetAllTrapGroupsQuery(string? Search = null) : IRequest<List<TrapGroupDto>>;
}
