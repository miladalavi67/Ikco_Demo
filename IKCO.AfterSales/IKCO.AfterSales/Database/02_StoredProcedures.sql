/* =====================================================================
   IKCO After-Sales Service Management - Stored Procedures
   All data access in the application goes through these procedures.
   ===================================================================== */

USE IKCO_AfterSales;
GO

/* ==================================================================== */
/*  CUSTOMERS                                                           */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_Customer_GetList','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_GetList;
GO
CREATE PROCEDURE dbo.usp_Customer_GetList
    @Search      NVARCHAR(100) = NULL,
    @OnlyActive  BIT           = 0,
    @PageIndex   INT           = 0,
    @PageSize    INT           = 20,
    @TotalCount  INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Customers c
    WHERE (@Search IS NULL OR c.FullName LIKE N'%' + @Search + N'%'
                           OR c.NationalCode LIKE @Search + '%'
                           OR c.Mobile LIKE @Search + '%')
      AND (@OnlyActive = 0 OR c.IsActive = 1);

    SELECT  c.CustomerId, c.FullName, c.NationalCode, c.Mobile, c.Email,
            c.City, c.Address, c.IsActive, c.CreatedAt,
            VehicleCount = (SELECT COUNT(*) FROM dbo.Vehicles v WHERE v.CustomerId = c.CustomerId)
    FROM dbo.Customers c
    WHERE (@Search IS NULL OR c.FullName LIKE N'%' + @Search + N'%'
                           OR c.NationalCode LIKE @Search + '%'
                           OR c.Mobile LIKE @Search + '%')
      AND (@OnlyActive = 0 OR c.IsActive = 1)
    ORDER BY c.FullName
    OFFSET (@PageIndex * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_Customer_GetById','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_GetById;
GO
CREATE PROCEDURE dbo.usp_Customer_GetById @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerId, FullName, NationalCode, Mobile, Email, City, Address, IsActive, CreatedAt
    FROM dbo.Customers WHERE CustomerId = @CustomerId;
END
GO

IF OBJECT_ID('dbo.usp_Customer_Insert','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_Insert;
GO
CREATE PROCEDURE dbo.usp_Customer_Insert
    @FullName NVARCHAR(100), @NationalCode VARCHAR(10), @Mobile VARCHAR(11),
    @Email NVARCHAR(100) = NULL, @City NVARCHAR(50) = NULL,
    @Address NVARCHAR(250) = NULL, @IsActive BIT = 1,
    @CustomerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Customers WHERE NationalCode = @NationalCode)
        THROW 50001, N'کد ملی تکراری است.', 1;

    INSERT INTO dbo.Customers (FullName, NationalCode, Mobile, Email, City, Address, IsActive)
    VALUES (@FullName, @NationalCode, @Mobile, @Email, @City, @Address, @IsActive);

    SET @CustomerId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.usp_Customer_Update','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_Update;
GO
CREATE PROCEDURE dbo.usp_Customer_Update
    @CustomerId INT, @FullName NVARCHAR(100), @NationalCode VARCHAR(10),
    @Mobile VARCHAR(11), @Email NVARCHAR(100) = NULL, @City NVARCHAR(50) = NULL,
    @Address NVARCHAR(250) = NULL, @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId)
        THROW 50002, N'مشتری یافت نشد.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Customers WHERE NationalCode = @NationalCode AND CustomerId <> @CustomerId)
        THROW 50001, N'کد ملی تکراری است.', 1;

    UPDATE dbo.Customers
       SET FullName = @FullName, NationalCode = @NationalCode, Mobile = @Mobile,
           Email = @Email, City = @City, Address = @Address, IsActive = @IsActive
     WHERE CustomerId = @CustomerId;
END
GO

IF OBJECT_ID('dbo.usp_Customer_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_Delete;
GO
CREATE PROCEDURE dbo.usp_Customer_Delete @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Vehicles WHERE CustomerId = @CustomerId)
        THROW 50003, N'برای این مشتری خودرو ثبت شده است و قابل حذف نیست.', 1;

    DELETE FROM dbo.Customers WHERE CustomerId = @CustomerId;
END
GO

IF OBJECT_ID('dbo.usp_Customer_GetCompletedRequestCount','P') IS NOT NULL DROP PROCEDURE dbo.usp_Customer_GetCompletedRequestCount;
GO
CREATE PROCEDURE dbo.usp_Customer_GetCompletedRequestCount @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)
    FROM dbo.ServiceRequests r
    INNER JOIN dbo.Vehicles v ON v.VehicleId = r.VehicleId
    WHERE v.CustomerId = @CustomerId AND r.StatusId = 5;
END
GO

/* ==================================================================== */
/*  VEHICLES                                                            */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_Vehicle_GetList','P') IS NOT NULL DROP PROCEDURE dbo.usp_Vehicle_GetList;
GO
CREATE PROCEDURE dbo.usp_Vehicle_GetList
    @Search NVARCHAR(100) = NULL, @CustomerId INT = NULL,
    @PageIndex INT = 0, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Vehicles v INNER JOIN dbo.Customers c ON c.CustomerId = v.CustomerId
    WHERE (@Search IS NULL OR v.PlateNumber LIKE N'%' + @Search + N'%'
                           OR v.Model LIKE N'%' + @Search + N'%'
                           OR c.FullName LIKE N'%' + @Search + N'%')
      AND (@CustomerId IS NULL OR v.CustomerId = @CustomerId);

    SELECT v.VehicleId, v.CustomerId, c.FullName AS CustomerName, v.PlateNumber,
           v.Model, v.ProductionYear, v.VIN, v.Mileage, v.CreatedAt
    FROM dbo.Vehicles v INNER JOIN dbo.Customers c ON c.CustomerId = v.CustomerId
    WHERE (@Search IS NULL OR v.PlateNumber LIKE N'%' + @Search + N'%'
                           OR v.Model LIKE N'%' + @Search + N'%'
                           OR c.FullName LIKE N'%' + @Search + N'%')
      AND (@CustomerId IS NULL OR v.CustomerId = @CustomerId)
    ORDER BY v.VehicleId DESC
    OFFSET (@PageIndex * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_Vehicle_GetById','P') IS NOT NULL DROP PROCEDURE dbo.usp_Vehicle_GetById;
GO
CREATE PROCEDURE dbo.usp_Vehicle_GetById @VehicleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.VehicleId, v.CustomerId, c.FullName AS CustomerName, v.PlateNumber,
           v.Model, v.ProductionYear, v.VIN, v.Mileage, v.CreatedAt
    FROM dbo.Vehicles v INNER JOIN dbo.Customers c ON c.CustomerId = v.CustomerId
    WHERE v.VehicleId = @VehicleId;
END
GO

IF OBJECT_ID('dbo.usp_Vehicle_Insert','P') IS NOT NULL DROP PROCEDURE dbo.usp_Vehicle_Insert;
GO
CREATE PROCEDURE dbo.usp_Vehicle_Insert
    @CustomerId INT, @PlateNumber NVARCHAR(20), @Model NVARCHAR(60),
    @ProductionYear INT, @VIN VARCHAR(17) = NULL, @Mileage INT = 0,
    @VehicleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Vehicles WHERE PlateNumber = @PlateNumber)
        THROW 50011, N'شماره پلاک تکراری است.', 1;

    INSERT INTO dbo.Vehicles (CustomerId, PlateNumber, Model, ProductionYear, VIN, Mileage)
    VALUES (@CustomerId, @PlateNumber, @Model, @ProductionYear, @VIN, @Mileage);

    SET @VehicleId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.usp_Vehicle_Update','P') IS NOT NULL DROP PROCEDURE dbo.usp_Vehicle_Update;
GO
CREATE PROCEDURE dbo.usp_Vehicle_Update
    @VehicleId INT, @CustomerId INT, @PlateNumber NVARCHAR(20), @Model NVARCHAR(60),
    @ProductionYear INT, @VIN VARCHAR(17) = NULL, @Mileage INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Vehicles WHERE PlateNumber = @PlateNumber AND VehicleId <> @VehicleId)
        THROW 50011, N'شماره پلاک تکراری است.', 1;

    UPDATE dbo.Vehicles
       SET CustomerId = @CustomerId, PlateNumber = @PlateNumber, Model = @Model,
           ProductionYear = @ProductionYear, VIN = @VIN, Mileage = @Mileage
     WHERE VehicleId = @VehicleId;
END
GO

IF OBJECT_ID('dbo.usp_Vehicle_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_Vehicle_Delete;
GO
CREATE PROCEDURE dbo.usp_Vehicle_Delete @VehicleId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceRequests WHERE VehicleId = @VehicleId)
        THROW 50012, N'برای این خودرو درخواست تعمیر ثبت شده است و قابل حذف نیست.', 1;

    DELETE FROM dbo.Vehicles WHERE VehicleId = @VehicleId;
END
GO

/* ==================================================================== */
/*  TECHNICIANS                                                         */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_Technician_GetList','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_GetList;
GO
CREATE PROCEDURE dbo.usp_Technician_GetList
    @Search NVARCHAR(100) = NULL, @OnlyActive BIT = 0,
    @PageIndex INT = 0, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Technicians t
    WHERE (@Search IS NULL OR t.FullName LIKE N'%' + @Search + N'%' OR t.PersonnelCode LIKE @Search + '%')
      AND (@OnlyActive = 0 OR t.IsActive = 1);

    SELECT t.TechnicianId, t.FullName, t.PersonnelCode, t.Specialty, t.HourlyRate, t.IsActive
    FROM dbo.Technicians t
    WHERE (@Search IS NULL OR t.FullName LIKE N'%' + @Search + N'%' OR t.PersonnelCode LIKE @Search + '%')
      AND (@OnlyActive = 0 OR t.IsActive = 1)
    ORDER BY t.FullName
    OFFSET (@PageIndex * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_Technician_GetById','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_GetById;
GO
CREATE PROCEDURE dbo.usp_Technician_GetById @TechnicianId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TechnicianId, FullName, PersonnelCode, Specialty, HourlyRate, IsActive
    FROM dbo.Technicians WHERE TechnicianId = @TechnicianId;
END
GO

IF OBJECT_ID('dbo.usp_Technician_GetActiveLookup','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_GetActiveLookup;
GO
CREATE PROCEDURE dbo.usp_Technician_GetActiveLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TechnicianId, FullName, PersonnelCode, Specialty, HourlyRate, IsActive
    FROM dbo.Technicians WHERE IsActive = 1 ORDER BY FullName;
END
GO

IF OBJECT_ID('dbo.usp_Technician_Insert','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_Insert;
GO
CREATE PROCEDURE dbo.usp_Technician_Insert
    @FullName NVARCHAR(100), @PersonnelCode VARCHAR(10), @Specialty NVARCHAR(60) = NULL,
    @HourlyRate DECIMAL(18,0) = 0, @IsActive BIT = 1, @TechnicianId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Technicians WHERE PersonnelCode = @PersonnelCode)
        THROW 50021, N'کد پرسنلی تکراری است.', 1;

    INSERT INTO dbo.Technicians (FullName, PersonnelCode, Specialty, HourlyRate, IsActive)
    VALUES (@FullName, @PersonnelCode, @Specialty, @HourlyRate, @IsActive);

    SET @TechnicianId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.usp_Technician_Update','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_Update;
GO
CREATE PROCEDURE dbo.usp_Technician_Update
    @TechnicianId INT, @FullName NVARCHAR(100), @PersonnelCode VARCHAR(10),
    @Specialty NVARCHAR(60) = NULL, @HourlyRate DECIMAL(18,0) = 0, @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Technicians WHERE PersonnelCode = @PersonnelCode AND TechnicianId <> @TechnicianId)
        THROW 50021, N'کد پرسنلی تکراری است.', 1;

    UPDATE dbo.Technicians
       SET FullName = @FullName, PersonnelCode = @PersonnelCode, Specialty = @Specialty,
           HourlyRate = @HourlyRate, IsActive = @IsActive
     WHERE TechnicianId = @TechnicianId;
END
GO

IF OBJECT_ID('dbo.usp_Technician_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_Technician_Delete;
GO
CREATE PROCEDURE dbo.usp_Technician_Delete @TechnicianId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceRequests WHERE TechnicianId = @TechnicianId)
        THROW 50022, N'این تعمیرکار به درخواست‌هایی تخصیص یافته است و قابل حذف نیست.', 1;

    DELETE FROM dbo.Technicians WHERE TechnicianId = @TechnicianId;
END
GO

/* ==================================================================== */
/*  PARTS                                                               */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_Part_GetList','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_GetList;
GO
CREATE PROCEDURE dbo.usp_Part_GetList
    @Search NVARCHAR(100) = NULL, @OnlyLowStock BIT = 0,
    @PageIndex INT = 0, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Parts p
    WHERE (@Search IS NULL OR p.PartName LIKE N'%' + @Search + N'%' OR p.PartCode LIKE @Search + '%')
      AND (@OnlyLowStock = 0 OR p.StockQty <= p.MinStockQty);

    SELECT p.PartId, p.PartCode, p.PartName, p.UnitPrice, p.StockQty, p.MinStockQty, p.IsActive
    FROM dbo.Parts p
    WHERE (@Search IS NULL OR p.PartName LIKE N'%' + @Search + N'%' OR p.PartCode LIKE @Search + '%')
      AND (@OnlyLowStock = 0 OR p.StockQty <= p.MinStockQty)
    ORDER BY p.PartName
    OFFSET (@PageIndex * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_Part_GetById','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_GetById;
GO
CREATE PROCEDURE dbo.usp_Part_GetById @PartId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartId, PartCode, PartName, UnitPrice, StockQty, MinStockQty, IsActive
    FROM dbo.Parts WHERE PartId = @PartId;
END
GO

IF OBJECT_ID('dbo.usp_Part_GetActiveLookup','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_GetActiveLookup;
GO
CREATE PROCEDURE dbo.usp_Part_GetActiveLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartId, PartCode, PartName, UnitPrice, StockQty, MinStockQty, IsActive
    FROM dbo.Parts WHERE IsActive = 1 ORDER BY PartName;
END
GO

IF OBJECT_ID('dbo.usp_Part_Insert','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_Insert;
GO
CREATE PROCEDURE dbo.usp_Part_Insert
    @PartCode VARCHAR(20), @PartName NVARCHAR(120), @UnitPrice DECIMAL(18,0),
    @StockQty INT = 0, @MinStockQty INT = 0, @IsActive BIT = 1, @PartId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Parts WHERE PartCode = @PartCode)
        THROW 50031, N'کد قطعه تکراری است.', 1;

    INSERT INTO dbo.Parts (PartCode, PartName, UnitPrice, StockQty, MinStockQty, IsActive)
    VALUES (@PartCode, @PartName, @UnitPrice, @StockQty, @MinStockQty, @IsActive);

    SET @PartId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.usp_Part_Update','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_Update;
GO
CREATE PROCEDURE dbo.usp_Part_Update
    @PartId INT, @PartCode VARCHAR(20), @PartName NVARCHAR(120), @UnitPrice DECIMAL(18,0),
    @StockQty INT = 0, @MinStockQty INT = 0, @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Parts WHERE PartCode = @PartCode AND PartId <> @PartId)
        THROW 50031, N'کد قطعه تکراری است.', 1;

    UPDATE dbo.Parts
       SET PartCode = @PartCode, PartName = @PartName, UnitPrice = @UnitPrice,
           StockQty = @StockQty, MinStockQty = @MinStockQty, IsActive = @IsActive
     WHERE PartId = @PartId;
END
GO

IF OBJECT_ID('dbo.usp_Part_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_Part_Delete;
GO
CREATE PROCEDURE dbo.usp_Part_Delete @PartId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceRequestParts WHERE PartId = @PartId)
        THROW 50032, N'این قطعه در درخواست‌ها استفاده شده است و قابل حذف نیست.', 1;

    DELETE FROM dbo.Parts WHERE PartId = @PartId;
END
GO

/* ==================================================================== */
/*  SERVICE STATUSES                                                    */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_ServiceStatus_GetAll','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceStatus_GetAll;
GO
CREATE PROCEDURE dbo.usp_ServiceStatus_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName, IsFinal FROM dbo.ServiceStatuses ORDER BY StatusId;
END
GO

/* ==================================================================== */
/*  SERVICE REQUESTS                                                    */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_ServiceRequest_Recalculate','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_Recalculate;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_Recalculate @RequestId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PartsCost DECIMAL(18,0) =
        ISNULL((SELECT SUM(LineTotal) FROM dbo.ServiceRequestParts WHERE RequestId = @RequestId), 0);

    DECLARE @LaborCost DECIMAL(18,0) =
        ISNULL((SELECT CAST(r.LaborHours * ISNULL(t.HourlyRate, 0) AS DECIMAL(18,0))
                FROM dbo.ServiceRequests r
                LEFT JOIN dbo.Technicians t ON t.TechnicianId = r.TechnicianId
                WHERE r.RequestId = @RequestId), 0);

    UPDATE dbo.ServiceRequests
       SET PartsCost = @PartsCost,
           LaborCost = @LaborCost,
           TotalCost = @PartsCost + @LaborCost - DiscountAmount
     WHERE RequestId = @RequestId;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_GetList','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_GetList;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_GetList
    @Search NVARCHAR(100) = NULL, @StatusId INT = NULL,
    @FromDate DATETIME = NULL, @ToDate DATETIME = NULL,
    @PageIndex INT = 0, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.ServiceRequests r
    INNER JOIN dbo.Vehicles  v ON v.VehicleId  = r.VehicleId
    INNER JOIN dbo.Customers c ON c.CustomerId = v.CustomerId
    WHERE (@Search   IS NULL OR r.RequestNo LIKE N'%' + @Search + N'%'
                             OR v.PlateNumber LIKE N'%' + @Search + N'%'
                             OR c.FullName LIKE N'%' + @Search + N'%')
      AND (@StatusId IS NULL OR r.StatusId = @StatusId)
      AND (@FromDate IS NULL OR r.RequestDate >= @FromDate)
      AND (@ToDate   IS NULL OR r.RequestDate <  DATEADD(DAY, 1, @ToDate));

    SELECT r.RequestId, r.RequestNo, r.VehicleId, v.PlateNumber, v.Model,
           c.CustomerId, c.FullName AS CustomerName,
           r.TechnicianId, t.FullName AS TechnicianName,
           r.StatusId, s.StatusName, s.IsFinal,
           r.RequestDate, r.Description, r.LaborHours,
           r.LaborCost, r.PartsCost, r.DiscountAmount, r.TotalCost,
           r.CompletedDate, r.CreatedAt
    FROM dbo.ServiceRequests r
    INNER JOIN dbo.Vehicles        v ON v.VehicleId  = r.VehicleId
    INNER JOIN dbo.Customers       c ON c.CustomerId = v.CustomerId
    INNER JOIN dbo.ServiceStatuses s ON s.StatusId   = r.StatusId
    LEFT  JOIN dbo.Technicians     t ON t.TechnicianId = r.TechnicianId
    WHERE (@Search   IS NULL OR r.RequestNo LIKE N'%' + @Search + N'%'
                             OR v.PlateNumber LIKE N'%' + @Search + N'%'
                             OR c.FullName LIKE N'%' + @Search + N'%')
      AND (@StatusId IS NULL OR r.StatusId = @StatusId)
      AND (@FromDate IS NULL OR r.RequestDate >= @FromDate)
      AND (@ToDate   IS NULL OR r.RequestDate <  DATEADD(DAY, 1, @ToDate))
    ORDER BY r.RequestDate DESC, r.RequestId DESC
    OFFSET (@PageIndex * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_GetById','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_GetById;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_GetById @RequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.RequestId, r.RequestNo, r.VehicleId, v.PlateNumber, v.Model,
           c.CustomerId, c.FullName AS CustomerName,
           r.TechnicianId, t.FullName AS TechnicianName,
           r.StatusId, s.StatusName, s.IsFinal,
           r.RequestDate, r.Description, r.LaborHours,
           r.LaborCost, r.PartsCost, r.DiscountAmount, r.TotalCost,
           r.CompletedDate, r.CreatedAt
    FROM dbo.ServiceRequests r
    INNER JOIN dbo.Vehicles        v ON v.VehicleId  = r.VehicleId
    INNER JOIN dbo.Customers       c ON c.CustomerId = v.CustomerId
    INNER JOIN dbo.ServiceStatuses s ON s.StatusId   = r.StatusId
    LEFT  JOIN dbo.Technicians     t ON t.TechnicianId = r.TechnicianId
    WHERE r.RequestId = @RequestId;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_Insert','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_Insert;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_Insert
    @VehicleId INT, @TechnicianId INT = NULL, @RequestDate DATETIME,
    @Description NVARCHAR(500) = NULL, @LaborHours DECIMAL(9,2) = 0,
    @DiscountAmount DECIMAL(18,0) = 0,
    @RequestId INT OUTPUT, @RequestNo VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

        DECLARE @Year INT = YEAR(@RequestDate);
        DECLARE @Seq  INT =
            ISNULL((SELECT MAX(CAST(RIGHT(RequestNo, 4) AS INT))
                    FROM dbo.ServiceRequests WITH (UPDLOCK, HOLDLOCK)
                    WHERE RequestNo LIKE 'SR-' + CAST(@Year AS VARCHAR(4)) + '-%'), 0) + 1;

        SET @RequestNo = 'SR-' + CAST(@Year AS VARCHAR(4)) + '-' + RIGHT('0000' + CAST(@Seq AS VARCHAR(4)), 4);

        INSERT INTO dbo.ServiceRequests
            (RequestNo, VehicleId, TechnicianId, StatusId, RequestDate, Description, LaborHours, DiscountAmount)
        VALUES
            (@RequestNo, @VehicleId, @TechnicianId, 1, @RequestDate, @Description, @LaborHours, @DiscountAmount);

        SET @RequestId = SCOPE_IDENTITY();

        EXEC dbo.usp_ServiceRequest_Recalculate @RequestId;

    COMMIT TRANSACTION;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_Update','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_Update;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_Update
    @RequestId INT, @VehicleId INT, @TechnicianId INT = NULL, @RequestDate DATETIME,
    @Description NVARCHAR(500) = NULL, @LaborHours DECIMAL(9,2) = 0,
    @DiscountAmount DECIMAL(18,0) = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceRequests r
               INNER JOIN dbo.ServiceStatuses s ON s.StatusId = r.StatusId
               WHERE r.RequestId = @RequestId AND s.IsFinal = 1)
        THROW 50041, N'درخواست در وضعیت نهایی است و قابل ویرایش نیست.', 1;

    UPDATE dbo.ServiceRequests
       SET VehicleId = @VehicleId, TechnicianId = @TechnicianId, RequestDate = @RequestDate,
           Description = @Description, LaborHours = @LaborHours, DiscountAmount = @DiscountAmount
     WHERE RequestId = @RequestId;

    EXEC dbo.usp_ServiceRequest_Recalculate @RequestId;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_ChangeStatus','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_ChangeStatus;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_ChangeStatus @RequestId INT, @StatusId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ServiceStatuses WHERE StatusId = @StatusId)
        THROW 50042, N'وضعیت نامعتبر است.', 1;

    UPDATE dbo.ServiceRequests
       SET StatusId = @StatusId,
           CompletedDate = CASE WHEN @StatusId = 5 THEN GETDATE() ELSE NULL END
     WHERE RequestId = @RequestId;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequest_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequest_Delete;
GO
CREATE PROCEDURE dbo.usp_ServiceRequest_Delete @RequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceRequests r
               INNER JOIN dbo.ServiceStatuses s ON s.StatusId = r.StatusId
               WHERE r.RequestId = @RequestId AND s.IsFinal = 1)
        THROW 50043, N'درخواست در وضعیت نهایی است و قابل حذف نیست.', 1;

    BEGIN TRANSACTION;

        /* returning reserved parts to stock */
        UPDATE p
           SET p.StockQty = p.StockQty + srp.Quantity
        FROM dbo.Parts p
        INNER JOIN dbo.ServiceRequestParts srp ON srp.PartId = p.PartId
        WHERE srp.RequestId = @RequestId;

        DELETE FROM dbo.ServiceRequestParts WHERE RequestId = @RequestId;
        DELETE FROM dbo.ServiceRequests     WHERE RequestId = @RequestId;

    COMMIT TRANSACTION;
END
GO

/* ==================================================================== */
/*  SERVICE REQUEST PARTS (detail lines)                                */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_ServiceRequestPart_GetByRequest','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequestPart_GetByRequest;
GO
CREATE PROCEDURE dbo.usp_ServiceRequestPart_GetByRequest @RequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT srp.Id, srp.RequestId, srp.PartId, p.PartCode, p.PartName,
           srp.Quantity, srp.UnitPrice, srp.LineTotal
    FROM dbo.ServiceRequestParts srp
    INNER JOIN dbo.Parts p ON p.PartId = srp.PartId
    WHERE srp.RequestId = @RequestId
    ORDER BY srp.Id;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequestPart_Add','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequestPart_Add;
GO
CREATE PROCEDURE dbo.usp_ServiceRequestPart_Add
    @RequestId INT, @PartId INT, @Quantity INT, @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Quantity <= 0
        THROW 50051, N'تعداد باید بزرگ‌تر از صفر باشد.', 1;

    BEGIN TRANSACTION;

        DECLARE @Stock INT, @Price DECIMAL(18,0);

        SELECT @Stock = StockQty, @Price = UnitPrice
        FROM dbo.Parts WITH (UPDLOCK, HOLDLOCK)
        WHERE PartId = @PartId;

        IF @Stock IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50052, N'قطعه یافت نشد.', 1;
        END

        IF @Stock < @Quantity
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50053, N'موجودی انبار کافی نیست.', 1;
        END

        UPDATE dbo.Parts SET StockQty = StockQty - @Quantity WHERE PartId = @PartId;

        INSERT INTO dbo.ServiceRequestParts (RequestId, PartId, Quantity, UnitPrice)
        VALUES (@RequestId, @PartId, @Quantity, @Price);

        SET @Id = SCOPE_IDENTITY();

        EXEC dbo.usp_ServiceRequest_Recalculate @RequestId;

    COMMIT TRANSACTION;
END
GO

IF OBJECT_ID('dbo.usp_ServiceRequestPart_Delete','P') IS NOT NULL DROP PROCEDURE dbo.usp_ServiceRequestPart_Delete;
GO
CREATE PROCEDURE dbo.usp_ServiceRequestPart_Delete @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

        DECLARE @RequestId INT, @PartId INT, @Quantity INT;

        SELECT @RequestId = RequestId, @PartId = PartId, @Quantity = Quantity
        FROM dbo.ServiceRequestParts WHERE Id = @Id;

        IF @RequestId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50054, N'ردیف قطعه یافت نشد.', 1;
        END

        UPDATE dbo.Parts SET StockQty = StockQty + @Quantity WHERE PartId = @PartId;
        DELETE FROM dbo.ServiceRequestParts WHERE Id = @Id;

        EXEC dbo.usp_ServiceRequest_Recalculate @RequestId;

    COMMIT TRANSACTION;
END
GO

/* ==================================================================== */
/*  REPORTS                                                             */
/* ==================================================================== */

IF OBJECT_ID('dbo.usp_Report_RevenueByMonth','P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_RevenueByMonth;
GO
CREATE PROCEDURE dbo.usp_Report_RevenueByMonth @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MonthNo    = MONTH(r.RequestDate),
           RequestCnt = COUNT(*),
           LaborSum   = SUM(r.LaborCost),
           PartsSum   = SUM(r.PartsCost),
           TotalSum   = SUM(r.TotalCost)
    FROM dbo.ServiceRequests r
    WHERE YEAR(r.RequestDate) = @Year AND r.StatusId = 5
    GROUP BY MONTH(r.RequestDate)
    ORDER BY MonthNo;
END
GO

IF OBJECT_ID('dbo.usp_Report_TechnicianPerformance','P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_TechnicianPerformance;
GO
CREATE PROCEDURE dbo.usp_Report_TechnicianPerformance @FromDate DATETIME = NULL, @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.TechnicianId, t.FullName, t.Specialty,
           RequestCnt = COUNT(r.RequestId),
           TotalHours = ISNULL(SUM(r.LaborHours), 0),
           TotalRevenue = ISNULL(SUM(r.TotalCost), 0)
    FROM dbo.Technicians t
    LEFT JOIN dbo.ServiceRequests r
           ON r.TechnicianId = t.TechnicianId
          AND (@FromDate IS NULL OR r.RequestDate >= @FromDate)
          AND (@ToDate   IS NULL OR r.RequestDate <  DATEADD(DAY, 1, @ToDate))
    GROUP BY t.TechnicianId, t.FullName, t.Specialty
    ORDER BY TotalRevenue DESC;
END
GO

IF OBJECT_ID('dbo.usp_Report_LowStockParts','P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_LowStockParts;
GO
CREATE PROCEDURE dbo.usp_Report_LowStockParts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartId, PartCode, PartName, UnitPrice, StockQty, MinStockQty, IsActive
    FROM dbo.Parts
    WHERE IsActive = 1 AND StockQty <= MinStockQty
    ORDER BY (StockQty - MinStockQty);
END
GO
