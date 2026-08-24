using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.EntityLayer.Entities
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
    = new List<WarehouseStock>();
        public ICollection<StockMovement> OutgoingStockMovements { get; set; }
    = new List<StockMovement>();

        public ICollection<StockMovement> IncomingStockMovements { get; set; }
            = new List<StockMovement>();
    }
}
