/* =====================================================================
   IKCO After-Sales Service Management - Schema
   Target: SQL Server 2016+
   ===================================================================== */

IF DB_ID('IKCO_AfterSales') IS NULL
    CREATE DATABASE IKCO_AfterSales;
GO

USE IKCO_AfterSales;
GO

/* ---------------------------------------------------------------- */
IF OBJECT_ID('dbo.ServiceRequestParts','U') IS NOT NULL DROP TABLE dbo.ServiceRequestParts;
IF OBJECT_ID('dbo.ServiceRequests','U')     IS NOT NULL DROP TABLE dbo.ServiceRequests;
IF OBJECT_ID('dbo.Vehicles','U')            IS NOT NULL DROP TABLE dbo.Vehicles;
IF OBJECT_ID('dbo.Customers','U')           IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Parts','U')               IS NOT NULL DROP TABLE dbo.Parts;
IF OBJECT_ID('dbo.Technicians','U')         IS NOT NULL DROP TABLE dbo.Technicians;
IF OBJECT_ID('dbo.ServiceStatuses','U')     IS NOT NULL DROP TABLE dbo.ServiceStatuses;
GO

/* ------------------------------ Lookup ------------------------------ */
CREATE TABLE dbo.ServiceStatuses
(
    StatusId    INT           NOT NULL PRIMARY KEY,
    StatusName  NVARCHAR(50)  NOT NULL,
    IsFinal     BIT           NOT NULL DEFAULT(0)
);
GO

/* ----------------------------- Customers ---------------------------- */
CREATE TABLE dbo.Customers
(
    CustomerId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName      NVARCHAR(100)  NOT NULL,
    NationalCode  VARCHAR(10)    NOT NULL,
    Mobile        VARCHAR(11)    NOT NULL,
    Email         NVARCHAR(100)  NULL,
    City          NVARCHAR(50)   NULL,
    Address       NVARCHAR(250)  NULL,
    IsActive      BIT            NOT NULL DEFAULT(1),
    CreatedAt     DATETIME       NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT UQ_Customers_NationalCode UNIQUE (NationalCode)
);
GO
CREATE INDEX IX_Customers_FullName ON dbo.Customers(FullName);
GO

/* ------------------------------ Vehicles ---------------------------- */
CREATE TABLE dbo.Vehicles
(
    VehicleId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerId      INT            NOT NULL,
    PlateNumber     NVARCHAR(20)   NOT NULL,
    Model           NVARCHAR(60)   NOT NULL,
    ProductionYear  INT            NOT NULL,
    VIN             VARCHAR(17)    NULL,
    Mileage         INT            NOT NULL DEFAULT(0),
    CreatedAt       DATETIME       NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT FK_Vehicles_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT UQ_Vehicles_Plate UNIQUE (PlateNumber)
);
GO
CREATE INDEX IX_Vehicles_CustomerId ON dbo.Vehicles(CustomerId);
GO

/* ---------------------------- Technicians --------------------------- */
CREATE TABLE dbo.Technicians
(
    TechnicianId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName       NVARCHAR(100)  NOT NULL,
    PersonnelCode  VARCHAR(10)    NOT NULL,
    Specialty      NVARCHAR(60)   NULL,
    HourlyRate     DECIMAL(18,0)  NOT NULL DEFAULT(0),
    IsActive       BIT            NOT NULL DEFAULT(1),
    CONSTRAINT UQ_Technicians_PersonnelCode UNIQUE (PersonnelCode)
);
GO

/* ------------------------------- Parts ------------------------------ */
CREATE TABLE dbo.Parts
(
    PartId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PartCode     VARCHAR(20)    NOT NULL,
    PartName     NVARCHAR(120)  NOT NULL,
    UnitPrice    DECIMAL(18,0)  NOT NULL DEFAULT(0),
    StockQty     INT            NOT NULL DEFAULT(0),
    MinStockQty  INT            NOT NULL DEFAULT(0),
    IsActive     BIT            NOT NULL DEFAULT(1),
    CONSTRAINT UQ_Parts_PartCode UNIQUE (PartCode)
);
GO

/* -------------------------- ServiceRequests ------------------------- */
CREATE TABLE dbo.ServiceRequests
(
    RequestId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RequestNo      VARCHAR(20)    NOT NULL,
    VehicleId      INT            NOT NULL,
    TechnicianId   INT            NULL,
    StatusId       INT            NOT NULL,
    RequestDate    DATETIME       NOT NULL DEFAULT(GETDATE()),
    Description    NVARCHAR(500)  NULL,
    LaborHours     DECIMAL(9,2)   NOT NULL DEFAULT(0),
    LaborCost      DECIMAL(18,0)  NOT NULL DEFAULT(0),
    PartsCost      DECIMAL(18,0)  NOT NULL DEFAULT(0),
    DiscountAmount DECIMAL(18,0)  NOT NULL DEFAULT(0),
    TotalCost      DECIMAL(18,0)  NOT NULL DEFAULT(0),
    CompletedDate  DATETIME       NULL,
    CreatedAt      DATETIME       NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT FK_SR_Vehicles    FOREIGN KEY (VehicleId)    REFERENCES dbo.Vehicles(VehicleId),
    CONSTRAINT FK_SR_Technicians FOREIGN KEY (TechnicianId) REFERENCES dbo.Technicians(TechnicianId),
    CONSTRAINT FK_SR_Statuses    FOREIGN KEY (StatusId)     REFERENCES dbo.ServiceStatuses(StatusId),
    CONSTRAINT UQ_SR_RequestNo   UNIQUE (RequestNo)
);
GO
CREATE INDEX IX_SR_VehicleId ON dbo.ServiceRequests(VehicleId);
CREATE INDEX IX_SR_StatusId  ON dbo.ServiceRequests(StatusId);
CREATE INDEX IX_SR_Date      ON dbo.ServiceRequests(RequestDate);
GO

/* ------------------------ ServiceRequestParts ----------------------- */
CREATE TABLE dbo.ServiceRequestParts
(
    Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RequestId  INT            NOT NULL,
    PartId     INT            NOT NULL,
    Quantity   INT            NOT NULL,
    UnitPrice  DECIMAL(18,0)  NOT NULL,
    LineTotal  AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT FK_SRP_Requests FOREIGN KEY (RequestId) REFERENCES dbo.ServiceRequests(RequestId),
    CONSTRAINT FK_SRP_Parts    FOREIGN KEY (PartId)    REFERENCES dbo.Parts(PartId),
    CONSTRAINT CK_SRP_Quantity CHECK (Quantity > 0)
);
GO
CREATE INDEX IX_SRP_RequestId ON dbo.ServiceRequestParts(RequestId);
GO
