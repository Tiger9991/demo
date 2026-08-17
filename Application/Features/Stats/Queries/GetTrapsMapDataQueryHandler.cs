using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stats.Queries
{
    public class GetTrapsMapDataQueryHandler
    : IRequestHandler<GetTrapsMapDataQuery, List<TrapDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTrapsMapDataQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<TrapDetailDto>> Handle(GetTrapsMapDataQuery request, CancellationToken cancellationToken)
        {
            // 1. Load active traps from database
            var trapQuery = _context.Traps.AsNoTracking();
            if (!string.IsNullOrEmpty(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);

            var databaseTraps = await trapQuery.ToListAsync(cancellationToken);

            // 3. Load TrapGroups from database
            var groupQuery = _context.TrapGroups.AsNoTracking();
            if (!string.IsNullOrEmpty(request.GroupNumber))
                groupQuery = groupQuery.Where(tg => tg.TrapGroup == request.GroupNumber);

            var allGroups = await groupQuery.ToListAsync(cancellationToken);

            // 4. Fetch telemetry activity for connected status check
            var dbTrapIds = databaseTraps.Select(t => t.Id).ToList();

            var latestMeasurements = await _context.TrapBaitMeasurement.AsNoTracking()
                .Where(m => dbTrapIds.Contains(m.TrapId))
                .GroupBy(m => m.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(m => m.MeasurementTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var latestCaptures = await _context.CaptureEvents.AsNoTracking()
                .Where(c => dbTrapIds.Contains(c.TrapId))
                .GroupBy(c => c.TrapId)
                .Select(g => new { TrapId = g.Key, LatestTime = g.Max(c => c.CaptureTime) })
                .ToDictionaryAsync(x => x.TrapId, x => x.LatestTime, cancellationToken);

            var results = new List<TrapDetailDto>();
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 5. Process all database traps
            foreach (var dbTrap in databaseTraps)
            {
                var groupKey = dbTrap.TrapGroup ?? "0";
                var trapKey = dbTrap.TrapNumber ?? "0";
                processedKeys.Add($"{groupKey}_{trapKey}");

                // Coordinates resolution
                double latitude;
                double longitude;

                if (dbTrap.Latitude.HasValue && dbTrap.Longitude.HasValue && dbTrap.Latitude.Value != 0 && dbTrap.Longitude.Value != 0)
                {
                    latitude = dbTrap.Latitude.Value;
                    longitude = dbTrap.Longitude.Value;
                }
                else
                {
                    var (defLat, defLng) = CalculateDefaultCoordinates(groupKey, trapKey);
                    latitude = defLat;
                    longitude = defLng;
                }

                IndicatorStatus status = Trap.CalculateIndicatorStatus(dbTrap.LastEntryDate);
                int days = dbTrap.LastEntryDate.HasValue ? (int)(DateTime.UtcNow - dbTrap.LastEntryDate.Value).TotalDays : 999;

                var candidateDates = new[] {
                    latestMeasurements.TryGetValue(dbTrap.Id, out var lb) ? lb : (DateTime?)null,
                    latestCaptures.TryGetValue(dbTrap.Id, out var lc) ? lc : (DateTime?)null,
                    dbTrap.LastEntryDate
                }.Where(d => d.HasValue).Select(d => d.Value).ToList();

                DateTime? lastActivity = candidateDates.Any() ? candidateDates.Max() : null;

                bool isConnected = false;
                if (dbTrap.status == "Active")
                {
                    if (lastActivity.HasValue)
                    {
                        isConnected = (DateTime.UtcNow - lastActivity.Value).TotalHours <= 2.0;
                    }
                    else
                    {
                        isConnected = (DateTime.UtcNow - dbTrap.StartTime).TotalHours <= 2.0;
                    }
                }

                results.Add(new TrapDetailDto
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    TrapNumber = dbTrap.TrapNumber,
                    GroupNumber = dbTrap.TrapGroup ?? "Unassigned",
                    Status = dbTrap.status,
                    LastEntryDate = dbTrap.LastEntryDate,
                    DaysSinceLastEntry = days,
                    IndicatorStatus = status,
                    BatteryPercentage = dbTrap.BatteryPercentage,
                    SignalStrength = dbTrap.SignalStrength,
                    SignalQuality = dbTrap.SignalQuality,
                    TotalTransmissions = dbTrap.TotalTransmissions,
                    OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - dbTrap.StartTime).TotalDays),
                    IsActive = dbTrap.status == "Active",
                    IsConnected = isConnected,
                    IsOffline = dbTrap.status != "Active" || !isConnected
                });
            }

            // 6. Process any configured TrapGroups not yet in Traps table
            foreach (var tg in allGroups)
            {
                var groupKey = tg.TrapGroup ?? "0";
                var trapKey = tg.TrapNumber ?? "0";
                var uniqueKey = $"{groupKey}_{trapKey}";

                if (processedKeys.Contains(uniqueKey))
                    continue;

                processedKeys.Add(uniqueKey);

                var (defLat, defLng) = CalculateDefaultCoordinates(groupKey, trapKey);

                results.Add(new TrapDetailDto
                {
                    Latitude = defLat,
                    Longitude = defLng,
                    TrapNumber = tg.TrapNumber,
                    GroupNumber = tg.TrapGroup,
                    Status = "Non-Initialized",
                    LastEntryDate = null,
                    DaysSinceLastEntry = 999,
                    IndicatorStatus = IndicatorStatus.Green,
                    BatteryPercentage = 0,
                    SignalStrength = 0,
                    SignalQuality = "None",
                    TotalTransmissions = 0,
                    OperatingDays = 0,
                    IsActive = false,
                    IsConnected = false,
                    IsOffline = true
                });
            }

            // 7. Return database results (empty list if no traps exist)
            var orderedResults = results.OrderBy(d => d.IsOffline).ThenBy(d => d.GroupNumber).ThenBy(d => d.TrapNumber).ToList();
            if (request.Limit.HasValue)
            {
                orderedResults = orderedResults.Take(request.Limit.Value).ToList();
            }
            return orderedResults;
        }

        private static (double Latitude, double Longitude) CalculateDefaultCoordinates(string groupStr, string trapStr)
        {
            int group = int.TryParse(groupStr, out var g) ? g : 0;
            int number = int.TryParse(trapStr, out var n) ? n : 0;

            double groupLat = group switch
            {
                0 => 30.0074, // New Cairo / Tagamoa
                1 => 30.1026, // Heliopolis
                2 => 30.0566, // Nasr City
                3 => 29.9602, // Maadi
                4 => 30.0609, // Zamalek
                5 => 29.9853, // Giza / Pyramids
                6 => 30.0877, // Shoubra
                7 => 30.0207, // Mokattam
                8 => 30.0614, // Rehab City
                9 => 30.0444, // Cairo Downtown
                _ => 30.0444
            };

            double groupLng = group switch
            {
                0 => 31.4913,
                1 => 31.3326,
                2 => 31.3438,
                3 => 31.2569,
                4 => 31.2197,
                5 => 31.1386,
                6 => 31.2461,
                7 => 31.2882,
                8 => 31.4922,
                9 => 31.2357,
                _ => 31.2357
            };

            double angle = (number * 36) * Math.PI / 180.0;
            double radius = 0.008 + (number * 0.006);
            return (groupLat + radius * Math.Sin(angle), groupLng + radius * Math.Cos(angle));
        }
    }
}
