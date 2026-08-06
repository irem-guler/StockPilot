using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Models
{
    public class InventoryIndexViewModel
    {
        public List<WarehouseStock> Stocks { get; set; } = new();

        public List<Warehouse> Warehouses { get; set; } = new();

        public string? SearchTerm { get; set; }

        public int? WarehouseId { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPageCount { get; set; }

        public int TotalStockCount { get; set; }
    }
}