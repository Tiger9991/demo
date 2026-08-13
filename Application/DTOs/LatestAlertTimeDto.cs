using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class LatestAlertTimeDto
    {
        public DateTime? LatestCaptureTime { get; set; }
        public string? TrapNumber { get; set; }
        public string? GroupNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
