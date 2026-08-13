using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.TapBaitMeasurement.Commands
{
    public class CreateTrapBaitMeasurementCommand : IRequest<TrapBaitMeasurementDto>
    {
        public string TrapNumber {  get; set; }
        public string TrapGroup { get; set; }
    //    public DateTime MeasurementTime { get; set; }
        //  public double BaitWeightGrams { get; set; }
        public double BWeight { get; set; }
        public float SignalStrength { get; set; }
    }
}
