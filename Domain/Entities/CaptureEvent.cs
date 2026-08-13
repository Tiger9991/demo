using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Domain.Entities
{
    public class CaptureEvent : AuditableEntity<Guid>
    {

        public Guid TrapId { get; set; }
        public Trap Trap { get; set; } = null!;
        //public string GroupNumber { get; set; } = string.Empty;
        //public string TrapNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CaptureTime { get; set; }
        public int ActiveSensorCount { get; set; }
        public RodentWeight RodentWeight { get; set; } = null!;
        public RodentLength RodentLength { get; set; } = new();
        public RodentType RodentType { get; set; }
        public int? Duration { get; set; }



        public double SignalStrength { get; set; }
        public string SignalQuality => Trap.CalculateSignalQuality(SignalStrength);
        public int NumberOfTransmissions { get; set; }
        public CaptureEvent()
        {
            Id = Guid.NewGuid();
        }

        public void SetLengthFromValue(double lengthCm)
        {
            RodentLength = new RodentLength { Centimeters = lengthCm };
        }

        // Keep the existing method that uses sensors
        public void SetLengthFromSensors(int triggeredSensors)
        {
            ActiveSensorCount = triggeredSensors;
            var maxSensor = triggeredSensors;
            double lengthFromSensor = maxSensor switch
            {
                1 => 4,
                2 => 8,
                3 => 13,
                4 => 19,
                5 => 24,
                6 => 29,
                _ => 0
            };
            RodentLength = new RodentLength { Centimeters = lengthFromSensor };
        }
        
        public void DetermineRodentType()
        {
            if (RodentLength.Centimeters >= 7 && RodentLength.Centimeters <= 10 &&
                RodentWeight.Grams >= 15 && RodentWeight.Grams <= 30)
                RodentType = RodentType.NormalRat;
            else if (RodentLength.Centimeters >= 16 && RodentLength.Centimeters <= 21 &&
                     RodentWeight.Grams >= 150 && RodentWeight.Grams <= 250)
                RodentType = RodentType.ClimbingRat;
            else if (RodentLength.Centimeters >= 18 && RodentLength.Centimeters <= 26 &&
                     RodentWeight.Grams >= 200 && RodentWeight.Grams <= 500)
                RodentType = RodentType.NorwegianRat;
            else
                RodentType = RodentType.Unknown;
        }
    }
}
