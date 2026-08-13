using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class LowBatteryCountDto
    {
        public int Count { get; set; }
        public int Threshold { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
