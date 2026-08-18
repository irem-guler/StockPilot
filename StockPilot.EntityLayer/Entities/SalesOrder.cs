using StockPilot.EntityLayer.Enums;

namespace StockPilot.EntityLayer.Entities
{
    public class SalesOrder
    {
        public int SalesOrderId { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;

        public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

        public DateTime? ShippedDateUtc { get; set; }

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Pending;

        public string? Note { get; set; }

        public string? CreatedByUserId { get; set; }

        public AppUser? CreatedByUser { get; set; }

        public List<SalesOrderItem> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
    }
}