using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Commands
{
    public class CalculateAverageBaitConsumptionCommand : IRequest<BaitConsumptionDto>
    {
        public string TrapNumber { get; set; } = string.Empty;
        public double BaitWeightGrams { get; set; }
        public int NumberOfTransmissions { get; set; }
        public DateTime? MeasurementTime { get; set; }
       // public bool SaveToDatabase { get; set; } = true;  // defaults to save
    }
}
