-- ====================================================================================
-- إيضاح: سكريبت T-SQL لإضافة بيانات مجموعات المحطات (TrapGroups) 
-- للعميل (CustomerId = '12ee192d-51e1-4960-8661-ae84d2cbcdd7')
-- وأرقام المحطات (TrapNumber) من 0 إلى 7
-- ====================================================================================

DECLARE @CustomerId UNIQUEIDENTIFIER = '12ee192d-51e1-4960-8661-ae84d2cbcdd7';
DECLARE @TrapGroup NVARCHAR(50) = N'1'; -- رقم أو اسم المجموعة (يمكن تعديله حسب رغبتك)

-- 1. إضافة 8 سجلات في جدول TrapGroups (من رقم 0 إلى 7) مرتبطة بالعميل
INSERT INTO [TrapGroups] (
    [Id], 
    [TrapNumber], 
    [TrapGroup], 
    [Description], 
    [CustomerId], 
    [CreatedAt], 
    [UpdatedAt]
)
VALUES 
(NEWID(), N'0', @TrapGroup, N'متحسس مصيدة رقم 0 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'1', @TrapGroup, N'متحسس مصيدة رقم 1 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'2', @TrapGroup, N'متحسس مصيدة رقم 2 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'3', @TrapGroup, N'متحسس مصيدة رقم 3 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'4', @TrapGroup, N'متحسس مصيدة رقم 4 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'5', @TrapGroup, N'متحسس مصيدة رقم 5 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'6', @TrapGroup, N'متحسس مصيدة رقم 6 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL),
(NEWID(), N'7', @TrapGroup, N'متحسس مصيدة رقم 7 - المجموعة ' + @TrapGroup, @CustomerId, SYSUTCDATETIME(), NULL);
GO

-- ====================================================================================
-- 2. (اختياري) إضافة المحطات الفعلية في جدول Traps المقابلة لهذه المجموعات والأرقام
-- ====================================================================================
DECLARE @TrapGroup NVARCHAR(50) = N'1';

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
(NEWID(), N'0', @TrapGroup, -65.0, N'Active', SYSUTCDATETIME(), 98, N'Green', SYSUTCDATETIME(), 10, 1, 24.7136, 46.6753, SYSUTCDATETIME(), NULL),
(NEWID(), N'1', @TrapGroup, -68.5, N'Active', SYSUTCDATETIME(), 95, N'Green', SYSUTCDATETIME(), 20, 2, 24.7140, 46.6758, SYSUTCDATETIME(), NULL),
(NEWID(), N'2', @TrapGroup, -70.2, N'Active', SYSUTCDATETIME(), 92, N'Green', SYSUTCDATETIME(), 30, 3, 24.7145, 46.6762, SYSUTCDATETIME(), NULL),
(NEWID(), N'3', @TrapGroup, -75.0, N'Active', SYSUTCDATETIME(), 88, N'Yellow', DATEADD(day, -3, SYSUTCDATETIME()), 45, 5, 24.7150, 46.6768, SYSUTCDATETIME(), NULL),
(NEWID(), N'4', @TrapGroup, -62.0, N'Active', SYSUTCDATETIME(), 100, N'Green', SYSUTCDATETIME(), 5, 1, 24.7155, 46.6772, SYSUTCDATETIME(), NULL),
(NEWID(), N'5', @TrapGroup, -80.0, N'Active', SYSUTCDATETIME(), 82, N'Orange', DATEADD(day, -5, SYSUTCDATETIME()), 60, 7, 24.7160, 46.6778, SYSUTCDATETIME(), NULL),
(NEWID(), N'6', @TrapGroup, -66.5, N'Active', SYSUTCDATETIME(), 91, N'Green', SYSUTCDATETIME(), 25, 3, 24.7165, 46.6782, SYSUTCDATETIME(), NULL),
(NEWID(), N'7', @TrapGroup, -85.0, N'Inactive', SYSUTCDATETIME(), 75, N'Red', DATEADD(day, -7, SYSUTCDATETIME()), 90, 10, 24.7170, 46.6788, SYSUTCDATETIME(), NULL);
GO

-- ====================================================================================
-- 3. استعلام للتأكد من ربط العميل بالمحطات ومجموعاتها بنجاح
-- ====================================================================================
SELECT 
    c.[CustomerNumber] AS [رقم_العميل],
    c.[Name] AS [اسم_العميل],
    tg.[TrapGroup] AS [مجموعة_المحطة],
    tg.[TrapNumber] AS [رقم_المحطة],
    tg.[Description] AS [وصف_المحطة],
    t.[status] AS [حالة_المحطة_الفعلية],
    t.[BatteryPercentage] AS [البطارية]
FROM [TrapGroups] tg
INNER JOIN [Customers] c ON tg.[CustomerId] = c.[Id]
LEFT JOIN [Traps] t ON tg.[TrapGroup] = t.[TrapGroup] AND tg.[TrapNumber] = t.[TrapNumber]
WHERE tg.[CustomerId] = '12ee192d-51e1-4960-8661-ae84d2cbcdd7'
ORDER BY tg.[TrapGroup], tg.[TrapNumber];
GO
