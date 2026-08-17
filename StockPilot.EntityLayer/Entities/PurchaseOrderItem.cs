namespace StockPilot.EntityLayer.Entities
{
    public class PurchaseOrderItem
    {
        public int PurchaseOrderItemId { get; set; }

        public int PurchaseOrderId { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}