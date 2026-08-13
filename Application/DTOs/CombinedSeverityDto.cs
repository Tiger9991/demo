using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class CombinedSeverityDto
    {
        public double AverageSeverityScore { get; set; }
        public int TotalTraps { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
