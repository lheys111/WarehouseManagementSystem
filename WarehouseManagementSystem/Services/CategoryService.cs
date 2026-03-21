using Npgsql;
using System;
using System.Data;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem.Services
{
    public class CategoryService
    {
        public DataTable GetAllCategories()
        {
            string sql = @"SELECT c.Id, c.Name, c.Description, COUNT(p.Id) as ProductsCount
                          FROM Categories c
                          LEFT JOIN Products p ON c.Id = p.CategoryId
                          GROUP BY c.Id, c.Name, c.Description
                          ORDER BY c.Name";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataRow GetCategoryById(int id)
        {
            string sql = "SELECT Id, Name, Description FROM Categories WHERE Id = @Id";
            var result = DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@Id", id) });
            return result.Rows.Count > 0 ? result.Rows[0] : null;
        }

        public int AddCategory(string name, string description)
        {
            string sql = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description) RETURNING Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description)
            };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, parameters));
        }

        public void UpdateCategory(int id, string name, string description)
        {
            string sql = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description),
                new NpgsqlParameter("@Id", id)
            };
            DatabaseHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteCategory(int id)
        {
            string checkSql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId";
            int productCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new[] { new NpgsqlParameter("@CategoryId", id) }));

            if (productCount > 0)
                throw new Exception("Нельзя удалить категорию, так как в ней есть товары");

            string deleteSql = "DELETE FROM Categories WHERE Id = @Id";
            DatabaseHelper.ExecuteNonQuery(deleteSql, new[] { new NpgsqlParameter("@Id", id) });
        }

        public DataTable GetCategoriesForCombo()
        {
            string sql = "SELECT Id, Name FROM Categories ORDER BY Name";
            return DatabaseHelper.ExecuteQuery(sql);
        }
    }
}