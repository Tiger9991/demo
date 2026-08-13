using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class VisitPatternDetailDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public int TotalVisits { get; set; }
        public DateTime? FirstVisit { get; set; }
        public DateTime? LastVisit { get; set; }
        public double AverageVisitsPerDay { get; set; } // calculated as TotalVisits / days between first and last
    }
}
