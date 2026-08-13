using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class LatestAlertDetailDto
    {
        public DateTime CaptureTime { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public string RodentType { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double Length { get; set; }
        public float SignalStrength { get; set; }
        public string SignalQuality { get; set; } = string.Empty;
        public int NumberOfTransmissions { get; set; }
    }
}
