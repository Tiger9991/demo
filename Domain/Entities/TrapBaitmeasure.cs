using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class TrapBaitMeasurement : AuditableEntity<Guid>
    {
        public Guid TrapId { get; set; }
        public Trap Trap { get; set; } = null!;

        public DateTime MeasurementTime { get; set; }
        public double BaitWeightGrams { get; set; }
        public float SignalStrength { get; set; }

        public TrapBaitMeasurement()
        {
            Id = Guid.NewGuid();
        }
    }
}
