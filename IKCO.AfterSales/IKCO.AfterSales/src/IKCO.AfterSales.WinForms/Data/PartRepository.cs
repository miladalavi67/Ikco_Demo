using System.Collections.Generic;
using System.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Data
{
    public class PartRepository
    {
        public PagedResult<Part> GetList(string search, bool onlyLowStock, int pageIndex, int pageSize)
        {
            var totalParam = SqlHelper.Out("@TotalCount", SqlDbType.Int);

            var table = SqlHelper.ExecuteDataTable("dbo.usp_Part_GetList",
                SqlHelper.In("@Search", string.IsNullOrWhiteSpace(search) ? null : search.Trim()),
                SqlHelper.In("@OnlyLowStock", onlyLowStock),
                SqlHelper.In("@PageIndex", pageIndex),
                SqlHelper.In("@PageSize", pageSize),
                totalParam);

            var result = new PagedResult<Part>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalParam.Value == null || totalParam.Value == System.DBNull.Value
                    ? 0 : (int)totalParam.Value
            };

            foreach (DataRow row in table.Rows)
                result.Items.Add(Map(row));

            return result;
        }

        public List<Part> GetActiveLookup()
        {
            var list = new List<Part>();
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Part_GetActiveLookup");

            foreach (DataRow row in table.Rows)
                list.Add(Map(row));

            return list;
        }

        public Part GetById(int partId)
        {
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Part_GetById",
                SqlHelper.In("@PartId", partId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public int Insert(Part part)
        {
            var idParam = SqlHelper.Out("@PartId", SqlDbType.Int);

            SqlHelper.ExecuteNonQuery("dbo.usp_Part_Insert",
                SqlHelper.In("@PartCode", part.PartCode),
                SqlHelper.In("@PartName", part.PartName),
                SqlHelper.In("@UnitPrice", part.UnitPrice),
                SqlHelper.In("@StockQty", part.StockQty),
                SqlHelper.In("@MinStockQty", part.MinStockQty),
                SqlHelper.In("@IsActive", part.IsActive),
                idParam);

            part.PartId = (int)idParam.Value;
            return part.PartId;
        }

        public void Update(Part part)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Part_Update",
                SqlHelper.In("@PartId", part.PartId),
                SqlHelper.In("@PartCode", part.PartCode),
                SqlHelper.In("@PartName", part.PartName),
                SqlHelper.In("@UnitPrice", part.UnitPrice),
                SqlHelper.In("@StockQty", part.StockQty),
                SqlHelper.In("@MinStockQty", part.MinStockQty),
                SqlHelper.In("@IsActive", part.IsActive));
        }

        public void Delete(int partId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Part_Delete",
                SqlHelper.In("@PartId", partId));
        }

        private static Part Map(DataRow row)
        {
            return new Part
            {
                PartId      = RowReader.GetInt(row, "PartId"),
                PartCode    = RowReader.GetString(row, "PartCode"),
                PartName    = RowReader.GetString(row, "PartName"),
                UnitPrice   = RowReader.GetDecimal(row, "UnitPrice"),
                StockQty    = RowReader.GetInt(row, "StockQty"),
                MinStockQty = RowReader.GetInt(row, "MinStockQty"),
                IsActive    = RowReader.GetBool(row, "IsActive")
            };
        }
    }
}
