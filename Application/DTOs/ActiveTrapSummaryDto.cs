using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class ActiveTrapSummaryDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastCaptureTime { get; set; }
        public int TotalCaptures { get; set; }
        public double TotalBaitConsumed { get; set; }
    }
}
