using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseManagementSystem.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public string ShipmentNumber { get; set; }
        public DateTime ShipmentDate { get; set; }
        public int StorekeeperId { get; set; }
        public string StorekeeperName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
        public int ItemsCount => Items.Count;
        public decimal TotalSum => Items.Sum(i => i.Quantity * i.PriceAtShipment);
    }
}