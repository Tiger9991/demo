using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class ActivityByHourSummaryDto
    {
        public int TotalCaptures { get; set; }
        public int PeakHour { get; set; } // 0-23
        public int PeakHourCount { get; set; }
        public double AveragePerHour { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
