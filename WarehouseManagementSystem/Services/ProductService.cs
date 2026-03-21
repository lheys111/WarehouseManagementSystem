using System;
using System.Data;
using Npgsql;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Services
{
    public class ProductService
    {
        public DataTable GetAllProducts(string searchText = "")
        {
            string sql = @"SELECT Id, Article, Name, CategoryName, UnitOfMeasure, PurchasePrice, StockQuantity
                          FROM vw_ProductsWithStock";

            if (!string.IsNullOrEmpty(searchText))
            {
                sql += " WHERE Article ILIKE @Search OR Name ILIKE @Search";
            }

            sql += " ORDER BY Name";

            if (!string.IsNullOrEmpty(searchText))
            {
                return DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@Search", "%" + searchText + "%") });
            }

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public Product GetProductById(int id)
        {
            string sql = @"SELECT p.*, c.Name as CategoryName, COALESCE(s.Quantity, 0) as StockQuantity
                          FROM Products p
                          LEFT JOIN Categories c ON p.CategoryId = c.Id
                          LEFT JOIN StockBalances s ON p.Id = s.ProductId
                          WHERE p.Id = @Id";

            var result = DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@Id", id) });

            if (result.Rows.Count == 0)
                return null;

            var row = result.Rows[0];
            return new Product
            {
                Id = Convert.ToInt32(row["Id"]),
                Article = row["Article"].ToString(),
                Name = row["Name"].ToString(),
                CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt32(row["CategoryId"]) : (int?)null,
                CategoryName = row["CategoryName"].ToString(),
                UnitOfMeasure = row["UnitOfMeasure"].ToString(),
                PurchasePrice = Convert.ToDecimal(row["PurchasePrice"]),
                ShelfLife = row["ShelfLife"] != DBNull.Value ? Convert.ToInt32(row["ShelfLife"]) : (int?)null,
                StockQuantity = Convert.ToDecimal(row["StockQuantity"])
            };
        }

        public bool IsArticleUnique(string article, int? excludeId = null)
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE Article = @Article";
            if (excludeId.HasValue)
                sql += " AND Id != @Id";

            var parameters = new System.Collections.Generic.List<NpgsqlParameter> { new NpgsqlParameter("@Article", article) };
            if (excludeId.HasValue)
                parameters.Add(new NpgsqlParameter("@Id", excludeId.Value));

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, parameters.ToArray()));
            return count == 0;
        }

        public int AddProduct(Product product)
        {
            if (!IsArticleUnique(product.Article))
                throw new Exception("Артикул уже существует");

            string insertSql = @"INSERT INTO Products (Article, Name, CategoryId, UnitOfMeasure, PurchasePrice, ShelfLife) 
                                VALUES (@Article, @Name, @CategoryId, @Unit, @Price, @ShelfLife) RETURNING Id";

            var insertParams = new[]
            {
                new NpgsqlParameter("@Article", product.Article),
                new NpgsqlParameter("@Name", product.Name),
                new NpgsqlParameter("@CategoryId", product.CategoryId.HasValue ? (object)product.CategoryId.Value : DBNull.Value),
                new NpgsqlParameter("@Unit", product.UnitOfMeasure),
                new NpgsqlParameter("@Price", product.PurchasePrice),
                new NpgsqlParameter("@ShelfLife", product.ShelfLife.HasValue ? (object)product.ShelfLife.Value : DBNull.Value)
            };

            int newId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertSql, insertParams));

            string stockSql = "INSERT INTO StockBalances (ProductId, Quantity) VALUES (@ProductId, 0)";
            DatabaseHelper.ExecuteNonQuery(stockSql, new[] { new NpgsqlParameter("@ProductId", newId) });

            return newId;
        }

        public void UpdateProduct(Product product)
        {
            if (!IsArticleUnique(product.Article, product.Id))
                throw new Exception("Артикул уже существует");

            string updateSql = @"UPDATE Products 
                                SET Article = @Article, Name = @Name, CategoryId = @CategoryId,
                                    UnitOfMeasure = @Unit, PurchasePrice = @Price, ShelfLife = @ShelfLife
                                WHERE Id = @Id";

            var updateParams = new[]
            {
                new NpgsqlParameter("@Article", product.Article),
                new NpgsqlParameter("@Name", product.Name),
                new NpgsqlParameter("@CategoryId", product.CategoryId.HasValue ? (object)product.CategoryId.Value : DBNull.Value),
                new NpgsqlParameter("@Unit", product.UnitOfMeasure),
                new NpgsqlParameter("@Price", product.PurchasePrice),
                new NpgsqlParameter("@ShelfLife", product.ShelfLife.HasValue ? (object)product.ShelfLife.Value : DBNull.Value),
                new NpgsqlParameter("@Id", product.Id)
            };

            DatabaseHelper.ExecuteNonQuery(updateSql, updateParams);
        }

        public void DeleteProduct(int id)
        {
            string checkSql = "SELECT COUNT(*) FROM ShipmentDetails WHERE ProductId = @ProductId";
            int shipmentCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new[] { new NpgsqlParameter("@ProductId", id) }));

            if (shipmentCount > 0)
                throw new Exception("Нельзя удалить товар, так как есть отгрузки с этим товаром");

            string deleteSql = "DELETE FROM Products WHERE Id = @Id";
            DatabaseHelper.ExecuteNonQuery(deleteSql, new[] { new NpgsqlParameter("@Id", id) });
        }

        public DataTable GetProductsForShipment(string searchText = "")
        {
            string sql = @"SELECT Id, Article, Name, UnitOfMeasure, StockQuantity, PurchasePrice
                          FROM vw_ProductsWithStock
                          WHERE StockQuantity > 0";

            if (!string.IsNullOrEmpty(searchText))
            {
                sql += " AND (Article ILIKE @Search OR Name ILIKE @Search)";
            }

            sql += " ORDER BY Name";

            if (!string.IsNullOrEmpty(searchText))
            {
                return DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@Search", "%" + searchText + "%") });
            }

            return DatabaseHelper.ExecuteQuery(sql);
        }
    }
}
