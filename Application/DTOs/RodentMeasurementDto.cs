using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class RodentMeasurementDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public int ActiveSensorsCount { get; set; }
        public int InputWeightGrams { get; set; }
        public int CalculatedLengthCm { get; set; }
        public string RodentType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
