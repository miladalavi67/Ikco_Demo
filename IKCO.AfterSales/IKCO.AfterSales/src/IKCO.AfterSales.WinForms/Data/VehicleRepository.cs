using System.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Data
{
    public class VehicleRepository
    {
        public PagedResult<Vehicle> GetList(string search, int? customerId, int pageIndex, int pageSize)
        {
            var totalParam = SqlHelper.Out("@TotalCount", SqlDbType.Int);

            var table = SqlHelper.ExecuteDataTable("dbo.usp_Vehicle_GetList",
                SqlHelper.In("@Search", string.IsNullOrWhiteSpace(search) ? null : search.Trim()),
                SqlHelper.In("@CustomerId", customerId),
                SqlHelper.In("@PageIndex", pageIndex),
                SqlHelper.In("@PageSize", pageSize),
                totalParam);

            var result = new PagedResult<Vehicle>
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

        public Vehicle GetById(int vehicleId)
        {
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Vehicle_GetById",
                SqlHelper.In("@VehicleId", vehicleId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public int Insert(Vehicle vehicle)
        {
            var idParam = SqlHelper.Out("@VehicleId", SqlDbType.Int);

            SqlHelper.ExecuteNonQuery("dbo.usp_Vehicle_Insert",
                SqlHelper.In("@CustomerId", vehicle.CustomerId),
                SqlHelper.In("@PlateNumber", vehicle.PlateNumber),
                SqlHelper.In("@Model", vehicle.Model),
                SqlHelper.In("@ProductionYear", vehicle.ProductionYear),
                SqlHelper.In("@VIN", vehicle.VIN),
                SqlHelper.In("@Mileage", vehicle.Mileage),
                idParam);

            vehicle.VehicleId = (int)idParam.Value;
            return vehicle.VehicleId;
        }

        public void Update(Vehicle vehicle)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Vehicle_Update",
                SqlHelper.In("@VehicleId", vehicle.VehicleId),
                SqlHelper.In("@CustomerId", vehicle.CustomerId),
                SqlHelper.In("@PlateNumber", vehicle.PlateNumber),
                SqlHelper.In("@Model", vehicle.Model),
                SqlHelper.In("@ProductionYear", vehicle.ProductionYear),
                SqlHelper.In("@VIN", vehicle.VIN),
                SqlHelper.In("@Mileage", vehicle.Mileage));
        }

        public void Delete(int vehicleId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Vehicle_Delete",
                SqlHelper.In("@VehicleId", vehicleId));
        }

        private static Vehicle Map(DataRow row)
        {
            return new Vehicle
            {
                VehicleId      = RowReader.GetInt(row, "VehicleId"),
                CustomerId     = RowReader.GetInt(row, "CustomerId"),
                CustomerName   = RowReader.GetString(row, "CustomerName"),
                PlateNumber    = RowReader.GetString(row, "PlateNumber"),
                Model          = RowReader.GetString(row, "Model"),
                ProductionYear = RowReader.GetInt(row, "ProductionYear"),
                VIN            = RowReader.GetString(row, "VIN"),
                Mileage        = RowReader.GetInt(row, "Mileage"),
                CreatedAt      = RowReader.GetDate(row, "CreatedAt")
            };
        }
    }
}
