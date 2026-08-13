using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
            // 1. Load seed coordinates
            List<TrapSeedDto> seedItems = new();
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Infrastructure");
                var resourceName = "Infrastructure.Data.SeedData.traps_seed.json";
                if (assembly != null)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var json = await reader.ReadToEndAsync(cancellationToken);
                        seedItems = JsonSerializer.Deserialize<List<TrapSeedDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    }
                }
            }
            catch { }

            if (!seedItems.Any())
            {
                try
                {
                    var filePath = "D:/system/Infrastructure/Data/seeddata/traps_seed.json";
                    if (File.Exists(filePath))
                    {
                        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                        seedItems = JsonSerializer.Deserialize<List<TrapSeedDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    }
                }
                catch { }
            }

            // 2. Load all TrapGroups assigned to customers
            var groupQuery = _context.TrapGroups
                .Where(tg => tg.CustomerId != null);

            if (!string.IsNullOrEmpty(request.GroupNumber))
                groupQuery = groupQuery.Where(tg => tg.TrapGroup == request.GroupNumber);

            var assignedGroups = await groupQuery.AsNoTracking().ToListAsync(cancellationToken);

            var distinctGroups = assignedGroups
                .DistinctBy(tg => new { tg.TrapGroup, tg.TrapNumber })
                .ToList();

            // 3. Load active traps from database
            var trapQuery = _context.Traps.AsNoTracking();
            if (!string.IsNullOrEmpty(request.GroupNumber))
                trapQuery = trapQuery.Where(t => t.TrapGroup == request.GroupNumber);

            var databaseTraps = await trapQuery.ToListAsync(cancellationToken);
            var dbTrapsDict = databaseTraps
                .GroupBy(t => new { t.TrapGroup, t.TrapNumber })
                .ToDictionary(g => g.Key, g => g.First());

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

            // 4. Map and merge
            var results = new List<TrapDetailDto>();
            foreach (var tg in distinctGroups)
            {
                var key = new { tg.TrapGroup, tg.TrapNumber };
                dbTrapsDict.TryGetValue(key, out var dbTrap);

                // Resolve coordinates from seed
                double latitude = 29.9088; // Default fallback
                double longitude = 31.7900; // Default fallback

                // Use robust matching helper to find match in seed items
                var seed = seedItems.FirstOrDefault(s => MatchTrapNumber(s.GroupNumber, s.TrapNumber, tg.TrapGroup, tg.TrapNumber));

                // Fall back to any trap in the same group in seed data if no exact match is found
                if (seed == null)
                {
                    seed = seedItems.FirstOrDefault(s => s.GroupNumber == tg.TrapGroup);
                }

                if (seed != null)
                {
                    latitude = seed.Latitude;
                    longitude = seed.Longitude;
                }

                // If database record exists, override coordinates with database values if database has them
                if (dbTrap != null && dbTrap.Latitude.HasValue && dbTrap.Longitude.HasValue)
                {
                    latitude = dbTrap.Latitude.Value;
                    longitude = dbTrap.Longitude.Value;
                }

                if (dbTrap != null)
                {
                    IndicatorStatus status = Trap.CalculateIndicatorStatus(dbTrap.LastEntryDate);
                    int days = dbTrap.LastEntryDate.HasValue ? (int)(DateTime.UtcNow - dbTrap.LastEntryDate.Value).TotalDays : 999;

                    bool isConnected = false;
                    DateTime? lastActivity = null;

                    if (latestMeasurements.TryGetValue(dbTrap.Id, out var latestBaitTime))
                    {
                        lastActivity = latestBaitTime;
                    }

                    if (latestCaptures.TryGetValue(dbTrap.Id, out var latestCaptureTime))
                    {
                        if (lastActivity == null || latestCaptureTime > lastActivity.Value)
                        {
                            lastActivity = latestCaptureTime;
                        }
                    }

                    if (lastActivity.HasValue)
                    {
                        if ((DateTime.UtcNow - lastActivity.Value).TotalHours <= 2)
                        {
                            isConnected = true;
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
                else
                {
                    // Trap is in groups but has not checked in (does not exist in database)
                    results.Add(new TrapDetailDto
                    {
                        Latitude = latitude,
                        Longitude = longitude,
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
            }

            var orderedResults = results.OrderBy(d => d.IsOffline).ThenBy(d => d.GroupNumber).ThenBy(d => d.TrapNumber).ToList();
            if (request.Limit.HasValue)
            {
                orderedResults = orderedResults.Take(request.Limit.Value).ToList();
            }
            return orderedResults;
        }

        private static bool MatchTrapNumber(string seedGroup, string seedTrap, string targetGroup, string targetTrap)
        {
            if (seedTrap.Contains('-'))
            {
                var parts = seedTrap.Split('-');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0], out int g) && int.TryParse(parts[1], out int t))
                    {
                        seedGroup = g.ToString();
                        seedTrap = t.ToString();
                    }
                }
            }

            if (int.TryParse(seedGroup, out int sgVal)) seedGroup = sgVal.ToString();
            if (int.TryParse(seedTrap, out int stVal)) seedTrap = stVal.ToString();
            if (int.TryParse(targetGroup, out int tgVal)) targetGroup = tgVal.ToString();
            if (int.TryParse(targetTrap, out int ttVal)) targetTrap = ttVal.ToString();

            return seedGroup.Equals(targetGroup, StringComparison.OrdinalIgnoreCase) && 
                   seedTrap.Equals(targetTrap, StringComparison.OrdinalIgnoreCase);
        }

        private class TrapSeedDto
        {
            public string TrapNumber { get; set; } = string.Empty;
            public string GroupNumber { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
