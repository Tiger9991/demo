using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapsNeedingRefillDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double LatestBaitWeight { get; set; }
        public DateTime LastMeasurementTime { get; set; }
        public int DaysSinceLastMeasurement { get; set; }
        public DateTime? PreviousRefillDate { get; set; }
        public double? WeightAfterRefill { get; set; }
        public DateTime? PreviousMeasurementTime { get; set; }
        public double? PreviousBaitWeight { get; set; }
    }
}
