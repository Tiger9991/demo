using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Battery.Queries
{
    public record GetBatteryStatusQuery(Guid TrapId) : IRequest<BatteryStatusDto>;
}
