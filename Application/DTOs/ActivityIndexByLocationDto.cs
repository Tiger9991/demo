using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class ActivityIndexByLocationDto
    {
        public string GroupNumber { get; set; } = string.Empty;
        public double Index { get; set; }           // 0–100
        public string Color { get; set; } = string.Empty;
    }
}
