/* =====================================================================
   IKCO After-Sales Service Management - Seed Data
   ===================================================================== */

USE IKCO_AfterSales;
GO

DELETE FROM dbo.ServiceRequestParts;
DELETE FROM dbo.ServiceRequests;
DELETE FROM dbo.Vehicles;
DELETE FROM dbo.Customers;
DELETE FROM dbo.Parts;
DELETE FROM dbo.Technicians;
DELETE FROM dbo.ServiceStatuses;
GO

INSERT INTO dbo.ServiceStatuses (StatusId, StatusName, IsFinal) VALUES
    (1, N'ثبت شده',      0),
    (2, N'در حال بررسی', 0),
    (3, N'در حال تعمیر', 0),
    (4, N'منتظر قطعه',   0),
    (5, N'تکمیل شده',    1),
    (6, N'لغو شده',      1);
GO

INSERT INTO dbo.Technicians (FullName, PersonnelCode, Specialty, HourlyRate, IsActive) VALUES
    (N'رضا کاظمی',    '10021', N'موتور و گیربکس', 850000,  1),
    (N'مهدی صادقی',   '10034', N'برق و الکترونیک', 900000, 1),
    (N'حسین نوروزی',  '10047', N'جلوبندی و تعلیق', 780000, 1),
    (N'امیر رستمی',   '10055', N'تنظیم موتور',     820000, 1),
    (N'سعید بهرامی',  '10068', N'بدنه و رنگ',      700000, 0);
GO

INSERT INTO dbo.Parts (PartCode, PartName, UnitPrice, StockQty, MinStockQty, IsActive) VALUES
    ('P-1001', N'فیلتر روغن',              320000,   45, 10, 1),
    ('P-1002', N'فیلتر هوا',               280000,   38, 10, 1),
    ('P-1003', N'لنت ترمز جلو',           1450000,   22,  8, 1),
    ('P-1004', N'لنت ترمز عقب',           1180000,   18,  8, 1),
    ('P-1005', N'شمع موتور',               190000,  120, 30, 1),
    ('P-1006', N'تسمه تایم',              2200000,    9,  5, 1),
    ('P-1007', N'واتر پمپ',               3400000,    6,  5, 1),
    ('P-1008', N'دیسک کلاچ',              4800000,    4,  5, 1),
    ('P-1009', N'باتری ۶۶ آمپر',          9500000,   11,  4, 1),
    ('P-1010', N'کمک فنر جلو',            2650000,   14,  6, 1),
    ('P-1011', N'روغن موتور ۴ لیتری',     1650000,   60, 20, 1),
    ('P-1012', N'مایع خنک کننده رادیاتور', 420000,   25, 10, 1),
    ('P-1013', N'سنسور اکسیژن',           3100000,    3,  4, 1),
    ('P-1014', N'دسته موتور',             1750000,    8,  5, 1),
    ('P-1015', N'برف پاک کن (جفت)',        560000,   30, 10, 1);
GO

INSERT INTO dbo.Customers (FullName, NationalCode, Mobile, Email, City, Address, IsActive) VALUES
    (N'علی محمدی',      '0012345678', '09121234567', N'ali@example.com',    N'تهران', N'خیابان آزادی، پلاک ۱۲',   1),
    (N'زهرا احمدی',     '0023456789', '09122345678', N'zahra@example.com',  N'کرج',   N'بلوار طالقانی، پلاک ۴۵', 1),
    (N'محمد رضایی',     '0034567890', '09123456789', NULL,                  N'تهران', N'میدان ونک، پلاک ۷',      1),
    (N'فاطمه حسینی',    '0045678901', '09124567890', N'f.hoseini@example.com', N'اصفهان', N'خیابان چهارباغ',      1),
    (N'مرتضی کریمی',    '0056789012', '09125678901', NULL,                  N'مشهد',  N'بلوار وکیل آباد',        1),
    (N'سمیرا جعفری',    '0067890123', '09126789012', N'samira@example.com', N'شیراز', N'خیابان زند',             1),
    (N'ناصر قاسمی',     '0078901234', '09127890123', NULL,                  N'تبریز', N'خیابان امام',            0),
    (N'الهام موسوی',    '0089012345', '09128901234', NULL,                  N'تهران', N'سعادت آباد، بلوار فرحزادی', 1);
GO

INSERT INTO dbo.Vehicles (CustomerId, PlateNumber, Model, ProductionYear, VIN, Mileage)
SELECT c.CustomerId, v.PlateNumber, v.Model, v.ProductionYear, v.VIN, v.Mileage
FROM (VALUES
    (N'علی محمدی',   N'۱۲ ب ۳۴۵ ایران ۱۰',  N'سمند LX',      1398, 'IR1SM98001234567', 128000),
    (N'علی محمدی',   N'۵۶ د ۷۸۹ ایران ۲۲',  N'پژو ۲۰۶ تیپ ۵', 1400, 'IR1P206001234568',  62000),
    (N'زهرا احمدی',  N'۳۳ س ۴۴۴ ایران ۲۱',  N'دنا پلاس',      1401, 'IR1DN01001234569',  41000),
    (N'محمد رضایی',  N'۷۷ ل ۱۲۳ ایران ۱۱',  N'پژو پارس',      1397, 'IR1PRS001234570',  185000),
    (N'فاطمه حسینی', N'۹۸ ن ۶۵۴ ایران ۵۳',  N'رانا پلاس',     1402, 'IR1RN01001234571',  23000),
    (N'مرتضی کریمی', N'۴۱ ط ۲۲۲ ایران ۳۶',  N'تارا اتوماتیک', 1402, 'IR1TR01001234572',  19500),
    (N'سمیرا جعفری', N'۶۲ ق ۸۸۸ ایران ۶۳',  N'پژو ۲۰۷i',      1399, 'IR1P207001234573',  87000),
    (N'الهام موسوی', N'۱۵ ی ۳۳۳ ایران ۱۰',  N'سمند سورن پلاس', 1400, 'IR1SR01001234574',  54000)
) AS v(CustomerName, PlateNumber, Model, ProductionYear, VIN, Mileage)
INNER JOIN dbo.Customers c ON c.FullName = v.CustomerName;
GO

/* --- a few service requests with detail lines, via the procedures --- */
DECLARE @rid INT, @rno VARCHAR(20), @lineId INT, @vid INT, @tid INT;

SELECT TOP 1 @vid = VehicleId FROM dbo.Vehicles ORDER BY VehicleId;
SELECT TOP 1 @tid = TechnicianId FROM dbo.Technicians WHERE IsActive = 1 ORDER BY TechnicianId;

EXEC dbo.usp_ServiceRequest_Insert @vid, @tid, '2026-01-14', N'سرویس دوره‌ای ۱۲۰ هزار کیلومتر', 3.5, 0, @rid OUTPUT, @rno OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 1, 1, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 11, 1, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequest_ChangeStatus @rid, 5;

SELECT @vid = VehicleId FROM dbo.Vehicles WHERE PlateNumber = N'۳۳ س ۴۴۴ ایران ۲۱';
SELECT @tid = TechnicianId FROM dbo.Technicians WHERE PersonnelCode = '10034';

EXEC dbo.usp_ServiceRequest_Insert @vid, @tid, '2026-02-03', N'ایراد در سیستم برق و روشنایی', 2.0, 0, @rid OUTPUT, @rno OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 5, 4, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequest_ChangeStatus @rid, 3;

SELECT @vid = VehicleId FROM dbo.Vehicles WHERE PlateNumber = N'۷۷ ل ۱۲۳ ایران ۱۱';
SELECT @tid = TechnicianId FROM dbo.Technicians WHERE PersonnelCode = '10047';

EXEC dbo.usp_ServiceRequest_Insert @vid, @tid, '2026-02-20', N'صدای غیرعادی از جلوبندی', 4.0, 500000, @rid OUTPUT, @rno OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 10, 2, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 3, 1, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequest_ChangeStatus @rid, 5;

SELECT @vid = VehicleId FROM dbo.Vehicles WHERE PlateNumber = N'۶۲ ق ۸۸۸ ایران ۶۳';
SELECT @tid = TechnicianId FROM dbo.Technicians WHERE PersonnelCode = '10021';

EXEC dbo.usp_ServiceRequest_Insert @vid, @tid, '2026-03-11', N'تعویض تسمه تایم و واتر پمپ', 5.5, 0, @rid OUTPUT, @rno OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 6, 1, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequestPart_Add @rid, 7, 1, @lineId OUTPUT;
EXEC dbo.usp_ServiceRequest_ChangeStatus @rid, 4;

SELECT @vid = VehicleId FROM dbo.Vehicles WHERE PlateNumber = N'۱۵ ی ۳۳۳ ایران ۱۰';
EXEC dbo.usp_ServiceRequest_Insert @vid, NULL, '2026-04-02', N'بررسی اولیه - مشتری منتظر برآورد هزینه', 0, 0, @rid OUTPUT, @rno OUTPUT;
GO

PRINT N'Seed data inserted.';
GO
