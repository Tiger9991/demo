using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetLowBatteryCountQuery(
    int Threshold = 25,
    string? Status = "Active",
   string? GroupNumber = null
) : IRequest<LowBatteryCountDto>;
}
