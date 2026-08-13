using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Queries
{
    public class CalculateAverageBaitConsumptionQuery : IRequest<BaitConsumptionDto>
    {
        public string TrapNumber { get; set; } = string.Empty;
        public double BaitWeightGrams { get; set; }      // consumed bait weight
        public int NumberOfTransmissions { get; set; }   // number of rodent visits
        public DateTime? MeasurementTime { get; set; }   // optional, defaults to UTC now
       // public bool SaveToDatabase { get; set; } = false;
    }
}
