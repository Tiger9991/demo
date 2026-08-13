using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Queries
{
    public record GetBaitMeasurementByIdQuery(Guid Id) : IRequest<BaitMeasurementDto>;
}
