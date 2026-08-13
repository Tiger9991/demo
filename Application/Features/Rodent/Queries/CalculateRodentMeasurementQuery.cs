using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rodent.Queries
{
    public class CalculateRodentMeasurementQuery : IRequest<RodentMeasurementDto>
    {
        public string TrapNumber { get; set; } = string.Empty;
        public List<int> ActiveSensorIndices { get; set; } = new(); // e.g., [1,3,5]
        public int WeightGrams { get; set; }
    }
}
