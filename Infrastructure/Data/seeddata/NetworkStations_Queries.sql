-- ====================================================================================
-- 1. إضافة مجموعة المحطات رقم 1 مع 8 محطات (من 0 إلى 7)
-- ====================================================================================

-- 1.1 إضافة مجموعة المحطات رقم 1
INSERT INTO [TrapGroups] (
    [Id], 
    [TrapNumber],
    [TrapGroup], 
    [Description], 
    [CustomerId], 
    [CreatedAt], 
    [UpdatedAt]
)
VALUES (
    NEWID(), 
    N'0',
    N'1', 
    N'مجموعة المحطات رقم 1', 
    NULL, 
    SYSUTCDATETIME(), 
    NULL
);
GO

-- 1.2 إضافة 8 محطات (من 0 إلى 7) تابعة للمجموعة رقم 1
INSERT INTO [Traps] (
    [Id], 
    [TrapNumber], 
    [TrapGroup], 
    [SignalStrength], 
    [status], 
    [StartTime], 
    [BatteryPercentage], 
    [IndicatorStatus], 
    [LastEntryDate], 
    [TotalTransmissions], 
    [OperatingDays], 
    [Latitude], 
    [Longitude], 
    [CreatedAt], 
    [UpdatedAt]
)
VALUES 
(NEWID(), N'0', N'1', -65.5, N'Active', SYSUTCDATETIME(), 98, N'Green', SYSUTCDATETIME(), 15, 1, 24.7136, 46.6753, SYSUTCDATETIME(), NULL),
(NEWID(), N'1', N'1', -70.0, N'Active', SYSUTCDATETIME(), 95, N'Green', SYSUTCDATETIME(), 28, 3, 24.7140, 46.6758, SYSUTCDATETIME(), NULL),
(NEWID(), N'2', N'1', -68.2, N'Active', SYSUTCDATETIME(), 92, N'Green', SYSUTCDATETIME(), 42, 5, 24.7145, 46.6762, SYSUTCDATETIME(), NULL),
(NEWID(), N'3', N'1', -75.1, N'Active', SYSUTCDATETIME(), 88, N'Yellow', DATEADD(day, -5, SYSUTCDATETIME()), 65, 8, 24.7150, 46.6768, SYSUTCDATETIME(), NULL),
(NEWID(), N'4', N'1', -62.0, N'Active', SYSUTCDATETIME(), 100, N'Green', SYSUTCDATETIME(), 5, 1, 24.7155, 46.6772, SYSUTCDATETIME(), NULL),
(NEWID(), N'5', N'1', -80.4, N'Active', SYSUTCDATETIME(), 82, N'Orange', DATEADD(day, -6, SYSUTCDATETIME()), 90, 12, 24.7160, 46.6778, SYSUTCDATETIME(), NULL),
(NEWID(), N'6', N'1', -66.8, N'Active', SYSUTCDATETIME(), 91, N'Green', SYSUTCDATETIME(), 35, 4, 24.7165, 46.6782, SYSUTCDATETIME(), NULL),
(NEWID(), N'7', N'1', -85.0, N'Inactive', SYSUTCDATETIME(), 75, N'Red', DATEADD(day, -8, SYSUTCDATETIME()), 120, 15, 24.7170, 46.6788, SYSUTCDATETIME(), NULL);
GO


-- ====================================================================================
-- 2. استعلامات إجمالي محطات الشبكة (Total Network Stations)
-- ====================================================================================

-- 2.1 إجمالي عدد المحطات في الشبكة (النشطة وغير النشطة)
SELECT 
    COUNT(*) AS [إجمالي_محطات_الشبكة],
    SUM(CASE WHEN [status] = N'Active' THEN 1 ELSE 0 END) AS [المحطات_النشطة],
    SUM(CASE WHEN [status] != N'Active' THEN 1 ELSE 0 END) AS [المحطات_غير_النشطة]
FROM [Traps];
GO

-- 2.2 إجمالي المحطات موزعة حسب كل مجموعة (Trap Groups)
SELECT 
    [TrapGroup] AS [مجموعة_المحطة],
    COUNT(*) AS [إجمالي_المحطات],
    SUM(CASE WHEN [status] = N'Active' THEN 1 ELSE 0 END) AS [نشطة],
    SUM(CASE WHEN [status] != N'Active' THEN 1 ELSE 0 END) AS [غير_نشطة],
    AVG([BatteryPercentage]) AS [متوسط_البطارية],
    AVG([SignalStrength]) AS [متوسط_قوة_الإشارة]
FROM [Traps]
GROUP BY [TrapGroup]
ORDER BY [TrapGroup];
GO

-- 2.3 عرض جميع محطات الشبكة بالتفاصيل (مطابق لجدول الداشبورد)
SELECT 
    ROW_NUMBER() OVER (ORDER BY [TrapGroup], [TrapNumber]) AS [م],
    [TrapGroup] AS [مجموعة_المحطة],
    [TrapNumber] AS [رقم_المحطة],
    CASE 
        WHEN [status] = N'Active' THEN N'نشطة' 
        ELSE N'غير نشطة' 
    END AS [الحالة],
    CONCAT([BatteryPercentage], '%') AS [البطارية],
    [SignalStrength] AS [قوة_الإشارة],
    COALESCE(FORMAT([LastEntryDate], 'yyyy-MM-dd HH:mm'), N'-') AS [آخر_دخول],
    [IndicatorStatus] AS [مؤشر_الحالة]
FROM [Traps]
ORDER BY [TrapGroup], [TrapNumber];
GO
