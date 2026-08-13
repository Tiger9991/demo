using System;

namespace Application.DTOs
{
    public class InfestationTimelineDto
    {
        public string[] Categories { get; set; } = Array.Empty<string>();
        public double[] BaitData { get; set; } = Array.Empty<double>();
        public int[] VisitData { get; set; } = Array.Empty<int>();
        public string? MonthName { get; set; }
    }
}
