using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class BatteryStatusDto
    {
        public Guid TrapId { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public int CurrentBatteryPercentage { get; set; }
        public int CalculatedBatteryPercentage { get; set; }
        public int TotalTransmissions { get; set; }
        public int OperatingDays { get; set; }
    }
}
