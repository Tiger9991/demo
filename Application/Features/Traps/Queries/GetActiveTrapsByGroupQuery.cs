using Application.DTOs;
using MediatR;

namespace Application.Features.Traps.Queries
{
    public sealed record GetActiveTrapsByGroupQuery(
    string? GroupNumber = null,
    string? Status = "Active",
    int? Take = null,
    Guid? CustomerId = null
) : IRequest<List<TrapDto>>;
}
