using System;

namespace WarehouseManagementSystem.Models
{
    public class ShipmentItem
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAtShipment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
