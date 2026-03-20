using Npgsql;
using System;
using System.Data;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Services
{
    public class CategoryService
    {
        // Получить все категории
        public DataTable GetAllCategories()
        {
            string query = @"SELECT c.Id, c.Name, c.Description, COUNT(p.Id) as ProductsCount
                            FROM Categories c
                            LEFT JOIN Products p ON c.Id = p.CategoryId
                            GROUP BY c.Id, c.Name, c.Description
                            ORDER BY c.Name";
            return DatabaseHelper.ExecuteQuery(query);
        }

        // Получить категорию по ID
        public DataRow GetCategoryById(int id)
        {
            string query = "SELECT Id, Name, Description FROM Categories WHERE Id = @Id";
            var dt = DatabaseHelper.ExecuteQuery(query, new[] { new NpgsqlParameter("@Id", id) });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // Добавление категории
        public int AddCategory(string name, string description)
        {
            string query = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description) RETURNING Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description)
            };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        // Обновление категории
        public void UpdateCategory(int id, string name, string description)
        {
            string query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description),
                new NpgsqlParameter("@Id", id)
            };
            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }

        // Удаление категории
        public void DeleteCategory(int id)
        {
            // Проверяем, есть ли товары в категории
            string checkQuery = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, new[] { new NpgsqlParameter("@CategoryId", id) }));

            if (count > 0)
                throw new Exception("Нельзя удалить категорию, так как в ней есть товары");

            string query = "DELETE FROM Categories WHERE Id = @Id";
            DatabaseHelper.ExecuteNonQuery(query, new[] { new NpgsqlParameter("@Id", id) });
        }

        // Получить категории для ComboBox
        public DataTable GetCategoriesForCombo()
        {
            string query = "SELECT Id, Name FROM Categories ORDER BY Name";
            return DatabaseHelper.ExecuteQuery(query);
        }
    }
}