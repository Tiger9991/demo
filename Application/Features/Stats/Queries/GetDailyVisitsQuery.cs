using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetDailyVisitsQuery(
    string? GroupNumber = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? Days = 30
) : IRequest<List<DailyVisitDto>>;
}
