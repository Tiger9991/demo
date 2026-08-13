using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class AverageSeverityDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public int CaptureCount { get; set; }
        public double BaitConsumption { get; set; }
        public double RepeatRate { get; set; }
        public double TimeSpent { get; set; }
        public double NormalizedCaptures { get; set; }  // 0–100
        public double NormalizedBait { get; set; }      // 0–100
        public double NormalizedRepeat { get; set; }    // 0–100
        public double NormalizedTime { get; set; }      // 0–100
        public double SeverityScore { get; set; }       // Average of the four normalized values
        public string SeverityLevel { get; set; } = string.Empty;
    }
}
