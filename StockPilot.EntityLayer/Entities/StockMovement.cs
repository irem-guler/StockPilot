using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.EntityLayer.Entities
{
    public class StockMovement
    {
        public int StockMovementId { get; set; }

        public int ProductId { get; set; }

        public int? SourceWarehouseId { get; set; }

        public int? DestinationWarehouseId { get; set; }

        public StockMovementType MovementType { get; set; }

        public int Quantity { get; set; }

        public DateTime MovementDateUtc { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

        public Product Product { get; set; } = null!;

        public Warehouse? SourceWarehouse { get; set; }

        public Warehouse? DestinationWarehouse { get; set; }
        public string? PerformedByUserId { get; set; }

        public AppUser? PerformedByUser { get; set; }
    }
}
