using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class AverageDailyVisitsSummaryDto
    {
        public double Average { get; set; }
        public int TotalVisits { get; set; }
        public int TotalDays { get; set; }
        public int MaxDayVisits { get; set; }
        public DateTime MaxDay { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
