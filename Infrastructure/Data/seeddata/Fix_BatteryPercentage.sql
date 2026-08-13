-- ============================================================
-- Fix_BatteryPercentage.sql
-- Fixes 11 traps that were showing 0% battery
-- Formula: battery = 100 - (trans*0.05) - (days*1.85)
-- Inactive traps (T18,T19): value stored directly
-- Run AFTER FullSeed_AllTables.sql
-- ============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @T00 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000000';
DECLARE @T01 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000001';
DECLARE @T02 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000002';
DECLARE @T03 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000003';
DECLARE @T04 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000004';
DECLARE @T05 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000005';
DECLARE @T06 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000006';
DECLARE @T07 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000007';
DECLARE @T08 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000008';
DECLARE @T09 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000009';
DECLARE @T10 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000010';
DECLARE @T11 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000011';
DECLARE @T12 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000012';
DECLARE @T13 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000013';
DECLARE @T14 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000014';
DECLARE @T15 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000015';
DECLARE @T16 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000016';
DECLARE @T17 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000017';
DECLARE @T18 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000018';
DECLARE @T19 UNIQUEIDENTIFIER = N'A0000000-0000-0000-0000-000000000019';

-- ============================================================
-- UPDATE each trap
-- ============================================================

-- T00 | Green   | Active | Battery=90% | Days=4 | Trans=48
UPDATE [Traps] SET
    [BatteryPercentage]  = 90,
    [OperatingDays]      = 4,
    [TotalTransmissions] = 48,
    [StartTime]          = DATEADD(day,-4,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T00;

-- T01 | Green   | Active | Battery=87% | Days=5 | Trans=66
UPDATE [Traps] SET
    [BatteryPercentage]  = 87,
    [OperatingDays]      = 5,
    [TotalTransmissions] = 66,
    [StartTime]          = DATEADD(day,-5,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T01;

-- T02 | Green   | Active | Battery=85% | Days=6 | Trans=72
UPDATE [Traps] SET
    [BatteryPercentage]  = 85,
    [OperatingDays]      = 6,
    [TotalTransmissions] = 72,
    [StartTime]          = DATEADD(day,-6,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T02;

-- T03 | Green   | Active | Battery=82% | Days=7 | Trans=96
UPDATE [Traps] SET
    [BatteryPercentage]  = 82,
    [OperatingDays]      = 7,
    [TotalTransmissions] = 96,
    [StartTime]          = DATEADD(day,-7,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T03;

-- T04 | Green   | Active | Battery=80% | Days=8 | Trans=96
UPDATE [Traps] SET
    [BatteryPercentage]  = 80,
    [OperatingDays]      = 8,
    [TotalTransmissions] = 96,
    [StartTime]          = DATEADD(day,-8,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T04;

-- T05 | Green   | Active | Battery=78% | Days=9 | Trans=108
UPDATE [Traps] SET
    [BatteryPercentage]  = 78,
    [OperatingDays]      = 9,
    [TotalTransmissions] = 108,
    [StartTime]          = DATEADD(day,-9,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T05;

-- T06 | Yellow  | Active | Battery=61% | Days=16 | Trans=192
UPDATE [Traps] SET
    [BatteryPercentage]  = 61,
    [OperatingDays]      = 16,
    [TotalTransmissions] = 192,
    [StartTime]          = DATEADD(day,-16,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T06;

-- T07 | Yellow  | Active | Battery=56% | Days=18 | Trans=216
UPDATE [Traps] SET
    [BatteryPercentage]  = 56,
    [OperatingDays]      = 18,
    [TotalTransmissions] = 216,
    [StartTime]          = DATEADD(day,-18,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T07;

-- T08 | Yellow  | Active | Battery=51% | Days=20 | Trans=240
UPDATE [Traps] SET
    [BatteryPercentage]  = 51,
    [OperatingDays]      = 20,
    [TotalTransmissions] = 240,
    [StartTime]          = DATEADD(day,-20,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T08;

-- T09 | Orange  | Active | Battery=34% | Days=27 | Trans=324
UPDATE [Traps] SET
    [BatteryPercentage]  = 34,
    [OperatingDays]      = 27,
    [TotalTransmissions] = 324,
    [StartTime]          = DATEADD(day,-27,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T09;

-- T10 | Orange  | Active | Battery=29% | Days=29 | Trans=348
UPDATE [Traps] SET
    [BatteryPercentage]  = 29,
    [OperatingDays]      = 29,
    [TotalTransmissions] = 348,
    [StartTime]          = DATEADD(day,-29,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T10;

-- T11 | Orange  | Active | Battery=24% | Days=31 | Trans=372
UPDATE [Traps] SET
    [BatteryPercentage]  = 24,
    [OperatingDays]      = 31,
    [TotalTransmissions] = 372,
    [StartTime]          = DATEADD(day,-31,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T11;

-- T12 | Red     | Active | Battery=17% | Days=34 | Trans=408
UPDATE [Traps] SET
    [BatteryPercentage]  = 17,
    [OperatingDays]      = 34,
    [TotalTransmissions] = 408,
    [StartTime]          = DATEADD(day,-34,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T12;

-- T13 | Red     | Active | Battery=14% | Days=35 | Trans=420
UPDATE [Traps] SET
    [BatteryPercentage]  = 14,
    [OperatingDays]      = 35,
    [TotalTransmissions] = 420,
    [StartTime]          = DATEADD(day,-35,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T13;

-- T14 | Red     | Active | Battery=12% | Days=36 | Trans=432
UPDATE [Traps] SET
    [BatteryPercentage]  = 12,
    [OperatingDays]      = 36,
    [TotalTransmissions] = 432,
    [StartTime]          = DATEADD(day,-36,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T14;

-- T15 | Red     | Active | Battery=9% | Days=37 | Trans=444
UPDATE [Traps] SET
    [BatteryPercentage]  = 9,
    [OperatingDays]      = 37,
    [TotalTransmissions] = 444,
    [StartTime]          = DATEADD(day,-37,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T15;

-- T16 | Red     | Active | Battery=7% | Days=38 | Trans=456
UPDATE [Traps] SET
    [BatteryPercentage]  = 7,
    [OperatingDays]      = 38,
    [TotalTransmissions] = 456,
    [StartTime]          = DATEADD(day,-38,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T16;

-- T17 | Red     | Active | Battery=4% | Days=39 | Trans=468
UPDATE [Traps] SET
    [BatteryPercentage]  = 4,
    [OperatingDays]      = 39,
    [TotalTransmissions] = 468,
    [StartTime]          = DATEADD(day,-39,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T17;

-- T18 | Red     | Inactive | Battery=15% (stored, code does not recalc Inactive)
-- BatteryPercentage stored directly; Days/Trans kept low so no conflict
UPDATE [Traps] SET
    [BatteryPercentage]  = 15,
    [OperatingDays]      = 55,
    [TotalTransmissions] = 200,
    [StartTime]          = DATEADD(day,-55,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T18;

-- T19 | Red     | Inactive | Battery=20% (stored, code does not recalc Inactive)
UPDATE [Traps] SET
    [BatteryPercentage]  = 20,
    [OperatingDays]      = 50,
    [TotalTransmissions] = 180,
    [StartTime]          = DATEADD(day,-50,SYSUTCDATETIME()),
    [UpdatedAt]          = SYSUTCDATETIME()
WHERE [Id] = @T19;

COMMIT TRANSACTION;

-- ============================================================
-- VERIFICATION
-- ============================================================
SELECT
    CAST([TrapNumber] AS INT)  AS [Trap],
    [status]                   AS [Status],
    [IndicatorStatus]          AS [Color],
    [BatteryPercentage]        AS [Battery%],
    [OperatingDays]            AS [Days],
    [TotalTransmissions]       AS [Trans],
    CAST(100.0 - ([TotalTransmissions]*0.05)
              - ([OperatingDays]*1.85)
    AS DECIMAL(5,1))           AS [Formula%],
    CASE
        WHEN [BatteryPercentage] >= 70 THEN N'High'
        WHEN [BatteryPercentage] >= 40 THEN N'Medium'
        WHEN [BatteryPercentage] >= 15 THEN N'Low'
        WHEN [BatteryPercentage] >  0  THEN N'Critical'
        ELSE                               N'DEAD (0%)'
    END                        AS [Level]
FROM [Traps]
ORDER BY CAST([TrapNumber] AS INT);
GO