using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace Domain.Entities
{

    public class Trap : AuditableEntity<Guid>
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string TrapGroup { get; set; } = string.Empty;
        public float SignalStrength { get; set; }
        public string SignalQuality => CalculateSignalQuality((double)SignalStrength);

        public static string CalculateSignalQuality(double rssi)
        {
            //if (rssi >= -60) return "Excellent (very close)";
            //if (rssi >= -80) return "Very good";
            //if (rssi >= -95) return "Good";
            //if (rssi >= -105) return "Fair";
            //if (rssi >= -115) return "Weak";
            //return "Very weak, near sensitivity limit";
            if (rssi >= -60) return "ممتاز";
            if (rssi >= -80) return "جيد جدا";
            if (rssi >= -95) return "جيد";
            if (rssi >= -105) return "ضغيف";
            if (rssi >= -115) return "ضعيف جدا";
            return "مستوى متدنى";
        }

        public string status { get; set; } = "Active";
        public DateTime StartTime { get; set; }
        public int BatteryPercentage { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public IndicatorStatus IndicatorStatus { get; set; } 
        public DateTime? LastEntryDate { get; set; }
        public int TotalTransmissions { get; set; }
        public int OperatingDays { get; set; }

        // Geolocation
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

       


        public static int CalculateBatteryPercentage(string status, int currentBattery, DateTime startTime, int transmissions, bool forceCalculate = false)
        {
            // Do not calculate if trap is inactive or already dead (unless forced)
            if (!forceCalculate && (status != "Active" || currentBattery == 0))
                return currentBattery;

            // 1. Transmission deduction: 1% per 20 transmissions (computed continuously: 0.05% per transmission)
            double transmissionDeduction = transmissions * 0.05;

            // 2. Time deduction: 1.85% per day of operation (computed continuously)
            double totalDays = Math.Max(0, (DateTime.UtcNow - startTime).TotalDays);
            double timeDeduction = totalDays * 1.85;

            // 3. Calculate remaining battery
            double exactBattery = 100.0 - (transmissionDeduction + timeDeduction);

            // 4. Round to nearest integer and clamp between 0 and 100
            int newBattery = (int)Math.Round(exactBattery, MidpointRounding.AwayFromZero);
            return Math.Clamp(newBattery, 0, 100);
        }

        public void UpdateBattery(bool forceCalculate = false)
        {
            BatteryPercentage = CalculateBatteryPercentage(status, BatteryPercentage, StartTime, TotalTransmissions, forceCalculate);
        }

        public static IndicatorStatus CalculateIndicatorStatus(DateTime? lastEntryDate)
        {
            if (!lastEntryDate.HasValue)
                return IndicatorStatus.Green;

            var daysSinceLast = (DateTime.UtcNow - lastEntryDate.Value).Days;
            if (daysSinceLast < 3) return IndicatorStatus.Red;
            if (daysSinceLast < 6) return IndicatorStatus.Orange;
            if (daysSinceLast < 7) return IndicatorStatus.Yellow;
            return IndicatorStatus.Green;
        }

        // Update indicator based on last entry
        public void UpdateIndicatorStatus()
        {
            IndicatorStatus = CalculateIndicatorStatus(LastEntryDate);
        }
        public Trap()
        {
            Id = Guid.NewGuid();
            // Default to Cairo, Egypt with small variation
            var random = new Random();
            double offsetLat = (random.NextDouble() * 0.08) - 0.04;
            double offsetLng = (random.NextDouble() * 0.08) - 0.04;
            Latitude = Math.Round(30.0444 + offsetLat, 6);
            Longitude = Math.Round(31.2357 + offsetLng, 6);
        }
    }
}
