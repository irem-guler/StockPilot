using Microsoft.AspNetCore.Mvc.Rendering;

namespace StockPilot.Web.Models
{
    public class SalesOrderItemInput
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class CreateSalesOrderViewModel
    {
        public int CustomerId { get; set; }

        public int WarehouseId { get; set; }

        public string? Note { get; set; }

        public List<SalesOrderItemInput> Items { get; set; } = new();

        public List<SelectListItem> Customers { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();

        public List<SelectListItem> Products { get; set; } = new();
    }
}