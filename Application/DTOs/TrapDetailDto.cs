using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapDetailDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastEntryDate { get; set; }
        public int DaysSinceLastEntry { get; set; }
        public IndicatorStatus IndicatorStatus { get; set; }
        public int BatteryPercentage { get; set; }
        public float SignalStrength { get; set; }
        public string SignalQuality { get; set; } = string.Empty;
        public int TotalTransmissions { get; set; }
        public int OperatingDays { get; set; }

        public bool IsOffline { get; set; }
        public bool IsConnected { get; set; }
        public bool IsActive { get; set; }

        public string Color => !IsActive ? "#808080" : (!IsConnected ? "#808080" : IndicatorStatus switch
        {
            IndicatorStatus.Red => "#dc3545",
            IndicatorStatus.Orange => "#fd7e14",
            IndicatorStatus.Yellow => "#ffc107",
            _ => "#28a745"
        });

        public string StatusArabic => !IsActive ? "غير منشطة" : (!IsConnected ? "غير متصلة" : IndicatorStatus switch
        {
            IndicatorStatus.Red => "نشاط كثيف",
            IndicatorStatus.Orange => "نشاط متوسط",
            IndicatorStatus.Yellow => "نشاط خفيف",
            _ => "بدون نشاط"
        });
    }
}
