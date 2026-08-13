using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class ActiveTrapsTodayDto
    {
        public int TotalActiveTrapsCount { get; set; }
        public List<ActiveTrapTodayDetailDto> ActiveTrapsDetails { get; set; } = new();
    }

    public class ActiveTrapTodayDetailDto
    {
        public Guid TrapId { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string TrapGroup { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastCaptureTime { get; set; }
        public int TotalCapturesToday { get; set; }
        public int BatteryPercentage { get; set; }
        public float SignalStrength { get; set; }
        public string SignalQuality { get; set; } = string.Empty;
    }
}
