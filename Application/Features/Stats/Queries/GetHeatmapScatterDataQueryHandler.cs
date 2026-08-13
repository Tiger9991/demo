using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Stats.Queries
{
    // معالج استعلام بيانات الخريطة المشتتة (Heatmap Scatter Data Query Handler)
    public class GetHeatmapScatterDataQueryHandler : IRequestHandler<GetHeatmapScatterDataQuery, List<ScatterChartSeriesDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetHeatmapScatterDataQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScatterChartSeriesDto>> Handle(GetHeatmapScatterDataQuery request, CancellationToken cancellationToken)
        {
            // استدعاء الدالة الوهمية (Mock) التي ترجع البيانات حالياً.
            // عندما تقوم بإنشاء الجداول والبيانات في قاعدة البيانات لاحقاً،
            // قم باستبدال الاستدعاء أدناه بقراءة حقيقية من الـ DbContext.
            // مثال:
            // return await _context.ScatterDataPoints
            //     .GroupBy(x => x.SeriesName)
            //     .Select(g => new ScatterChartSeriesDto {
            //         Name = g.Key,
            //         Data = g.Select(p => new double[] { p.X, p.Y }).ToList()
            //     }).ToListAsync(cancellationToken);
            
            return GetMockScatterData();
        }

        /// <summary>
        /// دالة static ترجع بيانات وهمية للرسم البياني المشتت (Scatter Chart) لغايات العرض لعدم وجود جدول وقاعدة بيانات مخصصة لها حالياً.
        /// يمكنك تعديل النقاط والإحداثيات هنا لتحديث الرسم البياني المعروض.
        /// </summary>
        public static List<ScatterChartSeriesDto> GetMockScatterData()
        {
            return new List<ScatterChartSeriesDto>
            {
                new ScatterChartSeriesDto
                {
                    Name = "Messenger",
                    Data = new List<double[]>
                    {
                        new double[] { 16.4, 5.4 },
                        new double[] { 21.7, 4.0 },
                        new double[] { 25.4, 3.0 },
                        new double[] { 19.0, 2.0 },
                        new double[] { 10.9, 1.0 },
                        new double[] { 13.6, 3.2 },
                        new double[] { 10.9, 7.0 },
                        new double[] { 10.9, 8.2 },
                        new double[] { 16.4, 4.0 },
                        new double[] { 13.6, 4.3 },
                        new double[] { 13.6, 12.0 },
                        new double[] { 29.9, 3.0 },
                        new double[] { 10.9, 5.2 },
                        new double[] { 16.4, 6.5 },
                        new double[] { 10.9, 8.0 },
                        new double[] { 24.5, 7.1 },
                        new double[] { 10.9, 7.0 },
                        new double[] { 8.1, 4.7 },
                        new double[] { 19.0, 10.0 },
                        new double[] { 27.1, 10.0 },
                        new double[] { 24.5, 8.0 },
                        new double[] { 27.1, 3.0 },
                        new double[] { 29.9, 11.5 },
                        new double[] { 27.1, 0.8 },
                        new double[] { 22.1, 2.0 }
                    }
                },
                new ScatterChartSeriesDto
                {
                    Name = "Instagram",
                    Data = new List<double[]>
                    {
                        new double[] { 6.4, 5.4 },
                        new double[] { 11.7, 4.0 },
                        new double[] { 15.4, 3.0 },
                        new double[] { 9.0, 2.0 },
                        new double[] { 10.9, 11.0 },
                        new double[] { 20.9, 7.0 },
                        new double[] { 12.9, 8.2 },
                        new double[] { 6.4, 14.0 },
                        new double[] { 11.6, 12.0 }
                    }
                }
            };
        }
    }
}
