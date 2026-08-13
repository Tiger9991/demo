using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class RodentActivityDetailDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public DateTime CaptureTime { get; set; }
        public string RodentType { get; set; } = string.Empty;
        public double RodentWeightGrams { get; set; }
        public double RodentLengthCm { get; set; }
        public float SignalStrength { get; set; }
        public int NumberOfTransmissions { get; set; }
        public int ActiveSensorCount { get; set; }
        public double? BaitWeightGrams { get; set; } // nullable, might not always have bait measurement
    }
}
