using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record GetCombinedSeverityScoreQuery(
    string? GroupNumber = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<CombinedSeverityDto>;
}
