using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Battery.Commands
{
    public record RecalculateBatteryCommand(Guid TrapId) : IRequest<BatteryStatusDto>;
}
