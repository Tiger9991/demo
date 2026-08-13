using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class BaitConsumptionDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public double BaitConsumedGrams { get; set; }
        public int NumberOfTransmissions { get; set; }
        public double AverageConsumptionPerRodent { get; set; }
        public DateTime? MeasurementTime { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    }

