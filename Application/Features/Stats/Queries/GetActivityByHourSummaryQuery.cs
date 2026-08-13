using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetActivityByHourSummaryQuery(
    string? GroupNumber = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<ActivityByHourSummaryDto>;
}
