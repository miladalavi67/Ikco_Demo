using System.Data;
using System.Data.SqlClient;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Data
{
    public class CustomerRepository
    {
        public PagedResult<Customer> GetList(string search, bool onlyActive, int pageIndex, int pageSize)
        {
            var totalParam = SqlHelper.Out("@TotalCount", SqlDbType.Int);

            var table = SqlHelper.ExecuteDataTable("dbo.usp_Customer_GetList",
                SqlHelper.In("@Search", string.IsNullOrWhiteSpace(search) ? null : search.Trim()),
                SqlHelper.In("@OnlyActive", onlyActive),
                SqlHelper.In("@PageIndex", pageIndex),
                SqlHelper.In("@PageSize", pageSize),
                totalParam);

            var result = new PagedResult<Customer>
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

        public Customer GetById(int customerId)
        {
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Customer_GetById",
                SqlHelper.In("@CustomerId", customerId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public int Insert(Customer customer)
        {
            var idParam = SqlHelper.Out("@CustomerId", SqlDbType.Int);

            SqlHelper.ExecuteNonQuery("dbo.usp_Customer_Insert",
                SqlHelper.In("@FullName", customer.FullName),
                SqlHelper.In("@NationalCode", customer.NationalCode),
                SqlHelper.In("@Mobile", customer.Mobile),
                SqlHelper.In("@Email", customer.Email),
                SqlHelper.In("@City", customer.City),
                SqlHelper.In("@Address", customer.Address),
                SqlHelper.In("@IsActive", customer.IsActive),
                idParam);

            customer.CustomerId = (int)idParam.Value;
            return customer.CustomerId;
        }

        public void Update(Customer customer)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Customer_Update",
                SqlHelper.In("@CustomerId", customer.CustomerId),
                SqlHelper.In("@FullName", customer.FullName),
                SqlHelper.In("@NationalCode", customer.NationalCode),
                SqlHelper.In("@Mobile", customer.Mobile),
                SqlHelper.In("@Email", customer.Email),
                SqlHelper.In("@City", customer.City),
                SqlHelper.In("@Address", customer.Address),
                SqlHelper.In("@IsActive", customer.IsActive));
        }

        public void Delete(int customerId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Customer_Delete",
                SqlHelper.In("@CustomerId", customerId));
        }

        /// <summary>Used by the loyalty-discount rule on the service request form.</summary>
        public int GetCompletedRequestCount(int customerId)
        {
            var value = SqlHelper.ExecuteScalar("dbo.usp_Customer_GetCompletedRequestCount",
                SqlHelper.In("@CustomerId", customerId));

            return value == null || value == System.DBNull.Value ? 0 : System.Convert.ToInt32(value);
        }

        private static Customer Map(DataRow row)
        {
            return new Customer
            {
                CustomerId   = RowReader.GetInt(row, "CustomerId"),
                FullName     = RowReader.GetString(row, "FullName"),
                NationalCode = RowReader.GetString(row, "NationalCode"),
                Mobile       = RowReader.GetString(row, "Mobile"),
                Email        = RowReader.GetString(row, "Email"),
                City         = RowReader.GetString(row, "City"),
                Address      = RowReader.GetString(row, "Address"),
                IsActive     = RowReader.GetBool(row, "IsActive"),
                CreatedAt    = RowReader.GetDate(row, "CreatedAt"),
                VehicleCount = RowReader.GetInt(row, "VehicleCount")
            };
        }
    }
}
