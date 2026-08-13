using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Commands
{
    public class CreateBaitMeasurementCommand : IRequest<BaitMeasurementDto>
    {
        public Guid CaptureEventId { get; set; }
        public DateTime MeasurementTime { get; set; }
        public double BaitWeightGrams { get; set; }
    }
}
