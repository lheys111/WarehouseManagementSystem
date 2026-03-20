using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;
using static System.Collections.Specialized.BitVector32;

namespace WarehouseManagementSystem.Services
{
    public class ShipmentService
    {
        // Создание черновика отгрузки
        public Shipment CreateDraft()
        {
            return new Shipment
            {
                ShipmentNumber = GenerateShipmentNumber(),
                ShipmentDate = DateTime.Now,
                Status = "Draft",
                Items = new List<ShipmentItem>()
            };
        }

        // Генерация номера отгрузки
        public string GenerateShipmentNumber()
        {
            try
            {
                string query = "SELECT generate_shipment_number()";
                return DatabaseHelper.ExecuteScalar(query).ToString();
            }
            catch
            {
                return $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1, 999):D3}";
            }
        }

        // Проверка доступности товара
        public bool CheckStockAvailability(int productId, decimal quantity)
        {
            return DatabaseHelper.CheckStockAvailability(productId, quantity);
        }

        // Получение доступного остатка
        public decimal GetAvailableStock(int productId)
        {
            string query = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            var result = DatabaseHelper.ExecuteScalar(query, new[] { new NpgsqlParameter("@ProductId", productId) });
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        // Проведение отгрузки (транзакция)
        public bool ProcessShipment(Shipment shipment)
        {
            if (shipment.Items.Count == 0)
                throw new Exception("Отгрузка не содержит товаров");

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Проверяем остатки по всем товарам
                        foreach (var item in shipment.Items)
                        {
                            string checkQuery = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
                            using (var cmd = new NpgsqlCommand(checkQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                var stock = cmd.ExecuteScalar();
                                decimal available = stock != null ? Convert.ToDecimal(stock) : 0;

                                if (available < item.Quantity)
                                    throw new Exception($"Недостаточно товара '{item.ProductName}'. Доступно: {available}");
                            }
                        }

                        // Создаем отгрузку
                        string shipmentQuery = @"INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status) 
                                                VALUES (@Number, @Date, @StorekeeperId, 'Completed') RETURNING Id";
                        int shipmentId;
                        using (var cmd = new NpgsqlCommand(shipmentQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Number", shipment.ShipmentNumber);
                            cmd.Parameters.AddWithValue("@Date", shipment.ShipmentDate);
                            cmd.Parameters.AddWithValue("@StorekeeperId", Session.CurrentUser.Id);
                            shipmentId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Добавляем детали и списываем остатки
                        foreach (var item in shipment.Items)
                        {
                            // Добавляем деталь
                            string detailQuery = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                                                  VALUES (@ShipmentId, @ProductId, @Quantity, @Price)";
                            using (var cmd = new NpgsqlCommand(detailQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@Price", item.PriceAtShipment);
                                cmd.ExecuteNonQuery();
                            }

                            // Списываем остатки
                            string stockQuery = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
                            using (var cmd = new NpgsqlCommand(stockQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Ошибка при проведении отгрузки: {ex.Message}");
                    }
                }
            }
        }

        // Получение всех отгрузок
        public DataTable GetAllShipments()
        {
            string query = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                            FROM vw_ShipmentsHistory
                            ORDER BY ShipmentDate DESC";
            return DatabaseHelper.ExecuteQuery(query);
        }

        // Получение отгрузок кладовщика
        public DataTable GetShipmentsByStorekeeper(int storekeeperId)
        {
            string query = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                            FROM vw_ShipmentsHistory
                            WHERE StorekeeperId = @StorekeeperId
                            ORDER BY ShipmentDate DESC";
            return DatabaseHelper.ExecuteQuery(query, new[] { new NpgsqlParameter("@StorekeeperId", storekeeperId) });
        }

        // Получение деталей отгрузки
        public DataTable GetShipmentDetails(int shipmentId)
        {
            string query = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                            FROM ShipmentDetails sd
                            JOIN Products p ON sd.ProductId = p.Id
                            WHERE sd.ShipmentId = @ShipmentId
                            ORDER BY p.Name";
            return DatabaseHelper.ExecuteQuery(query, new[] { new NpgsqlParameter("@ShipmentId", shipmentId) });
        }
    }
}