using System;
using System.Data;

namespace IKCO.AfterSales.WinForms.Data
{
    /// <summary>
    /// Reports are bound straight to the grid, so they return DataTable.
    /// </summary>
    public class ReportRepository
    {
        public DataTable GetRevenueByMonth(int year)
        {
            return SqlHelper.ExecuteDataTable("dbo.usp_Report_RevenueByMonth",
                SqlHelper.In("@Year", year));
        }

        public DataTable GetTechnicianPerformance(DateTime? fromDate, DateTime? toDate)
        {
            return SqlHelper.ExecuteDataTable("dbo.usp_Report_TechnicianPerformance",
                SqlHelper.In("@FromDate", fromDate),
                SqlHelper.In("@ToDate", toDate));
        }

        public DataTable GetLowStockParts()
        {
            return SqlHelper.ExecuteDataTable("dbo.usp_Report_LowStockParts");
        }
    }
}
