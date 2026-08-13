using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetTrapsMapDataQuery(string? GroupNumber = null, int? Limit = null) : IRequest<List<TrapDetailDto>>;
}
