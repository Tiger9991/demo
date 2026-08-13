using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class ActivityByHourDetailDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public Dictionary<int, int> HourlyCounts { get; set; } = new(); // Key: hour (0-23), Value: count
    }
}
