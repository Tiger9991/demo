using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class HeatMapDataPointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Intensity { get; set; }   // Number of captures at this location
        public string? Label { get; set; }   // e.g., group number or trap number
    }
}
