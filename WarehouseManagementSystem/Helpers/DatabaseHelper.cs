using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace WarehouseManagementSystem.Helpers
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Host=localhost;Port=5432;Database=WarehouseDB;Username=postgres;Password=postgres;";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        public static int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
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
            using (NpgsqlConnection conn = GetConnection())
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static object ExecuteScalar(string query, NpgsqlParameter[] parameters = null)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
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
            string sql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ProductId", productId) };

            object result = ExecuteScalar(sql, parameters);
            if (result != null && result != DBNull.Value)
            {
                decimal available = Convert.ToDecimal(result);
                return available >= quantity;
            }
            return false;
        }

        public static decimal GetStockQuantity(int productId)
        {
            string sql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ProductId", productId) };

            object result = ExecuteScalar(sql, parameters);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public static void UpdateStock(int productId, decimal quantity)
        {
            string sql = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Quantity", quantity),
                new NpgsqlParameter("@ProductId", productId)
            };
            ExecuteNonQuery(sql, parameters);
        }

        public static int ExecuteNonQueryInTransaction(string query, NpgsqlParameter[] parameters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalarInTransaction(string query, NpgsqlParameter[] parameters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
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
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}\n\n" +
                    $"Проверьте:\n" +
                    $"1. Запущен ли PostgreSQL сервер\n" +
                    $"2. Правильный ли пароль в строке подключения\n" +
                    $"3. Существует ли база данных WarehouseDB\n" +
                    $"4. Правильный ли порт (по умолчанию 5432)",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static string GenerateShipmentNumber()
        {
            try
            {
                string sql = "SELECT generate_shipment_number()";
                object result = ExecuteScalar(sql);
                if (result != null && result != DBNull.Value)
                    return result.ToString();
            }
            catch
            {
            }
            return $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1, 999):D3}";
        }

        public static DataTable GetCategoriesForCombo()
        {
            string sql = "SELECT Id, Name FROM Categories ORDER BY Name";
            return ExecuteQuery(sql);
        }

        public static bool IsArticleUnique(string article, int? excludeId = null)
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE Article = @Article";
            if (excludeId.HasValue)
                sql += " AND Id != @Id";

            var parameters = new System.Collections.Generic.List<NpgsqlParameter> { new NpgsqlParameter("@Article", article) };
            if (excludeId.HasValue)
                parameters.Add(new NpgsqlParameter("@Id", excludeId.Value));

            int count = Convert.ToInt32(ExecuteScalar(sql, parameters.ToArray()));
            return count == 0;
        }

        public static DataTable GetAllShipments()
        {
            string sql = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                          FROM vw_ShipmentsHistory
                          ORDER BY ShipmentDate DESC";
            return ExecuteQuery(sql);
        }

        public static DataTable GetShipmentsByStorekeeper(int storekeeperId)
        {
            string sql = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                          FROM vw_ShipmentsHistory
                          WHERE StorekeeperId = @StorekeeperId
                          ORDER BY ShipmentDate DESC";
            return ExecuteQuery(sql, new[] { new NpgsqlParameter("@StorekeeperId", storekeeperId) });
        }

        public static DataTable GetShipmentDetails(int shipmentId)
        {
            string sql = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                          FROM ShipmentDetails sd
                          JOIN Products p ON sd.ProductId = p.Id
                          WHERE sd.ShipmentId = @ShipmentId
                          ORDER BY p.Name";
            return ExecuteQuery(sql, new[] { new NpgsqlParameter("@ShipmentId", shipmentId) });
        }

        public static DataTable GetUserByEmail(string email)
        {
            string sql = "SELECT Id, FullName, Email, Role, PasswordHash FROM Users WHERE Email = @Email";
            return ExecuteQuery(sql, new[] { new NpgsqlParameter("@Email", email) });
        }

        public static bool IsEmailExists(string email)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int count = Convert.ToInt32(ExecuteScalar(sql, new[] { new NpgsqlParameter("@Email", email) }));
            return count > 0;
        }

        public static int AddUser(string fullName, string email, string passwordHash, string role)
        {
            string sql = @"INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                          VALUES (@FullName, @Email, @PasswordHash, @Role) RETURNING Id";
            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@PasswordHash", passwordHash),
                new NpgsqlParameter("@Role", role)
            };
            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }
    }
}

