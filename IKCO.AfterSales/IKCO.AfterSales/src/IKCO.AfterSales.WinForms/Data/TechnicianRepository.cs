using System.Collections.Generic;
using System.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Data
{
    public class TechnicianRepository
    {
        public PagedResult<Technician> GetList(string search, bool onlyActive, int pageIndex, int pageSize)
        {
            var totalParam = SqlHelper.Out("@TotalCount", SqlDbType.Int);

            var table = SqlHelper.ExecuteDataTable("dbo.usp_Technician_GetList",
                SqlHelper.In("@Search", string.IsNullOrWhiteSpace(search) ? null : search.Trim()),
                SqlHelper.In("@OnlyActive", onlyActive),
                SqlHelper.In("@PageIndex", pageIndex),
                SqlHelper.In("@PageSize", pageSize),
                totalParam);

            var result = new PagedResult<Technician>
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

        public List<Technician> GetActiveLookup()
        {
            var list = new List<Technician>();
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Technician_GetActiveLookup");

            foreach (DataRow row in table.Rows)
                list.Add(Map(row));

            return list;
        }

        public Technician GetById(int technicianId)
        {
            var table = SqlHelper.ExecuteDataTable("dbo.usp_Technician_GetById",
                SqlHelper.In("@TechnicianId", technicianId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public int Insert(Technician technician)
        {
            var idParam = SqlHelper.Out("@TechnicianId", SqlDbType.Int);

            SqlHelper.ExecuteNonQuery("dbo.usp_Technician_Insert",
                SqlHelper.In("@FullName", technician.FullName),
                SqlHelper.In("@PersonnelCode", technician.PersonnelCode),
                SqlHelper.In("@Specialty", technician.Specialty),
                SqlHelper.In("@HourlyRate", technician.HourlyRate),
                SqlHelper.In("@IsActive", technician.IsActive),
                idParam);

            technician.TechnicianId = (int)idParam.Value;
            return technician.TechnicianId;
        }

        public void Update(Technician technician)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Technician_Update",
                SqlHelper.In("@TechnicianId", technician.TechnicianId),
                SqlHelper.In("@FullName", technician.FullName),
                SqlHelper.In("@PersonnelCode", technician.PersonnelCode),
                SqlHelper.In("@Specialty", technician.Specialty),
                SqlHelper.In("@HourlyRate", technician.HourlyRate),
                SqlHelper.In("@IsActive", technician.IsActive));
        }

        public void Delete(int technicianId)
        {
            SqlHelper.ExecuteNonQuery("dbo.usp_Technician_Delete",
                SqlHelper.In("@TechnicianId", technicianId));
        }

        private static Technician Map(DataRow row)
        {
            return new Technician
            {
                TechnicianId  = RowReader.GetInt(row, "TechnicianId"),
                FullName      = RowReader.GetString(row, "FullName"),
                PersonnelCode = RowReader.GetString(row, "PersonnelCode"),
                Specialty     = RowReader.GetString(row, "Specialty"),
                HourlyRate    = RowReader.GetDecimal(row, "HourlyRate"),
                IsActive      = RowReader.GetBool(row, "IsActive")
            };
        }
    }
}
