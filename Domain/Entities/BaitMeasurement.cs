using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;


namespace Domain.Entities
{
    public class BaitMeasurement : AuditableEntity<Guid>
    {
        public Guid TrapId { get; set; }
        public Trap Trap { get; set; } = null!;

        public Guid? CaptureEventId { get; set; }
        public CaptureEvent? CaptureEvent { get; set; }

        public DateTime MeasurementTime { get; set; }
        public double BaitWeightGrams { get; set; }

        public BaitMeasurement()
        {
            Id = Guid.NewGuid();
        }
    }
}
