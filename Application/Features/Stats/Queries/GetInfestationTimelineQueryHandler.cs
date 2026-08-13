using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Stats.Queries
{
    public class GetInfestationTimelineQueryHandler
        : IRequestHandler<GetInfestationTimelineQuery, InfestationTimelineDto>
    {
        private readonly IApplicationDbContext _context;

        public GetInfestationTimelineQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InfestationTimelineDto> Handle(
            GetInfestationTimelineQuery request,
            CancellationToken cancellationToken)
        {
            var egyptZone = Application.Common.Helpers.DateTimeHelper.EgyptZone;
            
            var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);

            if (request.Timeframe == "daily")
            {
                // Today's date only (12:00 AM to 11:59 PM Egypt Time), grouped by hour (24 hours)
                var fromDateEgypt = nowEgypt.Date;
                var toDateEgypt = fromDateEgypt.AddDays(1).AddTicks(-1);

                var fromDate = TimeZoneInfo.ConvertTimeToUtc(fromDateEgypt, egyptZone);
                var toDate = TimeZoneInfo.ConvertTimeToUtc(toDateEgypt, egyptZone);

                // Capture Events (Visits)
                var captureQuery = _context.CaptureEvents.AsQueryable();
                if (!string.IsNullOrEmpty(request.GroupNumber))
                {
                    captureQuery = captureQuery.Where(c => c.Trap.TrapGroup == request.GroupNumber);
                }
                var captures = await captureQuery
                    .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= toDate)
                    .Select(c => new { c.CaptureTime })
                    .ToListAsync(cancellationToken);

                // Bait Measurements
                var baitQuery = _context.BaitMeasurements.AsQueryable();
                if (!string.IsNullOrEmpty(request.GroupNumber))
                {
                    baitQuery = baitQuery.Where(b => b.Trap.TrapGroup == request.GroupNumber);
                }
                var baits = await baitQuery
                    .Where(b => b.MeasurementTime >= fromDate && b.MeasurementTime <= toDate)
                    .Select(b => new { b.MeasurementTime, b.BaitWeightGrams })
                    .ToListAsync(cancellationToken);

                var categories = new string[24];
                var visitData = new int[24];
                var baitData = new double[24];

                for (int h = 0; h < 24; h++)
                {
                    categories[h] = $"{h:00}:00";
                    visitData[h] = captures.Count(c => TimeZoneInfo.ConvertTimeFromUtc(c.CaptureTime, egyptZone).Hour == h);
                    baitData[h] = Math.Round(baits.Where(b => TimeZoneInfo.ConvertTimeFromUtc(b.MeasurementTime, egyptZone).Hour == h).Sum(b => b.BaitWeightGrams), 2);
                }

                return new InfestationTimelineDto
                {
                    Categories = categories,
                    BaitData = baitData,
                    VisitData = visitData
                };
            }
            else
            {
                // Current calendar month (from day 1 to last day of the current month in Egypt time)
                var fromDateEgypt = new DateTime(nowEgypt.Year, nowEgypt.Month, 1);
                var daysInMonth = DateTime.DaysInMonth(nowEgypt.Year, nowEgypt.Month);
                var toDateEgypt = new DateTime(nowEgypt.Year, nowEgypt.Month, daysInMonth, 23, 59, 59);

                var fromDate = TimeZoneInfo.ConvertTimeToUtc(fromDateEgypt, egyptZone);
                var toDate = TimeZoneInfo.ConvertTimeToUtc(toDateEgypt, egyptZone);

                // Capture Events (Visits)
                var captureQuery = _context.CaptureEvents.AsQueryable();
                if (!string.IsNullOrEmpty(request.GroupNumber))
                {
                    captureQuery = captureQuery.Where(c => c.Trap.TrapGroup == request.GroupNumber);
                }
                var captures = await captureQuery
                    .Where(c => c.CaptureTime >= fromDate && c.CaptureTime <= toDate)
                    .Select(c => new { c.CaptureTime })
                    .ToListAsync(cancellationToken);

                // Bait Measurements
                var baitQuery = _context.BaitMeasurements.AsQueryable();
                if (!string.IsNullOrEmpty(request.GroupNumber))
                {
                    baitQuery = baitQuery.Where(b => b.Trap.TrapGroup == request.GroupNumber);
                }
                var baits = await baitQuery
                    .Where(b => b.MeasurementTime >= fromDate && b.MeasurementTime <= toDate)
                    .Select(b => new { b.MeasurementTime, b.BaitWeightGrams })
                    .ToListAsync(cancellationToken);

                var categoriesList = new List<string>();
                var visitDataList = new List<int>();
                var baitDataList = new List<double>();

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var day = new DateTime(nowEgypt.Year, nowEgypt.Month, d);
                    categoriesList.Add(d.ToString("00"));
                    visitDataList.Add(captures.Count(c => TimeZoneInfo.ConvertTimeFromUtc(c.CaptureTime, egyptZone).Date == day));
                    baitDataList.Add(Math.Round(baits.Where(b => TimeZoneInfo.ConvertTimeFromUtc(b.MeasurementTime, egyptZone).Date == day).Sum(b => b.BaitWeightGrams), 2));
                }

                string monthName = nowEgypt.ToString("MMMM", Application.Common.Helpers.DateTimeHelper.ArabicCulture);

                return new InfestationTimelineDto
                {
                    Categories = categoriesList.ToArray(),
                    BaitData = baitDataList.ToArray(),
                    VisitData = visitDataList.ToArray(),
                    MonthName = monthName
                };
            }
        }
    }
}
