using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class RodentActivityDto
    {
        public int TotalCaptures { get; set; }
        public int TrapsWithCaptures { get; set; }
        public Dictionary<string, int> CapturesByType { get; set; } = new();
        public Dictionary<DateTime, int> CapturesByDate { get; set; } = new(); // 👈 DateTime keys
        public string? GroupNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
