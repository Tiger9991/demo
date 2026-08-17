using Application.Common.Interfaces;
using Application.DTOs;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Application.Features.Traps.Queries
{


    public sealed class GetActiveTrapsByGroupQueryHandler
    : IRequestHandler<GetActiveTrapsByGroupQuery, List<TrapDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly TrapSettings _trapSettings;
        private readonly ILogger<GetActiveTrapsByGroupQueryHandler> _logger;

        public GetActiveTrapsByGroupQueryHandler(
            IApplicationDbContext context,
            IOptions<TrapSettings> trapSettings,
            ILogger<GetActiveTrapsByGroupQueryHandler> logger)
        {
            _context = context;
            _trapSettings = trapSettings.Value;
            _logger = logger;
        }

        public async Task<List<TrapDto>> Handle(GetActiveTrapsByGroupQuery request, CancellationToken cancellationToken)
        {
            // 1. Load all TrapGroups (optionally filtered by CustomerId)
            var groupQuery = _context.TrapGroups.AsNoTracking();
            if (request.CustomerId.HasValue)
                groupQuery = groupQuery.Where(g => g.CustomerId == request.CustomerId.Value);
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                groupQuery = groupQuery.Where(g => g.TrapGroup == request.GroupNumber);

            var allGroups = await groupQuery
                .OrderBy(g => g.TrapGroup)
                .ThenBy(g => g.TrapNumber)
                .ToListAsync(cancellationToken);

            // 2. Load all physical traps (filter by group if provided)
            var trapQuery = _context.Traps.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);

            var allTraps = await trapQuery.ToListAsync(cancellationToken);

            var groupDict = allGroups
                .GroupBy(g => new { TrapGroup = g.TrapGroup ?? "0", TrapNumber = g.TrapNumber ?? "0" })
                .ToDictionary(g => g.Key, g => g.First());

            var trapsDict = allTraps
                .GroupBy(t => new { TrapGroup = t.TrapGroup ?? "0", TrapNumber = t.TrapNumber ?? "0" })
                .ToDictionary(g => g.Key, g => g.First());

            // Merge all unique (TrapGroup, TrapNumber) pairs from both TrapGroups and Traps
            var allKeys = allGroups.Select(g => new { TrapGroup = g.TrapGroup ?? "0", TrapNumber = g.TrapNumber ?? "0" })
                .Union(allTraps.Select(t => new { TrapGroup = t.TrapGroup ?? "0", TrapNumber = t.TrapNumber ?? "0" }))
                .Distinct()
                .OrderBy(k => k.TrapGroup)
                .ThenBy(k => k.TrapNumber)
                .ToList();

            if (!allKeys.Any())
                return new List<TrapDto>();

            var trapIds = allTraps.Select(t => t.Id).ToList();

            // 3. Load all bait measurements for these traps (to compute intervals)
            var trapBaitQuery = _context.TrapBaitMeasurement.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                trapBaitQuery = trapBaitQuery.Where(m => trapIds.Contains(m.TrapId));

            var trapBaitList = await trapBaitQuery
                .Select(m => new { m.TrapId, m.MeasurementTime })
                .ToListAsync(cancellationToken);

            var baitQuery = _context.BaitMeasurements.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                baitQuery = baitQuery.Where(m => trapIds.Contains(m.TrapId));

            var baitList = await baitQuery
                .Select(m => new { m.TrapId, m.MeasurementTime })
                .ToListAsync(cancellationToken);

            var combinedBait = trapBaitList.Concat(baitList)
                .OrderBy(m => m.TrapId)
                .ThenBy(m => m.MeasurementTime)
                .ToList();

            var baitGrouped = combinedBait
                .GroupBy(m => m.TrapId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Load latest capture event per trap
            var captureQuery = _context.CaptureEvents.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                captureQuery = captureQuery.Where(c => trapIds.Contains(c.TrapId));

            var latestCapture = await captureQuery
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LastCapture = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LastCapture, cancellationToken);

            var result = new List<TrapDto>();

            foreach (var key in allKeys)
            {
                var group = groupDict.GetValueOrDefault(key);
                var trap = trapsDict.GetValueOrDefault(key);

                bool isConnected = false;
                string? disconnectReason = null;

                if (trap == null)
                {
                    disconnectReason = "المحطة غير مسجلة في قاعدة البيانات (لم ترسل بعد)";
                }
                else if (trap.status != "Active")
                {
                    disconnectReason = $"حالة المحطة: {trap.status} (ليست نشطة)";
                }
                else
                {
                    // Get last activity (bait or capture)
                    DateTime? lastBait = baitGrouped.TryGetValue(trap.Id, out var measurements)
                        ? measurements.LastOrDefault()?.MeasurementTime
                        : null;
                    DateTime? lastCapture = latestCapture.TryGetValue(trap.Id, out var capTime) ? capTime : null;

                    var candidateDates = new[] { lastBait, lastCapture, trap.LastEntryDate }.Where(d => d.HasValue).Select(d => d.Value).ToList();
                    DateTime? lastActivity = candidateDates.Any() ? candidateDates.Max() : null;

                    // If no activity at all, check if the trap is within the grace period
                    if (!lastActivity.HasValue)
                    {
                        var hoursSinceStart = (DateTime.UtcNow - trap.StartTime).TotalHours;
                        if (hoursSinceStart <= _trapSettings.NewTrapGracePeriodHours)
                        {
                            isConnected = true;
                            disconnectReason = "محطة جديدة، في انتظار أول قياس";
                        }
                        else
                        {
                            disconnectReason = "لا يوجد نشاط مسجل (لا قياسات طعم ولا دخول قارض)";
                        }
                    }
                    else
                    {
                        // Calculate adaptive threshold based on this trap's measurement history
                        double thresholdHours = _trapSettings.ConnectivityThresholdHours;

                        if (baitGrouped.TryGetValue(trap.Id, out var measurementsList) && measurementsList.Count > 1)
                        {
                            var intervals = new List<double>();
                            for (int i = 1; i < measurementsList.Count; i++)
                            {
                                var diff = (measurementsList[i].MeasurementTime - measurementsList[i - 1].MeasurementTime).TotalHours;
                                if (diff > 0)
                                    intervals.Add(diff);
                            }

                            if (intervals.Any())
                            {
                                var avgInterval = intervals.Average();
                                thresholdHours = Math.Clamp(
                                    avgInterval * _trapSettings.AdaptiveMultiplier,
                                    _trapSettings.MinimumThresholdHours,
                                    _trapSettings.MaximumThresholdHours
                                );
                            }
                        }

                        var hoursSinceLast = (DateTime.UtcNow - lastActivity.Value).TotalHours;

                        if (hoursSinceLast <= thresholdHours)
                        {
                            isConnected = true;
                        }
                        else
                        {
                            string source = lastBait.HasValue && lastCapture.HasValue
                                ? lastBait > lastCapture ? "قياس طعم" : "دخول قارض"
                                : lastBait.HasValue ? "قياس طعم" : "دخول قارض";

                            disconnectReason =
                                $"آخر نشاط ({source}) منذ {hoursSinceLast:F1} ساعة (ارسال كل: {thresholdHours:F1} ساعة)";
                        }
                    }
                }

                // Build DTO
                if (trap != null)
                {
                    result.Add(new TrapDto
                    {
                        Id = trap.Id,
                        TrapNumber = trap.TrapNumber,
                        TrapGroup = trap.TrapGroup,
                        IsActive = isConnected,
                        StartTime = trap.StartTime,
                        BatteryPercentage = Trap.CalculateBatteryPercentage(trap.status, trap.BatteryPercentage, trap.StartTime, trap.TotalTransmissions),
                        IndicatorStatus = Trap.CalculateIndicatorStatus(trap.LastEntryDate),
                        LastEntryDate = trap.LastEntryDate,
                        TotalTransmissions = trap.TotalTransmissions,
                        OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - trap.StartTime).TotalDays),
                        SignalStrength = trap.SignalStrength,
                        SignalQuality = trap.SignalQuality,
                        DisconnectReason = disconnectReason
                    });
                }
                else
                {
                    result.Add(new TrapDto
                    {
                        Id = group?.Id ?? Guid.NewGuid(),
                        TrapNumber = group?.TrapNumber ?? key.TrapNumber,
                        TrapGroup = group?.TrapGroup ?? key.TrapGroup,
                        IsActive = false,
                        StartTime = group?.CreatedAt ?? DateTime.UtcNow,
                        BatteryPercentage = 0,
                        IndicatorStatus = IndicatorStatus.Green,
                        LastEntryDate = null,
                        TotalTransmissions = 0,
                        OperatingDays = group != null ? Math.Max(0, (int)(DateTime.UtcNow - group.CreatedAt).TotalDays) : 0,
                        SignalStrength = 0,
                        SignalQuality = "-",
                        DisconnectReason = disconnectReason
                    });
                }
            }

            // 6. Apply Status filter if requested (Active / Inactive)
            if (!string.IsNullOrEmpty(request.Status))
            {
                bool activeFilter = request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                result = result.Where(d => d.IsActive == activeFilter).ToList();
            }

            // 7. Apply Take limit
            if (request.Take.HasValue && request.Take.Value > 0)
                result = result.Take(request.Take.Value).ToList();

            return result;
        }
    }
}

