using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace WarehouseManagementSystem.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с базой данных PostgreSQL
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string _connectionString = "Host=localhost;Port=5432;Database=WarehouseBD;Username=postgres;Password=postgres;";

        /// <summary>
        /// Получить соединение с базой данных
        /// </summary>
        /// <returns>Объект подключения к PostgreSQL</returns>
        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        /// <summary>
        /// Выполнить SQL запрос без возврата данных (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Количество затронутых строк</returns>
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

        /// <summary>
        /// Выполнить SQL запрос и вернуть результат в виде DataTable
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>DataTable с результатами запроса</returns>
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

        /// <summary>
        /// Выполнить SQL запрос и вернуть первое значение первой строки
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Первое значение результата запроса</returns>
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

        /// <summary>
        /// Проверить доступность товара на складе
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="quantity">Запрашиваемое количество</param>
        /// <returns>True - если достаточно, False - если недостаточно</returns>
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

        /// <summary>
        /// Проверить подключение к базе данных
        /// </summary>
        /// <returns>True - если подключение успешно, False - если ошибка</returns>
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