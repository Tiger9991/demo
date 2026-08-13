using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetAverageDailyVisitsSummaryQuery(
    string? GroupNumber = null,
    int? Days = 30
) : IRequest<AverageDailyVisitsSummaryDto>;
}
