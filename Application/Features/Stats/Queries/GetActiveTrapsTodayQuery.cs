using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Stats.Queries
{
    public record GetActiveTrapsTodayQuery(DateTime? Date = null, string? GroupNumber = null) : IRequest<ActiveTrapsTodayDto>;
}
