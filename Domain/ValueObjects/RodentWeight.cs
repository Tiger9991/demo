using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    public record RodentWeight
    {
        public double Grams { get; init; }

        private RodentWeight() { } // for EF Core

        public RodentWeight(double grams) => Grams = Math.Clamp(grams, 0, 1000);

        public static RodentWeight FromSensorValue(double value) => new RodentWeight(value);
    }
}
