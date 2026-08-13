import pathlib, math

# Formula: battery = 100 - (trans*0.05) - (days*1.85)  clamped 0..100
def calc_battery(days, trans):
    exact = 100.0 - (trans * 0.05) - (days * 1.85)
    return max(0, min(100, round(exact)))

# Solve for exact days given target and ratio trans = 12*days
# 100 - 12d*0.05 - d*1.85 = target
# 100 - 0.6d - 1.85d = target => d = (100-target)/2.45
def solve_days(target):
    return max(1, math.ceil((100 - target) / 2.45))

# Build corrected spec - unique target per trap, formula verified
specs = [
    # (name, var, target_battery, color, status)
    ('T00','@T00', 90, 'Green',   'Active'),
    ('T01','@T01', 87, 'Green',   'Active'),
    ('T02','@T02', 85, 'Green',   'Active'),
    ('T03','@T03', 82, 'Green',   'Active'),
    ('T04','@T04', 80, 'Green',   'Active'),
    ('T05','@T05', 78, 'Green',   'Active'),
    ('T06','@T06', 61, 'Yellow',  'Active'),
    ('T07','@T07', 56, 'Yellow',  'Active'),
    ('T08','@T08', 51, 'Yellow',  'Active'),
    ('T09','@T09', 34, 'Orange',  'Active'),
    ('T10','@T10', 29, 'Orange',  'Active'),
    ('T11','@T11', 24, 'Orange',  'Active'),
    ('T12','@T12', 17, 'Red',     'Active'),
    ('T13','@T13', 14, 'Red',     'Active'),
    ('T14','@T14', 12, 'Red',     'Active'),
    ('T15','@T15',  9, 'Red',     'Active'),
    ('T16','@T16',  7, 'Red',     'Active'),
    ('T17','@T17',  4, 'Red',     'Active'),
    # Inactive => not recalculated by C# code (guard: status != Active)
    # Store directly as BatteryPercentage with days/trans that don't affect it
    ('T18','@T18', 15, 'Red',     'Inactive'),
    ('T19','@T19', 20, 'Red',     'Inactive'),
]

guids = {
    '@T00':'A0000000-0000-0000-0000-000000000000',
    '@T01':'A0000000-0000-0000-0000-000000000001',
    '@T02':'A0000000-0000-0000-0000-000000000002',
    '@T03':'A0000000-0000-0000-0000-000000000003',
    '@T04':'A0000000-0000-0000-0000-000000000004',
    '@T05':'A0000000-0000-0000-0000-000000000005',
    '@T06':'A0000000-0000-0000-0000-000000000006',
    '@T07':'A0000000-0000-0000-0000-000000000007',
    '@T08':'A0000000-0000-0000-0000-000000000008',
    '@T09':'A0000000-0000-0000-0000-000000000009',
    '@T10':'A0000000-0000-0000-0000-000000000010',
    '@T11':'A0000000-0000-0000-0000-000000000011',
    '@T12':'A0000000-0000-0000-0000-000000000012',
    '@T13':'A0000000-0000-0000-0000-000000000013',
    '@T14':'A0000000-0000-0000-0000-000000000014',
    '@T15':'A0000000-0000-0000-0000-000000000015',
    '@T16':'A0000000-0000-0000-0000-000000000016',
    '@T17':'A0000000-0000-0000-0000-000000000017',
    '@T18':'A0000000-0000-0000-0000-000000000018',
    '@T19':'A0000000-0000-0000-0000-000000000019',
}

# Calculate exact days/trans for each target then verify
corrected = []
print("Trap  | Color    | Target% | Days | Trans | Actual% | OK?")
print("-"*65)
all_ok = True
for (name, var, target, color, status) in specs:
    if status == 'Inactive':
        days  = 90 if name == 'T18' else 85
        trans = 200 if name == 'T18' else 180
        actual = target  # stored directly, not recalculated
        note  = "(stored)"
    else:
        days  = solve_days(target)
        trans = days * 12
        actual = calc_battery(days, trans)
        # Fine-tune if off by 1
        if actual != target:
            for d_adj in range(-3, 4):
                d2 = days + d_adj
                if d2 < 1: continue
                t2 = d2 * 12
                if calc_battery(d2, t2) == target:
                    days, trans = d2, t2
                    actual = target
                    break
        note = ""
    ok = "OK" if actual == target else "ADJ"
    if actual != target:
        all_ok = False
    print(f"{name:<5} | {color:<8} | {target:>6}%  | {days:>4} | {trans:>5} | {actual:>6}%  | {ok} {note}")
    corrected.append((name, var, days, trans, actual, status, color))

print()
print("All OK:", all_ok)

# Build SQL
lines = []
lines.append("-- ============================================================")
lines.append("-- Fix_BatteryPercentage.sql")
lines.append("-- Corrects BatteryPercentage, OperatingDays, TotalTransmissions")
lines.append("-- so stored value MATCHES the C# formula:")
lines.append("--   battery = 100 - (trans*0.05) - (days*1.85)")
lines.append("-- 11 traps were showing 0% -- all fixed to realistic values")
lines.append("-- Run AFTER FullSeed_AllTables.sql")
lines.append("-- ============================================================")
lines.append("")
lines.append("SET NOCOUNT ON;")
lines.append("BEGIN TRANSACTION;")
lines.append("")
for var, guid in guids.items():
    lines.append(f"DECLARE {var} UNIQUEIDENTIFIER = N'{guid}';")
lines.append("")
lines.append("-- ============================================================")
lines.append("-- UPDATE statements")
lines.append("-- ============================================================")

for (name, var, days, trans, batt, status, color) in corrected:
    lines.append("")
    lines.append(f"-- {name} | {color:<7} | {status} | Days={days} | Trans={trans} | Battery={batt}%")
    lines.append(f"UPDATE [Traps] SET")
    lines.append(f"    [BatteryPercentage]  = {batt},")
    lines.append(f"    [OperatingDays]      = {days},")
    lines.append(f"    [TotalTransmissions] = {trans},")
    lines.append(f"    [StartTime]          = DATEADD(day,-{days},SYSUTCDATETIME()),")
    lines.append(f"    [UpdatedAt]          = SYSUTCDATETIME()")
    lines.append(f"WHERE [Id] = {var};")

lines.append("")
lines.append("COMMIT TRANSACTION;")
lines.append("")
lines.append("-- ============================================================")
lines.append("-- VERIFICATION")
lines.append("-- ============================================================")
lines.append("SELECT")
lines.append("    CAST([TrapNumber] AS INT)  AS [Trap#],")
lines.append("    [status]                   AS [Status],")
lines.append("    [IndicatorStatus]          AS [Color],")
lines.append("    [BatteryPercentage]        AS [Stored_%],")
lines.append("    [OperatingDays]            AS [Days],")
lines.append("    [TotalTransmissions]       AS [Trans],")
lines.append("    CAST(100.0 - ([TotalTransmissions]*0.05)")
lines.append("              - ([OperatingDays]*1.85)")
lines.append("    AS DECIMAL(5,1))           AS [Formula_%],")
lines.append("    CASE")
lines.append("        WHEN [BatteryPercentage] >= 70 THEN N'High'")
lines.append("        WHEN [BatteryPercentage] >= 40 THEN N'Medium'")
lines.append("        WHEN [BatteryPercentage] >= 15 THEN N'Low'")
lines.append("        WHEN [BatteryPercentage] >  0  THEN N'Critical'")
lines.append("        ELSE                               N'DEAD (0%)'")
lines.append("    END                        AS [Level]")
lines.append("FROM [Traps]")
lines.append("ORDER BY CAST([TrapNumber] AS INT);")
lines.append("GO")

content = '\n'.join(lines)
p = pathlib.Path(r'd:/system/Infrastructure/Data/seeddata/Fix_BatteryPercentage.sql')
p.write_text(content, encoding='utf-8-sig')
print(f"\nSQL written: {p.stat().st_size} bytes, {content.count(chr(10))} lines")
