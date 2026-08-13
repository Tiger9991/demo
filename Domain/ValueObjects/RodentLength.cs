using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    public record RodentLength
    {
        public double Centimeters { get; init; }

        public static RodentLength FromSensors(IEnumerable<int> activeSensors)
        {
            // Mapping from the table: IR1 <5cm, IR2 5-11, IR3 11-16, IR4 16-22, IR5 22-27, IR6 27-32
            var maxSensor = activeSensors.DefaultIfEmpty(0).Max();
            return maxSensor switch
            {
                1 => new RodentLength { Centimeters = 4 },        // <5
                2 => new RodentLength { Centimeters = 8 },        // avg
                3 => new RodentLength { Centimeters = 13 },
                4 => new RodentLength { Centimeters = 19 },
                5 => new RodentLength { Centimeters = 24 },
                6 => new RodentLength { Centimeters = 29 },
                _ => new RodentLength { Centimeters = 0 }
            };
        }
    }
}
