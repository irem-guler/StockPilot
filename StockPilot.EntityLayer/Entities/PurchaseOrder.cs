using StockPilot.EntityLayer.Enums;

namespace StockPilot.EntityLayer.Entities
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }

        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;

        public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

        public DateTime? ReceivedDateUtc { get; set; }

        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

        public string? Note { get; set; }

        public string? CreatedByUserId { get; set; }

        public AppUser? CreatedByUser { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
    }
}