using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    using Application.DTOs;
    using MediatR;
    
    public record GetVisitPatternDetailsQuery(
        string? GroupNumber = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null
    ) : IRequest<List<VisitPatternDetailDto>>;
}
