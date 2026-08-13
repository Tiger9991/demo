using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetPeakHourSummaryQuery(
    string? GroupNumber = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<PeakHourSummaryDto>;
}
