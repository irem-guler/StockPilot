using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.EntityLayer.Entities
{
    public class WarehouseStock
    {
        public int WarehouseStockId { get; set; }

        public int ProductId { get; set; }

        public int WarehouseId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; } = null!;

        public Warehouse Warehouse { get; set; } = null!;
    }
}
