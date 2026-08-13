using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    

    public class TrapDto
    {
        public Guid Id { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string TrapGroup { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        public DateTime StartTime { get; set; }
        public int BatteryPercentage { get; set; }
        public IndicatorStatus IndicatorStatus { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public int TotalTransmissions { get; set; }
        public int OperatingDays { get; set; }
      
        public float SignalStrength { get; set; }
        public string SignalQuality { get; set; } = string.Empty;
        public string? DisconnectionReason { get; set; }
        public string? DisconnectReason { get; internal set; }
    }
}
