using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;

        public List<WarehouseStock> StockByWarehouse { get; set; } = new();

        public List<StockMovement> RecentMovements { get; set; } = new();

        public int TotalQuantity { get; set; }
    }
}