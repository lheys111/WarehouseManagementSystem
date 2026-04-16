using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Models
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}
