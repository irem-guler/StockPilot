using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalProductCount { get; set; }

        public int TotalWarehouseCount { get; set; }

        public int CriticalStockCount { get; set; }

        public int TotalMovementCount { get; set; }

        public List<WarehouseStock> CriticalStocks { get; set; } = new();

        public List<StockMovement> RecentMovements { get; set; } = new();
        public int StockInCount { get; set; }

        public int StockOutCount { get; set; }

        public int TransferCount { get; set; }

        public List<string> WarehouseNames { get; set; } = new();

        public List<int> WarehouseQuantities { get; set; } = new();

        public List<string> MovementDays { get; set; } = new();

        public List<int> MovementDayCounts { get; set; } = new();
    }
}