using System;
using System.Collections.Generic;
using System.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Data
{
    public class ServiceRequestRepository
    {
        public PagedResult<ServiceRequest> GetList(string search, int? statusId,
            DateTime? fromDate, DateTime? toDate, int pageIndex, int pageSize)
        {
            var totalParam = SqlHelper.Out("@TotalCount", SqlDbType.Int);

            var table = SqlHelper.ExecuteDataTable("dbo.usp_ServiceRequest_GetList",
                SqlHelper.In("@Search", string.IsNullOrWhiteSpace(search) ? null : search.Trim()),
                SqlHelper.In("@StatusId", statusId),
                SqlHelper.In("@FromDate", fromDate),
                SqlHelper.In("@ToDate", toDate),
                SqlHelper.In("@PageIndex", pageIndex),
                SqlHelper.In("@PageSize", pageSize),
                totalParam);

            var result = new PagedResult<ServiceRequest>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalParam.Value == null || totalParam.Value == DBNull.Value
                    ? 0 : (int)totalParam.Value
            };

            foreach (DataRow row in table.Rows)
                result.Items.Add(Map(row));

            return result;
        }

        public ServiceRequest GetById(int requestId)
        {
            var table = SqlHelper.ExecuteDataTable("dbo.usp_ServiceRequest_GetById",
                SqlHelper.In("@RequestId", requestId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public int Insert(ServiceRequest request)
        {
            var idParam  = SqlHelper.Out("@RequestId", SqlDbType.Int);
            var noParam  = SqlHelper.Out("@RequestNo", SqlDbType.VarChar, 20);

            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequest_Insert",
                SqlHelper.In("@VehicleId", request.VehicleId),
                SqlHelper.In("@TechnicianId", request.TechnicianId),
                SqlHelper.In("@RequestDate", request.RequestDate),
                SqlHelper.In("@Description", request.Description),
                SqlHelper.In("@LaborHours", request.LaborHours),
                SqlHelper.In("@DiscountAmount", request.DiscountAmount),
                idParam, noParam);

            request.RequestId = (int)idParam.Value;
            request.RequestNo = Convert.ToString(noParam.Value);
            return request.RequestId;
        }

        public void Update(ServiceRequest request)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequest_Update",
                SqlHelper.In("@RequestId", request.RequestId),
                SqlHelper.In("@VehicleId", request.VehicleId),
                SqlHelper.In("@TechnicianId", request.TechnicianId),
                SqlHelper.In("@RequestDate", request.RequestDate),
                SqlHelper.In("@Description", request.Description),
                SqlHelper.In("@LaborHours", request.LaborHours),
                SqlHelper.In("@DiscountAmount", request.DiscountAmount));
        }

        public void ChangeStatus(int requestId, int statusId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequest_ChangeStatus",
                SqlHelper.In("@RequestId", requestId),
                SqlHelper.In("@StatusId", statusId));
        }

        public void Delete(int requestId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequest_Delete",
                SqlHelper.In("@RequestId", requestId));
        }

        /* ---------------------------- detail lines --------------------------- */

        public List<ServiceRequestPart> GetParts(int requestId)
        {
            var list = new List<ServiceRequestPart>();

            var table = SqlHelper.ExecuteDataTable("dbo.usp_ServiceRequestPart_GetByRequest",
                SqlHelper.In("@RequestId", requestId));

            foreach (DataRow row in table.Rows)
            {
                list.Add(new ServiceRequestPart
                {
                    Id        = RowReader.GetInt(row, "Id"),
                    RequestId = RowReader.GetInt(row, "RequestId"),
                    PartId    = RowReader.GetInt(row, "PartId"),
                    PartCode  = RowReader.GetString(row, "PartCode"),
                    PartName  = RowReader.GetString(row, "PartName"),
                    Quantity  = RowReader.GetInt(row, "Quantity"),
                    UnitPrice = RowReader.GetDecimal(row, "UnitPrice"),
                    LineTotal = RowReader.GetDecimal(row, "LineTotal")
                });
            }

            return list;
        }

        public int AddPart(int requestId, int partId, int quantity)
        {
            var idParam = SqlHelper.Out("@Id", SqlDbType.Int);

            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequestPart_Add",
                SqlHelper.In("@RequestId", requestId),
                SqlHelper.In("@PartId", partId),
                SqlHelper.In("@Quantity", quantity),
                idParam);

            return (int)idParam.Value;
        }

        public void RemovePart(int lineId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_ServiceRequestPart_Delete",
                SqlHelper.In("@Id", lineId));
        }

        /* ------------------------------- lookup ------------------------------ */

        public List<ServiceStatus> GetStatuses()
        {
            var list = new List<ServiceStatus>();
            var table = SqlHelper.ExecuteDataTable("dbo.usp_ServiceStatus_GetAll");

            foreach (DataRow row in table.Rows)
            {
                list.Add(new ServiceStatus
                {
                    StatusId   = RowReader.GetInt(row, "StatusId"),
                    StatusName = RowReader.GetString(row, "StatusName"),
                    IsFinal    = RowReader.GetBool(row, "IsFinal")
                });
            }

            return list;
        }

        private static ServiceRequest Map(DataRow row)
        {
            return new ServiceRequest
            {
                RequestId      = RowReader.GetInt(row, "RequestId"),
                RequestNo      = RowReader.GetString(row, "RequestNo"),
                VehicleId      = RowReader.GetInt(row, "VehicleId"),
                PlateNumber    = RowReader.GetString(row, "PlateNumber"),
                Model          = RowReader.GetString(row, "Model"),
                CustomerId     = RowReader.GetInt(row, "CustomerId"),
                CustomerName   = RowReader.GetString(row, "CustomerName"),
                TechnicianId   = RowReader.GetNullableInt(row, "TechnicianId"),
                TechnicianName = RowReader.GetString(row, "TechnicianName"),
                StatusId       = RowReader.GetInt(row, "StatusId"),
                StatusName     = RowReader.GetString(row, "StatusName"),
                IsFinal        = RowReader.GetBool(row, "IsFinal"),
                RequestDate    = RowReader.GetDate(row, "RequestDate"),
                Description    = RowReader.GetString(row, "Description"),
                LaborHours     = RowReader.GetDecimal(row, "LaborHours"),
                LaborCost      = RowReader.GetDecimal(row, "LaborCost"),
                PartsCost      = RowReader.GetDecimal(row, "PartsCost"),
                DiscountAmount = RowReader.GetDecimal(row, "DiscountAmount"),
                TotalCost      = RowReader.GetDecimal(row, "TotalCost"),
                CompletedDate  = RowReader.GetNullableDate(row, "CompletedDate"),
                CreatedAt      = RowReader.GetDate(row, "CreatedAt")
            };
        }
    }
}
