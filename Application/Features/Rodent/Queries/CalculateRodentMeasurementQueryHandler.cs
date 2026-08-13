using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rodent.Queries
{
    public class CalculateRodentMeasurementQueryHandler : IRequestHandler<CalculateRodentMeasurementQuery, RodentMeasurementDto>
    {
        public Task<RodentMeasurementDto> Handle(CalculateRodentMeasurementQuery request, CancellationToken cancellationToken)
        {
            // 1. Determine length from highest active sensor (IR1..IR6)
            int maxSensor = request.ActiveSensorIndices.Any() ? request.ActiveSensorIndices.Max() : 0;
            int calculatedLengthCm = maxSensor switch
            {
                1 => 4,   // <5 cm
                2 => 8,   // 5-11 cm
                3 => 13,  // 11-16 cm
                4 => 19,  // 16-22 cm
                5 => 24,  // 22-27 cm
                6 => 29,  // 27-32 cm
                _ => 0
            };

            // 2. Clamp weight to 0-500g (per original specification)
            int clampedWeight = Math.Clamp(request.WeightGrams, 0, 500);

            // 3. Determine rodent type based on length and weight (from your table)
            string rodentType = "Unknown";
            string message;

            if (calculatedLengthCm >= 7 && calculatedLengthCm <= 10 && clampedWeight >= 15 && clampedWeight <= 30)
            {
                rodentType = "Normal Rat";
                message = "Normal rat detected (7-10 cm, 15-30 g).";
            }
            else if (calculatedLengthCm >= 16 && calculatedLengthCm <= 21 && clampedWeight >= 150 && clampedWeight <= 250)
            {
                rodentType = "Climbing Rat";
                message = "Climbing rat detected (16-21 cm, 150-250 g).";
            }
            else if (calculatedLengthCm >= 18 && calculatedLengthCm <= 26 && clampedWeight >= 200 && clampedWeight <= 500)
            {
                rodentType = "Norwegian Rat";
                message = "Norwegian rat detected (18-26 cm, 200-500 g).";
            }
            else if (calculatedLengthCm > 0)
            {
                message = $"Length {calculatedLengthCm}cm, weight {clampedWeight}g does not match any known rodent type.";
            }
            else
            {
                message = "No active sensors detected. Unable to determine length.";
            }

            var result = new RodentMeasurementDto
            {
                TrapNumber = request.TrapNumber,
                ActiveSensorsCount = request.ActiveSensorIndices.Count,
                InputWeightGrams = clampedWeight,
                CalculatedLengthCm = calculatedLengthCm,
                RodentType = rodentType,
                Message = message
            };

            return Task.FromResult(result);
        }
    }
}
