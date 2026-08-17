using Microsoft.AspNetCore.Mvc.Rendering;

namespace StockPilot.Web.Models
{
    public class PurchaseOrderItemInput
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class CreatePurchaseOrderViewModel
    {
        public int SupplierId { get; set; }

        public int WarehouseId { get; set; }

        public string? Note { get; set; }

        public List<PurchaseOrderItemInput> Items { get; set; } = new();

        public List<SelectListItem> Suppliers { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();

        public List<SelectListItem> Products { get; set; } = new();
    }
}