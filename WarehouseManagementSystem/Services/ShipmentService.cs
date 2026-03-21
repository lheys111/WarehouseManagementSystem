using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using WarehouseManagementSystem.Helpers;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Services
{
    public class ShipmentService
    {
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

        public string GenerateShipmentNumber()
        {
            try
            {
                string sql = "SELECT generate_shipment_number()";
                return DatabaseHelper.ExecuteScalar(sql).ToString();
            }
            catch
            {
                return $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1, 999):D3}";
            }
        }

        public bool CheckStockAvailability(int productId, decimal quantity)
        {
            return DatabaseHelper.CheckStockAvailability(productId, quantity);
        }

        public decimal GetAvailableStock(int productId)
        {
            string sql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
            var result = DatabaseHelper.ExecuteScalar(sql, new[] { new NpgsqlParameter("@ProductId", productId) });
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        public bool ProcessShipment(Shipment shipment)
        {
            if (shipment.Items.Count == 0)
                throw new Exception("Отгрузка не содержит товаров");

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in shipment.Items)
                        {
                            string checkSql = "SELECT Quantity FROM StockBalances WHERE ProductId = @ProductId";
                            using (var cmd = new NpgsqlCommand(checkSql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                var stock = cmd.ExecuteScalar();
                                decimal available = stock != null ? Convert.ToDecimal(stock) : 0;

                                if (available < item.Quantity)
                                    throw new Exception($"Недостаточно товара '{item.ProductName}'. Доступно: {available}");
                            }
                        }

                        string shipmentSql = @"INSERT INTO Shipments (ShipmentNumber, ShipmentDate, StorekeeperId, Status) 
                                              VALUES (@Number, @Date, @StorekeeperId, 'Completed') RETURNING Id";
                        int shipmentId;
                        using (var cmd = new NpgsqlCommand(shipmentSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Number", shipment.ShipmentNumber);
                            cmd.Parameters.AddWithValue("@Date", shipment.ShipmentDate);
                            cmd.Parameters.AddWithValue("@StorekeeperId", Session.CurrentUser.Id);
                            shipmentId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var item in shipment.Items)
                        {
                            string detailSql = @"INSERT INTO ShipmentDetails (ShipmentId, ProductId, Quantity, PriceAtShipment) 
                                                VALUES (@ShipmentId, @ProductId, @Quantity, @Price)";
                            using (var cmd = new NpgsqlCommand(detailSql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ShipmentId", shipmentId);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@Price", item.PriceAtShipment);
                                cmd.ExecuteNonQuery();
                            }

                            string stockSql = "UPDATE StockBalances SET Quantity = Quantity - @Quantity, UpdatedAt = CURRENT_TIMESTAMP WHERE ProductId = @ProductId";
                            using (var cmd = new NpgsqlCommand(stockSql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception($"Ошибка при проведении отгрузки: {ex.Message}");
                    }
                }
            }
        }

        public DataTable GetAllShipments()
        {
            string sql = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                          FROM vw_ShipmentsHistory
                          ORDER BY ShipmentDate DESC";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable GetShipmentsByStorekeeper(int storekeeperId)
        {
            string sql = @"SELECT Id, ShipmentNumber, ShipmentDate, StorekeeperName, ItemsCount, TotalSum
                          FROM vw_ShipmentsHistory
                          WHERE StorekeeperId = @StorekeeperId
                          ORDER BY ShipmentDate DESC";
            return DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@StorekeeperId", storekeeperId) });
        }

        public DataTable GetShipmentDetails(int shipmentId)
        {
            string sql = @"SELECT p.Article, p.Name, sd.Quantity, sd.PriceAtShipment
                          FROM ShipmentDetails sd
                          JOIN Products p ON sd.ProductId = p.Id
                          WHERE sd.ShipmentId = @ShipmentId
                          ORDER BY p.Name";
            return DatabaseHelper.ExecuteQuery(sql, new[] { new NpgsqlParameter("@ShipmentId", shipmentId) });
        }
    }
}
