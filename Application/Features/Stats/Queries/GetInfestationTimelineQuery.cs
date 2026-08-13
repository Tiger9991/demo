using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Stats.Queries
{
    public record GetInfestationTimelineQuery(
        string Timeframe, // "daily" or "monthly"
        string? GroupNumber = null
    ) : IRequest<InfestationTimelineDto>;
}
