using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class ActivityIndexWithBadgeDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public double Index { get; set; }         // 0–100
        public string Level { get; set; } = string.Empty; // "منخفض", "متوسط", "مرتفع", "حرج"
        public string BadgeColor { get; set; } = string.Empty;
        public string BadgeIcon { get; set; } = string.Empty; // optional
    }
}
