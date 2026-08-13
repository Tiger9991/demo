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

            allGroups = allGroups
                .DistinctBy(g => new { g.TrapGroup, g.TrapNumber })
                .ToList();

            if (!allGroups.Any())
                return new List<TrapDto>();

            // 2. Load all physical traps (filter by group if provided)
            var trapQuery = _context.Traps.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);

            var allTraps = await trapQuery.ToListAsync(cancellationToken);
            var trapsDict = allTraps
                .GroupBy(t => new { t.TrapGroup, t.TrapNumber })
                .ToDictionary(g => g.Key, g => g.First());

            var trapIds = allTraps.Select(t => t.Id).ToList();

            // 3. Load all bait measurements for these traps (to compute intervals)
            var baitMeasurements = await _context.TrapBaitMeasurement
                .Where(m => trapIds.Contains(m.TrapId))
                .OrderBy(m => m.TrapId)
                .ThenBy(m => m.MeasurementTime)
                .ToListAsync(cancellationToken);

            var baitGrouped = baitMeasurements
                .GroupBy(m => m.TrapId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Load latest capture event per trap
            var latestCapture = await _context.CaptureEvents
                .Where(c => trapIds.Contains(c.TrapId))
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LastCapture = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LastCapture, cancellationToken);

            var result = new List<TrapDto>();

            foreach (var group in allGroups)
            {
                var key = new { group.TrapGroup, group.TrapNumber };
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

                    DateTime? lastActivity = new[] { lastBait, lastCapture }.Max();

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
                        BatteryPercentage = trap.BatteryPercentage,
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
                        Id = group.Id,
                        TrapNumber = group.TrapNumber,
                        TrapGroup = group.TrapGroup,
                        IsActive = false,
                        StartTime = group.CreatedAt,
                        BatteryPercentage = 0,
                        IndicatorStatus = IndicatorStatus.Green,
                        LastEntryDate = null,
                        TotalTransmissions = 0,
                        OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - group.CreatedAt).TotalDays),
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

