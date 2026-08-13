using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetLowBatteryTrapsDetailQuery(int Threshold = 30, string? GroupNumber = null) : IRequest<List<LowBatteryTrapDto>>;
}
