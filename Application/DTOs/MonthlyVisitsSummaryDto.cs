using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class MonthlyVisitsSummaryDto
    {
        public int TotalVisits { get; set; }
        public DateTime MonthStart { get; set; }
        public DateTime MonthEnd { get; set; }
        public int AveragePerDay { get; set; }
        public int MaxDayVisits { get; set; }
        public DateTime MaxDay { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
