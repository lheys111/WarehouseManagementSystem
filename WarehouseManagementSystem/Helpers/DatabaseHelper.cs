using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace WarehouseManagementSystem.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString = "Host=localhost;Port=5432;Database=WarehouseDB;Username=postgres;Password=postgres;";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        var data = new DataTable();
                        adapter.Fill(data);
                        return data;
                    }
                }
            }
        }

        public static object ExecuteScalar(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static bool CheckStockAvailability(int productId, decimal quantity)
        {
            var sql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            var parameters = new[] { new NpgsqlParameter("@ProductId", productId) };

            var result = ExecuteScalar(sql, parameters);
            if (result != null && result != DBNull.Value)
            {
                var available = Convert.ToDecimal(result);
                return available >= quantity;
            }
            return false;
        }
        
        public static bool TestConnection()
         {
             try
             {
                 using (var conn = GetConnection())
                 {
                     conn.Open();
                     return true;
                 }
             }
             catch (Exception ex)
             {
                 AppLogger.Error(ex, "Ошибка подключения к базе данных");
                 MessageBox.Show(Constants.Messages.ConnectionError, "Ошибка",
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return false;
             }
         }
    }
}
