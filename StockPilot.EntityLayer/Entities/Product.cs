using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.EntityLayer.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; } = true;
        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
    = new List<WarehouseStock>();
        public ICollection<StockMovement> StockMovements { get; set; }
    = new List<StockMovement>();
    }
}
