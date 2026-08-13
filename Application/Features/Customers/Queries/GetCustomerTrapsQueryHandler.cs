using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public sealed class GetCustomerTrapsQueryHandler
        : IRequestHandler<GetCustomerTrapsQuery, List<TrapDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerTrapsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrapDto>> Handle(
            GetCustomerTrapsQuery request,
            CancellationToken cancellationToken)
        {
            // جلب أرقام المجموعات والرموز المرتبطة بهذا العميل
            var customerTrapGroupList = await _context.TrapGroups
                .Where(g => g.CustomerId == request.CustomerId)
                .Select(g => new { g.TrapGroup, g.TrapNumber })
                .ToListAsync(cancellationToken);

            if (!customerTrapGroupList.Any())
                return new List<TrapDto>();

            var groupNumbers = customerTrapGroupList.Select(g => g.TrapGroup).Distinct().ToList();

            // جلب المحطات المنتمية لهذه المجموعات
            var allTrapsInGroups = await _context.Traps
                .Where(t => groupNumbers.Contains(t.TrapGroup))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // تصفية المحطات لتطابق المجموعات والأرقام الخاصة بالعميل
            var traps = allTrapsInGroups
                .Where(t => customerTrapGroupList.Any(g => g.TrapGroup == t.TrapGroup && g.TrapNumber == t.TrapNumber))
                .OrderBy(t => t.TrapGroup)
                .ThenBy(t => t.TrapNumber)
                .ToList();

            var trapIds = traps.Select(t => t.Id).ToList();

            var latestMeasurements = await _context.TrapBaitMeasurement.AsNoTracking()
                .Where(m => trapIds.Contains(m.TrapId))
                .GroupBy(m => m.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(m => m.MeasurementTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var latestCaptures = await _context.CaptureEvents.AsNoTracking()
                .Where(c => trapIds.Contains(c.TrapId))
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            return traps.Select(t =>
            {
                bool isConnected = false;
                DateTime? lastActivity = null;

                if (latestMeasurements.TryGetValue(t.Id, out var latestBaitTime))
                {
                    lastActivity = latestBaitTime;
                }

                if (latestCaptures.TryGetValue(t.Id, out var latestCaptureTime))
                {
                    if (lastActivity == null || latestCaptureTime > lastActivity.Value)
                    {
                        lastActivity = latestCaptureTime;
                    }
                }

                if (lastActivity.HasValue)
                {
                    if ((System.DateTime.UtcNow - lastActivity.Value).TotalHours <= 2)
                    {
                        isConnected = true;
                    }
                }

                return new TrapDto
                {
                    Id = t.Id,
                    TrapNumber = t.TrapNumber,
                    TrapGroup = t.TrapGroup,
                    IsActive = t.status == "Active" && isConnected,
                    StartTime = t.StartTime,
                    BatteryPercentage = Trap.CalculateBatteryPercentage(t.status, t.BatteryPercentage, t.StartTime, t.TotalTransmissions),
                    IndicatorStatus = Trap.CalculateIndicatorStatus(t.LastEntryDate),
                    LastEntryDate = t.LastEntryDate,
                    TotalTransmissions = t.TotalTransmissions,
                    OperatingDays = System.Math.Max(0, (int)(System.DateTime.UtcNow - t.StartTime).TotalDays),
                    SignalStrength = t.SignalStrength,
                    SignalQuality = t.SignalQuality
                };
            }).ToList();
        }
    }
}
