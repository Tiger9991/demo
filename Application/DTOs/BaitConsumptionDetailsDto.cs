using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class BaitConsumptionDetailsDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public double TotalConsumed { get; set; }
        public int MeasurementCount { get; set; }
        public double AveragePerMeasurement { get; set; }
    }
}
