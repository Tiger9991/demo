using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Commands
{
    public record DeleteBaitMeasurementCommand(Guid Id) : IRequest;
}
