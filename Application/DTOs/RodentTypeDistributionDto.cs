using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class RodentTypeDistributionDto
    {
        public string RodentType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
