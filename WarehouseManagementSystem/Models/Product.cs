using System;

namespace WarehouseManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal PurchasePrice { get; set; }
        public int? ShelfLife { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal StockQuantity { get; set; } // Только для отображения, не хранится в Products
    }
}