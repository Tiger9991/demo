using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapBaitMeasurementDto
    {
        public Guid Id { get; set; }

        public Guid TrapId { get; set; }

      //  public DateTime MeasurementTime { get; set; }
        public double BWeight { get; set; }
        public float SignalStrength { get; set; }
    }
}
