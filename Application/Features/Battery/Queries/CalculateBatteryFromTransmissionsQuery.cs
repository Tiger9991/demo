using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Battery.Queries
{
    public class CalculateBatteryFromTransmissionsQuery : IRequest<BatteryCalculationDto>
    {
        public string TrapNumber { get; set; } = string.Empty;
        public int? TransmissionsCount { get; set; }  // optional; if null, use stored value
    }
}
