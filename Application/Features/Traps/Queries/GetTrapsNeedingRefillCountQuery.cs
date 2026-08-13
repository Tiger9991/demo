using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetTrapsNeedingRefillCountQuery(
    double Threshold = 50.0,
    string? Status = "Active",
    string? GroupNumber = null
) : IRequest<RefillCountDto>;
}
