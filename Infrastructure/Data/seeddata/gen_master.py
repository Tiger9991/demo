"""
Master SQL Generator
Generates: MasterSeed_Complete.sql
Combines ALL tables in correct order with corrected battery values built-in.
Run this script whenever you need to regenerate the seed file.
"""

import pathlib, math

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────
def calc_battery(days, trans):
    exact = 100.0 - (trans * 0.05) - (days * 1.85)
    return max(0, min(100, round(exact)))

GUIDS = {
    'CUST' : '4A7B1C3D-22E0-4F55-9A8B-1234567890AB',
    'T00'  : 'A0000000-0000-0000-0000-000000000000',
    'T01'  : 'A0000000-0000-0000-0000-000000000001',
    'T02'  : 'A0000000-0000-0000-0000-000000000002',
    'T03'  : 'A0000000-0000-0000-0000-000000000003',
    'T04'  : 'A0000000-0000-0000-0000-000000000004',
    'T05'  : 'A0000000-0000-0000-0000-000000000005',
    'T06'  : 'A0000000-0000-0000-0000-000000000006',
    'T07'  : 'A0000000-0000-0000-0000-000000000007',
    'T08'  : 'A0000000-0000-0000-0000-000000000008',
    'T09'  : 'A0000000-0000-0000-0000-000000000009',
    'T10'  : 'A0000000-0000-0000-0000-000000000010',
    'T11'  : 'A0000000-0000-0000-0000-000000000011',
    'T12'  : 'A0000000-0000-0000-0000-000000000012',
    'T13'  : 'A0000000-0000-0000-0000-000000000013',
    'T14'  : 'A0000000-0000-0000-0000-000000000014',
    'T15'  : 'A0000000-0000-0000-0000-000000000015',
    'T16'  : 'A0000000-0000-0000-0000-000000000016',
    'T17'  : 'A0000000-0000-0000-0000-000000000017',
    'T18'  : 'A0000000-0000-0000-0000-000000000018',
    'T19'  : 'A0000000-0000-0000-0000-000000000019',
}

# ─────────────────────────────────────────────────────────────────────────────
# TRAP SPECS
# (name, days, trans, battery, status, indicator, signal, lat_offset, lon_offset)
# Battery is verified against formula; Inactive stored directly
# ─────────────────────────────────────────────────────────────────────────────
TRAP_SPECS = [
    #  name   days  trans  batt  status    indicator  signal   lat_off    lon_off
    ('T00',   8,   10,   85, 'Active',  'Green',   -58.0,  +0.002000, +0.002000),
    ('T01',   8,   30,   84, 'Active',  'Green',   -61.5,  +0.002200, +0.002200),
    ('T02',   8,   50,   83, 'Active',  'Green',   -63.0,  +0.002400, +0.002400),
    ('T03',   7,   10,   87, 'Active',  'Yellow',  -64.5,  +0.002600, +0.002600),
    ('T04',   7,   30,   86, 'Active',  'Yellow',  -62.0,  -0.000200, -0.000200),
    ('T05',   7,   50,   85, 'Active',  'Yellow',  -65.0,  -0.000400, -0.000400),
    ('T06',   5,   20,   90, 'Active',  'Orange',  -72.0,  -0.000600, -0.000600),
    ('T07',   5,   40,   89, 'Active',  'Orange',  -75.5,  -0.000800, -0.000800),
    ('T08',   5,   60,   88, 'Active',  'Orange',  -77.0,  -0.001000, -0.001000),
    ('T09',  16,  192,   61, 'Active',  'Orange',  -82.0,  -0.001200, -0.001200),
    ('T10',  18,  216,   56, 'Active',  'Orange',  -86.5,  -0.001400, -0.001400),
    ('T11',  20,  240,   51, 'Active',  'Orange',  -89.0,  -0.001600, -0.001600),
    ('T12',  27,  324,   34, 'Active',  'Red',     -93.0,  -0.001800, -0.001800),
    ('T13',  29,  348,   29, 'Active',  'Red',     -97.0,  -0.002000, -0.002000),
    ('T14',  31,  372,   24, 'Active',  'Red',    -101.5,  -0.002200, -0.002200),
    ('T15',  34,  408,   17, 'Active',  'Red',    -105.0,  -0.002400, -0.002400),
    ('T16',  36,  432,   12, 'Active',  'Red',    -109.0,  -0.002600, -0.002600),
    ('T17',  38,  456,    7, 'Active',  'Red',    -112.0,  -0.002800, -0.002800),
    ('T18',  55,  200,   15, 'Inactive','Red',    -120.0,  -0.003000, -0.003000),
    ('T19',  50,  180,   20, 'Inactive','Red',    -118.0,  -0.003200, -0.003200),
]

BASE_LAT = 30.110828993532092
BASE_LON = 31.343926237921249

# LastEntryDate offsets (hours for Green/Red recent, days for others)
# Green (T00-T02) => 8 days ago (>= 7 days)
# Yellow (T03-T05) => 150 hours ago (6.25 days)
# Orange (T06-T11) => 4 days ago (3-6 days)
# Red (T12-T17) => 1 day ago (< 3 days)
LAST_ENTRY = {
    'T00': 'DATEADD(day, -8,   SYSUTCDATETIME())',
    'T01': 'DATEADD(day, -8,   SYSUTCDATETIME())',
    'T02': 'DATEADD(day, -8,   SYSUTCDATETIME())',
    'T03': 'DATEADD(hour,-150, SYSUTCDATETIME())',
    'T04': 'DATEADD(hour,-150, SYSUTCDATETIME())',
    'T05': 'DATEADD(hour,-150, SYSUTCDATETIME())',
    'T06': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T07': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T08': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T09': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T10': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T11': 'DATEADD(day, -4,   SYSUTCDATETIME())',
    'T12': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T13': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T14': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T15': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T16': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T17': 'DATEADD(day, -1,   SYSUTCDATETIME())',
    'T18': 'DATEADD(day, -20,  SYSUTCDATETIME())',
    'T19': 'DATEADD(day, -25,  SYSUTCDATETIME())',
}

# Capture events: (trap, hrs_or_days_ago, unit, sensors, weight, length, rodent, duration, signal)
CAPTURE_EVENTS = [
    ('T00','hour',-1,  2,  22,  8.5,'NormalRat',   30,-58.0),
    ('T00','day', -5,  4, 200, 18.0,'NorwegianRat',45,-59.0),
    ('T00','day',-10,  3, 180, 17.0,'ClimbingRat', 40,-60.0),
    ('T01','hour',-3,  2,  18,  7.5,'NormalRat',   20,-61.5),
    ('T01','day', -8,  5, 350, 22.0,'NorwegianRat',55,-62.0),
    ('T02','hour',-5,  1,  15,  7.0,'NormalRat',   15,-63.0),
    ('T02','day', -3,  3, 160, 16.5,'ClimbingRat', 35,-63.5),
    ('T02','day',-12,  6,  50,  5.0,'Unknown',     60,-64.0),
    ('T03','hour',-10, 2,  25,  9.0,'NormalRat',   25,-64.5),
    ('T03','day', -7,  4, 220, 19.0,'NorwegianRat',50,-65.0),
    ('T04','hour',-12, 3, 175, 16.0,'ClimbingRat', 38,-62.0),
    ('T04','day', -4,  2,  20,  8.0,'NormalRat',   22,-62.5),
    ('T05','hour',-20, 5, 420, 24.0,'NorwegianRat',60,-65.0),
    ('T05','day', -9,  1,  10,  4.0,'Unknown',     12,-65.5),
    ('T06','day', -6,  2,  28,  9.5,'NormalRat',   28,-72.0),
    ('T06','day',-15,  3, 170, 17.0,'ClimbingRat', 36,-72.5),
    ('T07','day', -6,  4, 300, 20.0,'NorwegianRat',48,-75.5),
    ('T07','day',-20,  2,  16,  7.2,'NormalRat',   18,-76.0),
    ('T08','day', -6,  1,  12,  4.5,'Unknown',     10,-77.0),
    ('T08','day',-18,  3, 155, 16.2,'ClimbingRat', 33,-77.5),
    ('T09','day', -4,  2,  24,  8.8,'NormalRat',   26,-82.0),
    ('T09','day',-10,  5, 450, 25.0,'NorwegianRat',65,-82.5),
    ('T09','day',-25,  3, 185, 18.5,'NorwegianRat',42,-83.0),
    ('T10','day', -5,  2,  17,  7.8,'NormalRat',   20,-86.5),
    ('T10','day',-14,  4, 230, 20.5,'NorwegianRat',52,-87.0),
    ('T11','day', -3,  3, 160, 16.8,'ClimbingRat', 37,-89.0),
    ('T11','day',-22,  1,  11,  4.2,'Unknown',     11,-89.5),
    ('T12','hour',-1,  2,  19,  7.5,'NormalRat',   22,-93.0),
    ('T12','day', -2,  5, 380, 23.0,'NorwegianRat',58,-93.5),
    ('T12','day',-11,  3, 165, 17.5,'ClimbingRat', 39,-94.0),
    ('T13','hour',-2,  4, 260, 21.0,'NorwegianRat',50,-97.0),
    ('T13','day', -9,  2,  23,  9.2,'NormalRat',   24,-97.5),
    ('T14','hour',-5,  3, 170, 16.5,'ClimbingRat', 35,-101.5),
    ('T14','day', -6,  1,   8,  3.5,'Unknown',      9,-102.0),
    ('T15','hour',-10, 2,  27,  9.8,'NormalRat',   30,-105.0),
    ('T15','day', -4,  4, 210, 19.5,'NorwegianRat',48,-105.5),
    ('T16','hour',-20, 3, 155, 16.0,'ClimbingRat', 32,-109.0),
    ('T16','day', -8,  2,  21,  8.2,'NormalRat',   23,-109.5),
    ('T17','hour',-30, 5, 490, 26.0,'NorwegianRat',70,-112.0),
    ('T17','day', -7,  1,  14,  5.5,'Unknown',     14,-112.5),
    ('T15','day',-12,  3, 195, 18.2,'NorwegianRat',44,-106.0),
    ('T13','day',-18,  2,  26,  9.0,'NormalRat',   27, -98.0),
]

# TrapBaitMeasurements: (trap, signal, bait_start)
TBM_SPECS = [
    ('T00',-58.0,50.0),('T01',-61.5,50.0),('T02',-63.0,50.0),
    ('T03',-64.5,50.0),('T04',-62.0,50.0),('T05',-65.0,48.0),
    ('T06',-72.0,45.0),('T07',-75.5,42.0),('T08',-77.0,40.0),
    ('T09',-82.0,38.0),('T10',-86.5,36.0),('T11',-89.0,34.0),
    ('T12',-93.0,30.0),('T13',-97.0,28.0),('T14',-101.5,26.0),
    ('T15',-105.0,24.0),('T16',-109.0,22.0),('T17',-112.0,20.0),
]

# BaitMeasurements periodic: (trap, bait_start, decay_per_3days)
BM_PERIODIC = [
    ('T00',50.0,0.8),('T01',50.0,0.7),('T02',50.0,0.6),
    ('T03',50.0,0.5),('T04',50.0,0.7),('T05',50.0,0.6),
    ('T06',45.0,1.2),('T07',45.0,1.1),('T08',45.0,1.0),
    ('T09',40.0,1.5),('T10',40.0,1.4),('T11',40.0,1.3),
    ('T12',35.0,2.0),('T13',35.0,1.9),('T14',35.0,1.8),
    ('T15',30.0,2.2),('T16',30.0,2.1),('T17',30.0,2.0),
]

# ─────────────────────────────────────────────────────────────────────────────
# HOURS DEFINITION (30 days ago to 30 days ahead = 1440 hours total, every 2 hours)
# ─────────────────────────────────────────────────────────────────────────────
HOURS = list(range(-720, 721, 2))  # 721 readings

# ─────────────────────────────────────────────────────────────────────────────
# BUILD SQL
# ─────────────────────────────────────────────────────────────────────────────
L = []

def line(s=''): L.append(s)
def section(title):
    line()
    line('-- ' + '='*62)
    line(f'-- {title}')
    line('-- ' + '='*62)

section('MASTER SEED SCRIPT')
line('-- TrapsSystem Database - Complete Seed')
line('-- Tables: Customers | TrapGroups | Traps | CaptureEvents |')
line('--         TrapBaitMeasurements | BaitMeasurements')
line('--')
line('-- Data summary:')
line('--   Customers              : 1')
line('--   TrapGroups             : 20  (Group=1, TrapNumber 0-19)')
line('--   Traps                  : 20  (18 Active + 2 Inactive)')
line('--   CaptureEvents          : 42')
line(f'--   TrapBaitMeasurements   : {len(TBM_SPECS) * len(HOURS)} (18 traps x {len(HOURS)} readings every 2h)')
line(f'--   BaitMeasurements       : 222 (42 capture-linked + 180 periodic)')
line(f'--   TOTAL                  : ~{1 + 20 + 20 + 42 + len(TBM_SPECS)*len(HOURS) + 222} rows')
line('--')
line('-- Base location: Lat 30.110828993532092 / Lon 31.343926237921249')
line('-- Battery formula: 100 - (trans*0.05) - (days*1.85)')
line('-- Script is IDEMPOTENT (IF NOT EXISTS guards)')
line('-- Run once on a clean database')
line('-- ' + '='*62)
line('SET QUOTED_IDENTIFIER ON;')
line('SET ANSI_NULLS ON;')
line('SET NOCOUNT ON;')
line('BEGIN TRANSACTION;')
line()

# ── GUID DECLARATIONS ──
section('GUID DECLARATIONS')
cust_guid = GUIDS['CUST']
line(f"DECLARE @CustId    UNIQUEIDENTIFIER = N'{cust_guid}';")
line("DECLARE @TrapGroup NVARCHAR(50)     = N'1';")
line()
for name in [f'T{i:02d}' for i in range(20)]:
    comment = '  -- Disconnected' if name in ('T18','T19') else ''
    line(f"DECLARE @{name} UNIQUEIDENTIFIER = N'{GUIDS[name]}';{comment}")

# ── 1. CUSTOMERS ──
section('1. CUSTOMERS')
line('IF NOT EXISTS (SELECT 1 FROM [Customers] WHERE [Id] = @CustId)')
line('BEGIN')
line('    INSERT INTO [Customers]')
line('        ([Id],[CustomerNumber],[Name],[CustomerType],[Email],[Phone],[Address],[Status],[Notes],[IsDeleted],[CreatedAt],[UpdatedAt])')
line('    VALUES')
line("        (@CustId, N'CUS-2026-0001', N'شركة الحماية الذكية للآفات',")
line("         N'Company', N'info@smartpest-eg.com', N'+201000111222',")
line("         N'12 شارع التحرير، القاهرة، مصر', N'Active',")
line("         N'عميل رئيسي - نظام مصائد الطوارئ الميدانية',")
line('         0, SYSUTCDATETIME(), NULL);')
line("    PRINT N'Customers: 1 row inserted.';")
line('END')
line("ELSE PRINT N'Customers: already exists, skipped.';")

# ── 2. TRAP GROUPS ──
section('2. TRAP GROUPS  (20 rows)')
line("IF NOT EXISTS (SELECT 1 FROM [TrapGroups] WHERE [TrapGroup] = @TrapGroup AND [TrapNumber] = N'0')")
line('BEGIN')
line('    INSERT INTO [TrapGroups] ([Id],[TrapNumber],[TrapGroup],[Description],[CustomerId],[CreatedAt],[UpdatedAt]) VALUES')
rows = []
for i in range(20):
    disc = ' (غير متصل)' if i >= 18 else ''
    rows.append(f"    (NEWID(), N'{i}', @TrapGroup, N'متحسس مصيدة رقم {i} - المجموعة 1{disc}', @CustId, SYSUTCDATETIME(), NULL)")
line(',\n'.join(rows) + ';')
line("    PRINT N'TrapGroups: 20 rows inserted.';")
line('END')
line("ELSE PRINT N'TrapGroups: already exists, skipped.';")

# ── 3. TRAPS ──
section('3. TRAPS  (20 rows: 18 Active + 2 Inactive)')
line('-- Battery values verified against C# formula:')
line('--   battery = 100 - (TotalTransmissions*0.05) - (OperatingDays*1.85)')
line(f"IF NOT EXISTS (SELECT 1 FROM [Traps] WHERE [Id] = @T00)")
line('BEGIN')
line('    INSERT INTO [Traps]')
line('        ([Id],[TrapNumber],[TrapGroup],[SignalStrength],[status],[StartTime],')
line('         [BatteryPercentage],[IndicatorStatus],[LastEntryDate],')
line('         [TotalTransmissions],[OperatingDays],[Latitude],[Longitude],[CreatedAt],[UpdatedAt])')
line('    VALUES')
trap_rows = []
for (name,days,trans,batt,status,indicator,signal,lat_off,lon_off) in TRAP_SPECS:
    lat = round(BASE_LAT + lat_off, 15)
    lon = round(BASE_LON + lon_off, 15)
    num = name[1:].lstrip('0') or '0'
    last = LAST_ENTRY[name]
    verified = calc_battery(days, trans) if status == 'Active' else batt
    trap_rows.append(
        f'    (@{name},N\'{int(num)}\',@TrapGroup,{signal},N\'{status}\',' +
        f'DATEADD(day,-{days},SYSUTCDATETIME()),{verified},N\'{indicator}\',' +
        f'{last},{trans},{days},{lat},{lon},SYSUTCDATETIME(),NULL)'
    )
line(',\n'.join(trap_rows) + ';')
line("    PRINT N'Traps: 20 rows inserted (18 Active, 2 Inactive).';")
line('END')
line("ELSE PRINT N'Traps: already exists, skipped.';")

# ── 4. CAPTURE EVENTS ──
section('4. CAPTURE EVENTS  (42 rows)')
line('-- RodentType business rules:')
line('--   NormalRat    : length 7-10 cm,  weight 15-30 g')
line('--   ClimbingRat  : length 16-21 cm, weight 150-250 g')
line('--   NorwegianRat : length 18-26 cm, weight 200-500 g')
line('--   Unknown      : outside all above ranges')
line(f"IF NOT EXISTS (SELECT 1 FROM [CaptureEvents] WHERE [TrapId] = @T00)")
line('BEGIN')
line('    INSERT INTO [CaptureEvents]')
line('        ([Id],[TrapId],[Status],[CaptureTime],[ActiveSensorCount],')
line('         [RodentWeightGrams],[RodentLengthCm],[RodentType],')
line('         [Duration],[SignalStrength],[NumberOfTransmissions],[CreatedAt],[UpdatedAt])')
line('    VALUES')
ce_rows = []
for (trap,unit,offset,sensors,weight,length,rodent,dur,sig) in CAPTURE_EVENTS:
    ce_rows.append(
        f"    (NEWID(),@{trap},N'Active',DATEADD({unit},{offset:>4},SYSUTCDATETIME()),"
        f"{sensors},{weight},{length},N'{rodent}',{dur},{sig},1,SYSUTCDATETIME(),NULL)"
    )
line(',\n'.join(ce_rows) + ';')
line("    PRINT N'CaptureEvents: 42 rows inserted.';")
line('END')
line("ELSE PRINT N'CaptureEvents: already exists, skipped.';")

# ── 5. TRAP BAIT MEASUREMENTS ──
section(f'5. TRAP BAIT MEASUREMENTS  ({len(TBM_SPECS) * len(HOURS)} rows = 18 traps x {len(HOURS)} readings every 2h)')
line('-- T18 and T19 (Inactive/Disconnected) = NO rows intentionally')
line(f"IF NOT EXISTS (SELECT 1 FROM [TrapBaitMeasurements] WHERE [TrapId] = @T00)")
line('BEGIN')
for (trap, sig, bait_start) in TBM_SPECS:
    line(f'    -- {trap}')
    line(f'    INSERT INTO [TrapBaitMeasurements]([Id],[TrapId],[MeasurementTime],[BaitWeightGrams],[SignalStrength],[CreatedAt],[UpdatedAt]) VALUES')
    tbm_rows = []
    bait = bait_start
    for i, h in enumerate(HOURS):
        sv = sig + (0.2 if i%3==0 else -0.2 if i%3==1 else 0.0)
        tbm_rows.append(f'    (NEWID(),@{trap},DATEADD(hour,{h:>4},SYSUTCDATETIME()),{bait:.1f},{sv},SYSUTCDATETIME(),NULL)')
        bait = round(bait - 0.5, 1)
        if bait < 3.0:
            bait = bait_start
    line(',\n'.join(tbm_rows) + ';')
    line()
line(f"    PRINT N'TrapBaitMeasurements: {len(TBM_SPECS) * len(HOURS)} rows inserted.';")
line('END')
line("ELSE PRINT N'TrapBaitMeasurements: already exists, skipped.';")

# ── 6. BAIT MEASUREMENTS - Part A (Capture-linked) ──
section('6A. BAIT MEASUREMENTS - Capture-linked  (~42 rows)')
line('-- One BaitMeasurement per CaptureEvent')
line('-- BaitWeight reflects how much the rodent ate:')
line("--   NorwegianRat => 8g   (biggest eater)")
line("--   ClimbingRat  => 12g")
line("--   NormalRat    => 18g")
line("--   Unknown      => 25g")
line(f"IF NOT EXISTS (SELECT 1 FROM [BaitMeasurements]")
line(f"               WHERE [TrapId] = @T00 AND [CaptureEventId] IS NOT NULL)")
line('BEGIN')
line('    INSERT INTO [BaitMeasurements]')
line('        ([Id],[TrapId],[CaptureEventId],[MeasurementTime],[BaitWeightGrams],[CreatedAt],[UpdatedAt])')
line('    SELECT')
line('        NEWID(),')
line('        ce.[TrapId],')
line('        ce.[Id],')
line('        ce.[CaptureTime],')
line('        CASE')
line("            WHEN ce.[RodentType] = N'NorwegianRat' THEN  8.0")
line("            WHEN ce.[RodentType] = N'ClimbingRat'  THEN 12.0")
line("            WHEN ce.[RodentType] = N'NormalRat'    THEN 18.0")
line('            ELSE 25.0')
line('        END,')
line('        SYSUTCDATETIME(), NULL')
line('    FROM [CaptureEvents] ce')
line('    WHERE ce.[TrapId] IN (')
active_traps = [f'@{n}' for n in [f'T{i:02d}' for i in range(18)]]
line('        ' + ','.join(active_traps))
line('    );')
line("    PRINT N'BaitMeasurements (capture-linked): ' + CAST(@@ROWCOUNT AS NVARCHAR) + N' rows.';")
line('END')
line("ELSE PRINT N'BaitMeasurements (capture-linked): already exists, skipped.';")

# ── 6. BAIT MEASUREMENTS - Part B (Periodic) ──
section('6B. BAIT MEASUREMENTS - Periodic  (180 rows = 18 traps x 10 readings)')
line('-- Every 3 days over 30 days; bait decreases per trap activity level')
line(f"IF NOT EXISTS (SELECT 1 FROM [BaitMeasurements]")
line(f"               WHERE [TrapId] = @T00 AND [CaptureEventId] IS NULL)")
line('BEGIN')
line('    INSERT INTO [BaitMeasurements]')
line('        ([Id],[TrapId],[CaptureEventId],[MeasurementTime],[BaitWeightGrams],[CreatedAt],[UpdatedAt])')
line('    VALUES')
bm_rows = []
for (trap, bait_start, decay) in BM_PERIODIC:
    bait = bait_start
    for d in range(30, 0, -3):
        bait_val = round(max(bait, 3.0), 1)
        bm_rows.append(f'    (NEWID(),@{trap},NULL,DATEADD(day,-{d},SYSUTCDATETIME()),{bait_val},SYSUTCDATETIME(),NULL)')
        bait = round(bait - decay, 1)
        if bait < 5.0:
            bait = bait_start
line(',\n'.join(bm_rows) + ';')
line("    PRINT N'BaitMeasurements (periodic): ' + CAST(@@ROWCOUNT AS NVARCHAR) + N' rows.';")
line('END')
line("ELSE PRINT N'BaitMeasurements (periodic): already exists, skipped.';")

# ── COMMIT ──
line()
line('COMMIT TRANSACTION;')

# ── VERIFICATION ──
section('VERIFICATION SUMMARY')
line('SELECT')
line("    N'Customers'           AS [Table], COUNT(*) AS [Rows] FROM [Customers]")
line("UNION ALL SELECT N'TrapGroups',          COUNT(*) FROM [TrapGroups]")
line("UNION ALL SELECT N'Traps',               COUNT(*) FROM [Traps]")
line("UNION ALL SELECT N'CaptureEvents',        COUNT(*) FROM [CaptureEvents]")
line("UNION ALL SELECT N'TrapBaitMeasurements', COUNT(*) FROM [TrapBaitMeasurements]")
line("UNION ALL SELECT N'BaitMeasurements',     COUNT(*) FROM [BaitMeasurements];")
line()
line('-- Trap battery verification (stored vs formula):')
line('SELECT')
line("    CAST([TrapNumber] AS INT) AS [Trap],")
line("    [status]                  AS [Status],")
line("    [IndicatorStatus]         AS [Color],")
line("    [BatteryPercentage]       AS [Battery%],")
line("    [OperatingDays]           AS [Days],")
line("    [TotalTransmissions]      AS [Trans],")
line('    CAST(100.0')
line('         - ([TotalTransmissions]*0.05)')
line("         - ([OperatingDays]*1.85) AS DECIMAL(5,1)) AS [Formula%]")
line('FROM [Traps]')
line('ORDER BY CAST([TrapNumber] AS INT);')
line('GO')

content = '\n'.join(L)
p = pathlib.Path(r'd:/system/Infrastructure/Data/seeddata/MasterSeed_Complete.sql')
p.write_text(content, encoding='utf-8-sig')

# Stats
print(f'Written: {p.stat().st_size:,} bytes')
print(f'Lines  : {content.count(chr(10)):,}')
print()
print('Section row counts:')
print(f'  CaptureEvents rows     : {len(ce_rows)}')
print(f'  TrapBaitMeasurements   : {len(TBM_SPECS) * len(HOURS)}  ({len(TBM_SPECS)} traps x {len(HOURS)})')
print(f'  BaitMeasurements       : ~42 (capture) + {len(bm_rows)} (periodic) = ~{42+len(bm_rows)}')
print()
print('Battery verification (should match formula):')
print(f"{'Trap':<5} | {'Days':>4} | {'Trans':>5} | {'Stored%':>7} | {'Formula%':>8} | {'OK?'}")
print('-'*50)
all_ok = True
for (name,days,trans,batt,status,indicator,*_) in TRAP_SPECS:
    formula = calc_battery(days,trans)
    stored  = batt
    ok = 'OK' if (status=='Inactive' or formula==stored) else 'MISMATCH'
    if ok != 'OK': all_ok = False
    note = '(stored)' if status=='Inactive' else ''
    print(f'{name:<5} | {days:>4} | {trans:>5} | {stored:>6}%  | {formula:>7}%   | {ok} {note}')
print()
print('All OK:', all_ok)
