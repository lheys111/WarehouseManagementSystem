using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace WarehouseManagementSystem.Helpers
{
    public static class DatabaseHelper
    {
        // =====================================================
        // СТРОКА ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ
        // =====================================================
        // ИЗМЕНИТЕ ПАРОЛЬ НА ВАШ!
        // Пользователь: postgres
        // Пароль: (тот, что вы задали при установке PostgreSQL)
        // База данных: WarehouseDB
        // Порт: 5432
        // =====================================================
        private static string connectionString = "Host=localhost;Port=5432;Database=WarehouseDB;Username=postgres;Password=postgres;";

        // =====================================================
        // ОСНОВНЫЕ МЕТОДЫ ДЛЯ РАБОТЫ С БАЗОЙ ДАННЫХ
        // =====================================================

        /// <summary>
        /// Получить соединение с базой данных
        /// </summary>
        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        /// <summary>
        /// Выполнить запрос без возврата данных (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Количество затронутых строк</returns>
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

        /// <summary>
        /// Выполнить запрос и вернуть DataTable (SELECT)
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>DataTable с результатами</returns>
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

        /// <summary>
        /// Выполнить скалярный запрос (возвращает одно значение)
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Первое значение первого столбца</returns>
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

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С ОСТАТКАМИ
        // =====================================================

        /// <summary>
        /// Проверить доступность товара на складе
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="quantity">Запрашиваемое количество</param>
        /// <returns>True если достаточно, иначе False</returns>
        public static bool CheckStockAvailability(int productId, decimal quantity)
        {
            string query = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ProductId", productId) };

            object result = ExecuteScalar(query, parameters);
            if (result != null && result != DBNull.Value)
            {
                decimal available = Convert.ToDecimal(result);
                return available >= quantity;
            }
            return false;
        }

        /// <summary>
        /// Получить остаток товара
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <returns>Текущий остаток</returns>
        public static decimal GetStockQuantity(int productId)
        {
            string query = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ProductId", productId) };

            object result = ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        /// <summary>
        /// Обновить остаток товара (списание)
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="quantity">Количество для списания</param>
        public static void UpdateStock(int productId, decimal quantity)
        {
            string query = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Quantity", quantity),
                new NpgsqlParameter("@ProductId", productId)
            };
            ExecuteNonQuery(query, parameters);
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ В ТРАНЗАКЦИЯХ
        // =====================================================

        /// <summary>
        /// Выполнить запрос в транзакции
        /// </summary>
        public static int ExecuteNonQueryInTransaction(string query, NpgsqlParameter[] parameters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Получить скалярное значение в транзакции
        /// </summary>
        public static object ExecuteScalarInTransaction(string query, NpgsqlParameter[] parameters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ ПРОВЕРКИ ПОДКЛЮЧЕНИЯ
        // =====================================================

        /// <summary>
        /// Проверить подключение к базе данных
        /// </summary>
        /// <returns>True если подключение успешно, иначе False</returns>
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

        // =====================================================
        // МЕТОДЫ ДЛЯ ГЕНЕРАЦИИ НОМЕРОВ
        // =====================================================

        /// <summary>
        /// Сгенерировать номер отгрузки
        /// </summary>
        /// <returns>Номер отгрузки в формате INV-YYYYMMDD-XXX</returns>
        public static string GenerateShipmentNumber()
        {
            try
            {
                // Пытаемся использовать функцию из БД
                string query = "SELECT generate_shipment_number()";
                object result = ExecuteScalar(query);
                if (result != null && result != DBNull.Value)
                    return result.ToString();
            }
            catch
            {
                // Если функции нет, генерируем сами
            }
            // Запасной вариант генерации
            return $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1, 999):D3}";
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С КАТЕГОРИЯМИ
        // =====================================================

        /// <summary>
        /// Получить все категории для ComboBox
        /// </summary>
        /// <returns>DataTable с категориями (Id, Name)</returns>
        public static DataTable GetCategoriesForCombo()
        {
            string query = "SELECT Id, Name FROM Categories ORDER BY Name";
            return ExecuteQuery(query);
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С ТОВАРАМИ
        // =====================================================

        /// <summary>
        /// Проверить уникальность артикула
        /// </summary>
        /// <param name="article">Артикул для проверки</param>
        /// <param name="excludeId">ID товара, который исключить из проверки (при редактировании)</param>
        /// <returns>True если артикул уникален, иначе False</returns>
        public static bool IsArticleUnique(string article, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM Products WHERE Article = @Article";
            if (excludeId.HasValue)
                query += " AND Id != @Id";

            var parameters = new System.Collections.Generic.List<NpgsqlParameter> { new NpgsqlParameter("@Article", article) };
            if (excludeId.HasValue)
                parameters.Add(new NpgsqlParameter("@Id", excludeId.Value));

            int count = Convert.ToInt32(ExecuteScalar(query, parameters.ToArray()));
            return count == 0;
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С ОТГРУЗКАМИ
        // =====================================================

        /// <summary>
        /// Получить все отгрузки (для администратора)
        /// </summary>
        /// <returns>DataTable со всеми отгрузками</returns>
        public static DataTable GetAllShipments()
        {
            string query = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                            FROM vw_ShipmentsHistory
                            ORDER BY ShipmentDate DESC";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получить отгрузки конкретного кладовщика
        /// </summary>
        /// <param name="storekeeperId">ID кладовщика</param>
        /// <returns>DataTable с отгрузками кладовщика</returns>
        public static DataTable GetShipmentsByStorekeeper(int storekeeperId)
        {
            string query = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                            FROM vw_ShipmentsHistory
                            WHERE StorekeeperId = @StorekeeperId
                            ORDER BY ShipmentDate DESC";
            return ExecuteQuery(query, new[] { new NpgsqlParameter("@StorekeeperId", storekeeperId) });
        }

        /// <summary>
        /// Получить детали отгрузки
        /// </summary>
        /// <param name="shipmentId">ID отгрузки</param>
        /// <returns>DataTable с деталями отгрузки</returns>
        public static DataTable GetShipmentDetails(int shipmentId)
        {
            string query = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                            FROM ShipmentDetails sd
                            JOIN Products p ON sd.ProductId = p.Id
                            WHERE sd.ShipmentId = @ShipmentId
                            ORDER BY p.Name";
            return ExecuteQuery(query, new[] { new NpgsqlParameter("@ShipmentId", shipmentId) });
        }

        // =====================================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С ПОЛЬЗОВАТЕЛЯМИ
        // =====================================================

        /// <summary>
        /// Получить пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>DataTable с данными пользователя</returns>
        public static DataTable GetUserByEmail(string email)
        {
            string query = "SELECT Id, FullName, Email, Role, PasswordHash FROM Users WHERE Email = @Email";
            return ExecuteQuery(query, new[] { new NpgsqlParameter("@Email", email) });
        }

        /// <summary>
        /// Проверить существование email
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True если email существует, иначе False</returns>
        public static bool IsEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            int count = Convert.ToInt32(ExecuteScalar(query, new[] { new NpgsqlParameter("@Email", email) }));
            return count > 0;
        }

        /// <summary>
        /// Добавить нового пользователя
        /// </summary>
        /// <param name="fullName">ФИО</param>
        /// <param name="email">Email</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <param name="role">Роль (Admin/Storekeeper)</param>
        /// <returns>ID нового пользователя</returns>
        public static int AddUser(string fullName, string email, string passwordHash, string role)
        {
            string query = @"INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                            VALUES (@FullName, @Email, @PasswordHash, @Role) RETURNING Id";
            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@PasswordHash", passwordHash),
                new NpgsqlParameter("@Role", role)
            };
            return Convert.ToInt32(ExecuteScalar(query, parameters));
        }
    }
}
