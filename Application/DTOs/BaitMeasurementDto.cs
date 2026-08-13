using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class BaitMeasurementDto
    {
        public Guid Id { get; set; }
        public Guid? CaptureEventId { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public DateTime MeasurementTime { get; set; }
        public double BaitWeightGrams { get; set; }
    }
}
