using System;
using System.Data;
using System.Data.SqlClient;
using IKCO.AfterSales.WinForms.Common;

namespace IKCO.AfterSales.WinForms.Data
{
    /// <summary>
    /// Thin ADO.NET wrapper. Every call in this application is a stored procedure.
    /// </summary>
    public static class SqlHelper
    {
        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(AppSettings.ConnectionString);
        }

        public static SqlParameter In(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter Out(string name, SqlDbType type, int size = 0)
        {
            var p = new SqlParameter(name, type) { Direction = ParameterDirection.Output };
            if (size > 0) p.Size = size;
            return p;
        }

        public static DataTable ExecuteDataTable(string procedureName, params SqlParameter[] parameters)
        {
            var table = new DataTable();

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(procedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        public static int ExecuteNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(procedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string procedureName, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(procedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public static bool TestConnection(out string error)
        {
            error = null;
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }

    /// <summary>
    /// Null-safe readers for DataRow fields.
    /// </summary>
    public static class RowReader
    {
        public static int GetInt(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToInt32(row[column]) : 0;
        }

        public static int? GetNullableInt(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? (int?)Convert.ToInt32(row[column]) : null;
        }

        public static decimal GetDecimal(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToDecimal(row[column]) : 0m;
        }

        public static string GetString(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToString(row[column]) : string.Empty;
        }

        public static bool GetBool(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                && Convert.ToBoolean(row[column]);
        }

        public static DateTime GetDate(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToDateTime(row[column]) : DateTime.MinValue;
        }

        public static DateTime? GetNullableDate(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(row[column]) : null;
        }
    }
}
