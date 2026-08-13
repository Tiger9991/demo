using MediatR;
using System.Collections.Generic;
using Application.DTOs;

namespace Application.Features.Stats.Queries
{
    // استعلام لجلب بيانات الخريطة المشتتة (Heatmap Scatter Data)
    public record GetHeatmapScatterDataQuery : IRequest<List<ScatterChartSeriesDto>>;
}
