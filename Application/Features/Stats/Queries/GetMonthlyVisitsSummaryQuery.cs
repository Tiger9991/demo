using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetMonthlyVisitsSummaryQuery(
    string? GroupNumber = null,
    int? MonthOffset = 0  // 0 = current month, -1 = last month, etc.
) : IRequest<MonthlyVisitsSummaryDto>;
}
