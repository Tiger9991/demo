using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class HourlyActivityDto
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public string HourLabel => $"{Hour:00}:00 - {Hour + 1:00}:00";
    }
}
