using System.Collections.Generic;

namespace Application.DTOs
{
    // DTO يمثل سلسلة بيانات الرسم البياني المشتت (Scatter Chart Series)
    // كل سلسلة تحتوي على اسم ونقاط إحداثيات [X, Y]
    public class ScatterChartSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public List<double[]> Data { get; set; } = new List<double[]>();
    }
}
