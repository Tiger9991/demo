using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class LowBatteryTrapDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public int BatteryPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
    }
}
