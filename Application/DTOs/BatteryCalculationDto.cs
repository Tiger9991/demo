using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class BatteryCalculationDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public int UsedTransmissions { get; set; }
        public int OperatingDays { get; set; }
        public int CalculatedBatteryPercentage { get; set; }
        public int CurrentStoredBatteryPercentage { get; set; }  // from database
        public string Message { get; set; } = string.Empty;
    }
}
