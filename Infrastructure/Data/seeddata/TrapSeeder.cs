using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.seeddata
{
    public class TrapSeeder
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<TrapSeeder> _logger;

        public TrapSeeder(IApplicationDbContext context, ILogger<TrapSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.Traps.AnyAsync())
            {
                // Check if traps are distributed in the new Cairo districts layout. If not, redistribute them.
                var hasNewLayout = await _context.Traps.AnyAsync(t => t.TrapGroup == "1" && t.Latitude != null && Math.Abs(t.Latitude.Value - 30.1026) < 0.01);
                if (!hasNewLayout)
                {
                    _logger.LogInformation("Redistributing all traps within Cairo districts (Heliopolis, Nasr City, Maadi, etc.) with improved spacing...");
                    var allTraps = await _context.Traps.ToListAsync();
                    foreach (var trap in allTraps)
                    {
                        int group = int.TryParse(trap.TrapGroup, out var g) ? g : 0;
                        int number = int.TryParse(trap.TrapNumber, out var n) ? n : 0;
                        
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
                            _ => 30.0444  // Cairo Center
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
                        double radius = 0.008 + (number * 0.006); // Space out traps clearly (approx 600m - 1cm on screen)
                        trap.Latitude = groupLat + radius * Math.Sin(angle);
                        trap.Longitude = groupLng + radius * Math.Cos(angle);
                        
                        _context.Traps.Update(trap);
                    }
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("All traps redistributed within Cairo successfully.");
                }

                _logger.LogInformation("Traps already seeded. Checking for missing coordinates...");
                var hasNullCoordinates = await _context.Traps.AnyAsync(t => t.Latitude == null || t.Longitude == null);
                if (hasNullCoordinates)
                {
                    _logger.LogInformation("Some traps have null coordinates. Updating coordinates from seed data...");
                    var seedAssembly = typeof(TrapSeeder).Assembly;
                    var seedResName = "Infrastructure.Data.SeedData.traps_seed.json";
                    using var seedStream = seedAssembly.GetManifestResourceStream(seedResName);
                    if (seedStream != null)
                    {
                        using var seedReader = new StreamReader(seedStream);
                        var seedJson = await seedReader.ReadToEndAsync();
                        var seedDataItems = JsonSerializer.Deserialize<List<TrapSeedDto>>(seedJson);
                        if (seedDataItems != null && seedDataItems.Any())
                        {
                            var seedDict = seedDataItems
                                .GroupBy(dto => dto.TrapNumber)
                                .ToDictionary(g => g.Key, g => g.First());
                            var existingTraps = await _context.Traps.ToListAsync();
                            foreach (var trap in existingTraps)
                            {
                                if (seedDict.TryGetValue(trap.TrapNumber, out var seedDto))
                                {
                                    trap.Latitude = seedDto.Latitude;
                                    trap.Longitude = seedDto.Longitude;
                                }
                            }
                            await _context.SaveChangesAsync(CancellationToken.None);
                            _logger.LogInformation("Updated coordinates for existing traps.");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Seed file '{seedResName}' not found. Available resources: {string.Join(", ", seedAssembly.GetManifestResourceNames())}");
                    }
                }

                if (!await _context.CaptureEvents.AnyAsync())
                {
                    _logger.LogInformation("No capture events found. Seeding 2 weeks of capture events for existing traps...");
                    var existingTraps = await _context.Traps.ToListAsync();
                    await SeedCaptureEventsAndBaitMeasurementsAsync(existingTraps);
                }

                // Ensure at least 5 traps have recent activity (are connected)
                var thresholdTime = DateTime.UtcNow.AddHours(-2);
                var hasRecentBait = await _context.TrapBaitMeasurement.AnyAsync(m => m.MeasurementTime >= thresholdTime);
                var hasRecentCapture = await _context.CaptureEvents.AnyAsync(c => c.CaptureTime >= thresholdTime);
                
                if (!hasRecentBait && !hasRecentCapture)
                {
                    _logger.LogInformation("No traps have recent activity. Seeding recent connection data for some traps...");
                    var activeTraps = await _context.Traps
                        .Where(t => t.status == "Active")
                        .Take(5)
                        .ToListAsync();
                        
                    foreach (var trap in activeTraps)
                    {
                        var recentBait = new TrapBaitMeasurement
                        {
                            Id = Guid.NewGuid(),
                            TrapId = trap.Id,
                            MeasurementTime = DateTime.UtcNow.AddMinutes(-30),
                            BaitWeightGrams = 85.5,
                            SignalStrength = -75
                        };
                        await _context.TrapBaitMeasurement.AddAsync(recentBait);
                        
                        // Also update trap's last entry date and other stats
                        trap.LastEntryDate = recentBait.MeasurementTime;
                        trap.TotalTransmissions += 1;
                        trap.UpdateBattery();
                        trap.UpdateIndicatorStatus();
                        _context.Traps.Update(trap);
                    }
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Seeded recent connection data successfully.");
                }

                // Ensure at least 1 active traps have low battery (under 30%)
                var allDbTraps = await _context.Traps.Where(t => t.status == "Active").ToListAsync();
                var lowBatteryCount = allDbTraps
                    .Count(t => Trap.CalculateBatteryPercentage(t.status, t.BatteryPercentage, t.StartTime, t.TotalTransmissions) <= 30);
                    
                if (lowBatteryCount < 1)
                {
                    _logger.LogInformation("Not enough low battery traps. Updating some traps to have low battery...");
                    var trapsToUpdate = allDbTraps.Where(t => t.TrapNumber != "0").Take(1).ToList();
                    foreach (var trap in trapsToUpdate)
                    {
                        trap.StartTime = DateTime.UtcNow.AddDays(-42);
                        trap.UpdateBattery(forceCalculate: true);
                        _context.Traps.Update(trap);
                    }
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Updated traps to have low battery successfully.");
                }

                // Ensure at least 3 active traps have a rodent capture event today (recent capture)
                var recentCaptures = await _context.CaptureEvents
                    .Where(c => c.CaptureTime >= DateTime.UtcNow.AddDays(-1))
                    .ToListAsync();
                if (recentCaptures.Count < 3)
                {
                    _logger.LogInformation("Seeding recent rodent capture events for some traps...");
                    var trapsForCaptures = await _context.Traps
                        .Where(t => t.status == "Active")
                        .Take(3)
                        .ToListAsync();
                    
                    var random = new Random();
                    foreach (var trap in trapsForCaptures)
                    {
                        var baseDate = DateTime.UtcNow.Date;
                        var hour = GenerateRealisticRodentHour(random);
                        var captureTime = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour, random.Next(0, 60), random.Next(0, 60), DateTimeKind.Utc);
                        int sensorCount = random.Next(3, 7);
                        int rodentWeight = random.Next(150, 400);

                        var capture = new CaptureEvent
                        {
                            Id = Guid.NewGuid(),
                            TrapId = trap.Id,
                            CaptureTime = captureTime,
                            ActiveSensorCount = sensorCount,
                            RodentWeight = new Domain.ValueObjects.RodentWeight(rodentWeight),
                            Status = "Active",
                            SignalStrength = -75.0,
                            NumberOfTransmissions = trap.TotalTransmissions + 1,
                            Duration = random.Next(5, 31)
                        };
                        capture.SetLengthFromSensors(sensorCount);
                        capture.DetermineRodentType();

                        await _context.CaptureEvents.AddAsync(capture);

                        // Also add a bait measurement corresponding to this capture
                        var bait = new BaitMeasurement
                        {
                            Id = Guid.NewGuid(),
                            TrapId = trap.Id,
                            CaptureEventId = capture.Id,
                            MeasurementTime = captureTime,
                            BaitWeightGrams = Math.Round(random.NextDouble() * 30.0 + 20.0, 2)
                        };
                        await _context.BaitMeasurements.AddAsync(bait);

                        // Also make sure they have a recent TrapBaitMeasurement so they are considered connected!
                        var recentBait = new TrapBaitMeasurement
                        {
                            Id = Guid.NewGuid(),
                            TrapId = trap.Id,
                            MeasurementTime = captureTime.AddMinutes(5),
                            BaitWeightGrams = bait.BaitWeightGrams,
                            SignalStrength = -75
                        };
                        await _context.TrapBaitMeasurement.AddAsync(recentBait);

                        // Update the trap's LastEntryDate to ensure it shows high activity (Red)
                        trap.LastEntryDate = captureTime;
                        trap.TotalTransmissions += 2;
                        trap.UpdateIndicatorStatus();
                        _context.Traps.Update(trap);
                    }
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Seeded recent rodent capture events successfully.");
                }

                // Correct existing capture events in the database to have realistic rodent hours (nocturnal behavior)
                var allDbCaptures = await _context.CaptureEvents.ToListAsync();
                var allDbBaits = await _context.BaitMeasurements.ToListAsync();
                var dbBaitDict = allDbBaits.Where(b => b.CaptureEventId.HasValue).ToDictionary(b => b.CaptureEventId.Value);
                var randGen = new Random();
                bool dbUpdated = false;

                foreach (var capture in allDbCaptures)
                {
                    int realisticHour = GenerateRealisticRodentHour(randGen);
                    var oldTime = capture.CaptureTime;
                    if (oldTime.Hour != realisticHour)
                    {
                        var newTime = new DateTime(oldTime.Year, oldTime.Month, oldTime.Day, realisticHour, randGen.Next(0, 60), randGen.Next(0, 60), DateTimeKind.Utc);
                        capture.CaptureTime = newTime;
                        _context.CaptureEvents.Update(capture);

                        if (dbBaitDict.TryGetValue(capture.Id, out var bait))
                        {
                            bait.MeasurementTime = newTime;
                            _context.BaitMeasurements.Update(bait);
                        }
                        dbUpdated = true;
                    }
                }

                if (dbUpdated)
                {
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Successfully corrected capture event times in the database to reflect realistic nocturnal rodent activity.");
                }

                return;
            }

            var assembly = typeof(TrapSeeder).Assembly;
            var resourceName = "Infrastructure.Data.SeedData.traps_seed.json";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logger.LogError("Seed file not found.");
                return;
            }

            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var seedItems = JsonSerializer.Deserialize<List<TrapSeedDto>>(json);
            if (seedItems == null || !seedItems.Any())
            {
                _logger.LogWarning("No seed data.");
                return;
            }

            var traps = seedItems.Select(dto =>
            {
                int group = int.TryParse(dto.GroupNumber, out var g) ? g : 0;
                int number = int.TryParse(dto.TrapNumber, out var n) ? n : 0;
                
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
                    _ => 30.0444  // Cairo Center
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
                double lat = groupLat + radius * Math.Sin(angle);
                double lng = groupLng + radius * Math.Cos(angle);

                return new Trap
                {
                    Id = Guid.NewGuid(),
                    TrapNumber = dto.TrapNumber,
                    TrapGroup = dto.GroupNumber,
                    status = dto.Status,
                    StartTime = dto.StartTime,
                    BatteryPercentage = dto.BatteryPercentage,
                    IndicatorStatus = (IndicatorStatus)dto.IndicatorStatus,
                    SignalStrength = dto.SignalStrength,
                    TotalTransmissions = dto.TotalTransmissions,
                    OperatingDays = dto.OperatingDays,
                    Latitude = lat,
                    Longitude = lng
                };
            }).ToList();

            await _context.Traps.AddRangeAsync(traps);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation($"Seeded {traps.Count} traps.");

            await SeedCaptureEventsAndBaitMeasurementsAsync(traps);
        }

        private async Task SeedCaptureEventsAndBaitMeasurementsAsync(List<Trap> traps)
        {
            var random = new Random();
            var captureEvents = new List<CaptureEvent>();
            var baitMeasurements = new List<BaitMeasurement>();

            foreach (var trap in traps)
            {
                // Generate 5 to 10 events per trap
                int numEvents = random.Next(5, 11);
                double currentBaitWeight = 100.0;

                for (int i = 0; i < numEvents; i++)
                {
                    // Spread events chronologically over the last 14 days
                    double dayOffset = 14.0 - (14.0 * i / numEvents);
                    var baseTime = DateTime.UtcNow.AddDays(-dayOffset).Date;
                    var hour = GenerateRealisticRodentHour(random);
                    var captureTime = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, hour, random.Next(0, 60), random.Next(0, 60), DateTimeKind.Utc);

                    int sensorCount = random.Next(1, 7);
                    int rodentWeight = sensorCount switch
                    {
                        1 or 2 => random.Next(15, 31),
                        3 or 4 => random.Next(150, 251),
                        _ => random.Next(200, 501)
                    };

                    var capture = new CaptureEvent
                    {
                        Id = Guid.NewGuid(),
                        TrapId = trap.Id,
                        CaptureTime = captureTime,
                        ActiveSensorCount = sensorCount,
                        RodentWeight = new Domain.ValueObjects.RodentWeight(rodentWeight),
                        Status = "Active",
                        SignalStrength = Math.Round(random.NextDouble() * 5.0 + 1.0, 1),
                        NumberOfTransmissions = i + 1,
                        Duration = random.Next(5, 31)
                    };

                    capture.SetLengthFromSensors(sensorCount);
                    capture.DetermineRodentType();

                    captureEvents.Add(capture);

                    // Bait consumption
                    currentBaitWeight -= random.NextDouble() * 15.0;
                    if (currentBaitWeight < 10.0)
                    {
                        currentBaitWeight = 100.0; // refill
                    }

                    var bait = new BaitMeasurement
                    {
                        Id = Guid.NewGuid(),
                        TrapId = trap.Id,
                        CaptureEventId = capture.Id,
                        MeasurementTime = captureTime,
                        BaitWeightGrams = Math.Round(currentBaitWeight, 2)
                    };
                    baitMeasurements.Add(bait);

                    // Update trap properties
                    trap.LastEntryDate = captureTime;
                    trap.TotalTransmissions = i + 1;
                }
            }

            await _context.CaptureEvents.AddRangeAsync(captureEvents);
            await _context.BaitMeasurements.AddRangeAsync(baitMeasurements);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation($"Seeded {captureEvents.Count} capture events and {baitMeasurements.Count} bait measurements.");
        }

        private static int GenerateRealisticRodentHour(Random random)
        {
            // 85% chance of being in night hours (20 to 23, or 0 to 4)
            if (random.NextDouble() < 0.85)
            {
                int[] nightHours = { 20, 21, 22, 23, 0, 1, 2, 3, 4 };
                return nightHours[random.Next(nightHours.Length)];
            }
            else
            {
                int[] dayHours = { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
                return dayHours[random.Next(dayHours.Length)];
            }
        }

        private class TrapSeedDto
        {
            public string TrapNumber { get; set; } = string.Empty;
            public string GroupNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public int BatteryPercentage { get; set; }
            public int IndicatorStatus { get; set; }
            public float SignalStrength { get; set; }
            public int TotalTransmissions { get; set; }
            public int OperatingDays { get; set; }
            public bool IsActive { get; set; }
            public string? CustomerId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
