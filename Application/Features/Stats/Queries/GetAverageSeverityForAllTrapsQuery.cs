using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetAverageSeverityForAllTrapsQuery(
    string? GroupNumber = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<List<AverageSeverityDto>>;
}
