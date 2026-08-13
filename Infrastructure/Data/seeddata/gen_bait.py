import pathlib

# ============================================================
# Generator: Seed_BaitMeasurements.sql
# جدول BaitMeasurements - محطات إعادة تعبئة الطعوم
# ============================================================

trap_vars = [
    ('@T00','A0000000-0000-0000-0000-000000000000', 50.0, 0.8, 'Green'),
    ('@T01','A0000000-0000-0000-0000-000000000001', 50.0, 0.7, 'Green'),
    ('@T02','A0000000-0000-0000-0000-000000000002', 50.0, 0.6, 'Green'),
    ('@T03','A0000000-0000-0000-0000-000000000003', 50.0, 0.5, 'Green'),
    ('@T04','A0000000-0000-0000-0000-000000000004', 50.0, 0.7, 'Green'),
    ('@T05','A0000000-0000-0000-0000-000000000005', 50.0, 0.6, 'Green'),
    ('@T06','A0000000-0000-0000-0000-000000000006', 45.0, 1.2, 'Yellow'),
    ('@T07','A0000000-0000-0000-0000-000000000007', 45.0, 1.1, 'Yellow'),
    ('@T08','A0000000-0000-0000-0000-000000000008', 45.0, 1.0, 'Yellow'),
    ('@T09','A0000000-0000-0000-0000-000000000009', 40.0, 1.5, 'Orange'),
    ('@T10','A0000000-0000-0000-0000-000000000010', 40.0, 1.4, 'Orange'),
    ('@T11','A0000000-0000-0000-0000-000000000011', 40.0, 1.3, 'Orange'),
    ('@T12','A0000000-0000-0000-0000-000000000012', 35.0, 2.0, 'Red'),
    ('@T13','A0000000-0000-0000-0000-000000000013', 35.0, 1.9, 'Red'),
    ('@T14','A0000000-0000-0000-0000-000000000014', 35.0, 1.8, 'Red'),
    ('@T15','A0000000-0000-0000-0000-000000000015', 30.0, 2.2, 'Red'),
    ('@T16','A0000000-0000-0000-0000-000000000016', 30.0, 2.1, 'Red'),
    ('@T17','A0000000-0000-0000-0000-000000000017', 30.0, 2.0, 'Red'),
]

lines = []
lines.append("-- ============================================================")
lines.append("-- SEED: BaitMeasurements  (محطات إعادة تعبئة الطعوم)")
lines.append("-- ============================================================")
lines.append("-- نوعان من القياسات:")
lines.append("--   A. مرتبط بحدث اصطياد (CaptureEventId != NULL) => 42 صف")
lines.append("--   B. دوري مستقل (CaptureEventId = NULL)          => 180 صف")
lines.append("-- المجموع المتوقع: ~222 صف (42 capture + 180 periodic)")
lines.append("-- T18 و T19 = غير متصلة => لا بيانات")
lines.append("-- ============================================================")
lines.append("")
lines.append("SET NOCOUNT ON;")
lines.append("BEGIN TRANSACTION;")
lines.append("")

# GUID declarations
for (var, guid, bstart, decay, color) in trap_vars:
    lines.append(f"DECLARE {var} UNIQUEIDENTIFIER = N'{guid}';")

lines.append("")
lines.append("-- ============================================================")
lines.append("-- PART A: قياسات مرتبطة بأحداث الاصطياد (CaptureEventId != NULL)")
lines.append("-- لكل CaptureEvent => قياس طعم بنفس الوقت")
lines.append("-- وزن الطعم يعكس ما أكله القارض:")
lines.append("--   NorwegianRat => أكبر => أكثر أكلاً => وزن طعم أقل")
lines.append("--   ClimbingRat  => متوسط")
lines.append("--   NormalRat    => أصغر => وزن طعم أعلى")
lines.append("--   Unknown      => غير محدد => وزن طعم كامل تقريباً")
lines.append("-- ============================================================")
lines.append("IF NOT EXISTS (SELECT 1 FROM [BaitMeasurements]")
lines.append("               WHERE [TrapId] = @T00 AND [CaptureEventId] IS NOT NULL)")
lines.append("BEGIN")
lines.append("    INSERT INTO [BaitMeasurements]")
lines.append("        ([Id],[TrapId],[CaptureEventId],[MeasurementTime],[BaitWeightGrams],[CreatedAt],[UpdatedAt])")
lines.append("    SELECT")
lines.append("        NEWID(),")
lines.append("        ce.[TrapId],")
lines.append("        ce.[Id],")
lines.append("        ce.[CaptureTime],")
lines.append("        CASE")
lines.append("            WHEN ce.[RodentType] = N'NorwegianRat' THEN 8.0")
lines.append("            WHEN ce.[RodentType] = N'ClimbingRat'  THEN 12.0")
lines.append("            WHEN ce.[RodentType] = N'NormalRat'    THEN 18.0")
lines.append("            ELSE 25.0")
lines.append("        END,")
lines.append("        SYSUTCDATETIME(),")
lines.append("        NULL")
lines.append("    FROM [CaptureEvents] ce")
lines.append("    WHERE ce.[TrapId] IN (")
trap_list = ','.join([v[0] for v in trap_vars])
lines.append(f"        {trap_list}")
lines.append("    );")
lines.append("    PRINT N'BaitMeasurements (Capture-linked): ' + CAST(@@ROWCOUNT AS NVARCHAR) + N' rows inserted.';")
lines.append("END")
lines.append("ELSE PRINT N'BaitMeasurements (Capture-linked): already exists, skipped.';")
lines.append("")
lines.append("-- ============================================================")
lines.append("-- PART B: قياسات دورية مستقلة (CaptureEventId = NULL)")
lines.append("-- كل 3 أيام لكل مصيدة => 10 قياسات x 18 مصيدة = 180 صف")
lines.append("-- الوزن يتناقص تدريجياً => إعادة تعبئة عند النفاد")
lines.append("-- ============================================================")
lines.append("IF NOT EXISTS (SELECT 1 FROM [BaitMeasurements]")
lines.append("               WHERE [TrapId] = @T00 AND [CaptureEventId] IS NULL)")
lines.append("BEGIN")
lines.append("    INSERT INTO [BaitMeasurements]")
lines.append("        ([Id],[TrapId],[CaptureEventId],[MeasurementTime],[BaitWeightGrams],[CreatedAt],[UpdatedAt])")
lines.append("    VALUES")

rows = []
for (var, guid, bait_start, decay, color) in trap_vars:
    bait = bait_start
    day_offsets = list(range(30, 0, -3))   # 30,27,24,21,18,15,12,9,6,3
    for d in day_offsets:
        bait_val = round(max(bait, 3.0), 1)
        rows.append(f"    (NEWID(),{var},NULL,DATEADD(day,-{d},SYSUTCDATETIME()),{bait_val},SYSUTCDATETIME(),NULL)")
        bait = round(bait - decay, 1)
        if bait < 5.0:
            bait = bait_start   # إعادة تعبئة

lines.append(',\n'.join(rows) + ';')
lines.append("    PRINT N'BaitMeasurements (Periodic): ' + CAST(@@ROWCOUNT AS NVARCHAR) + N' rows inserted.';")
lines.append("END")
lines.append("ELSE PRINT N'BaitMeasurements (Periodic): already exists, skipped.';")
lines.append("")
lines.append("COMMIT TRANSACTION;")
lines.append("")
lines.append("-- ============================================================")
lines.append("-- VERIFICATION: ملخص قياسات الطعم لكل مصيدة")
lines.append("-- ============================================================")
lines.append("SELECT")
lines.append("    CAST(t.[TrapNumber] AS INT)   AS [رقم_المصيدة],")
lines.append("    t.[TrapGroup]                  AS [المجموعة],")
lines.append("    t.[IndicatorStatus]            AS [الحالة_اللونية],")
lines.append("    t.[status]                     AS [حالة_الاتصال],")
lines.append("    COUNT(bm.[Id])                 AS [إجمالي_قياسات_الطعم],")
lines.append("    SUM(CASE WHEN bm.[CaptureEventId] IS NOT NULL THEN 1 ELSE 0 END) AS [مرتبط_باصطياد],")
lines.append("    SUM(CASE WHEN bm.[CaptureEventId] IS NULL     THEN 1 ELSE 0 END) AS [دوري_مستقل],")
lines.append("    MIN(bm.[BaitWeightGrams])      AS [أقل_وزن_g],")
lines.append("    MAX(bm.[BaitWeightGrams])      AS [أعلى_وزن_g],")
lines.append("    CAST(AVG(bm.[BaitWeightGrams]) AS DECIMAL(5,1)) AS [متوسط_وزن_g]")
lines.append("FROM [Traps] t")
lines.append("LEFT JOIN [BaitMeasurements] bm ON bm.[TrapId] = t.[Id]")
lines.append("GROUP BY t.[TrapNumber], t.[TrapGroup], t.[IndicatorStatus], t.[status]")
lines.append("ORDER BY CAST(t.[TrapNumber] AS INT);")
lines.append("GO")
lines.append("")
lines.append("-- ملخص إجمالي")
lines.append("SELECT")
lines.append("    N'BaitMeasurements TOTAL'  AS [الجدول],")
lines.append("    COUNT(*)                   AS [إجمالي_الصفوف],")
lines.append("    SUM(CASE WHEN [CaptureEventId] IS NOT NULL THEN 1 ELSE 0 END) AS [مرتبط_باصطياد],")
lines.append("    SUM(CASE WHEN [CaptureEventId] IS NULL     THEN 1 ELSE 0 END) AS [دوري_مستقل]")
lines.append("FROM [BaitMeasurements];")
lines.append("GO")

content = '\n'.join(lines)
p = pathlib.Path(r'd:/system/Infrastructure/Data/seeddata/Seed_BaitMeasurements.sql')
p.write_text(content, encoding='utf-8-sig')

periodic = content.count('NULL,DATEADD(day,')
print(f'Written OK: {p.stat().st_size} bytes, {content.count(chr(10))} lines')
print(f'Periodic rows in file: {periodic}')
print(f'Expected total rows: 42 (capture) + {periodic} (periodic) = {42+periodic}')
