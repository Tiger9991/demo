using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetRodentActivityDetailsQuery(
     string? GroupNumber = null,
     string? Status = null,
     DateTime? FromDate = null,
     DateTime? ToDate = null
 ) : IRequest<List<ActiveTrapSummaryDto>>;
}
