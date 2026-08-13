using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class PeakHourSummaryDto
    {
        public int PeakHour { get; set; }          // 0-23
        public int PeakHourCount { get; set; }
        public int TotalCaptures { get; set; }
        public double PercentageOfTotal { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
