using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public class ValuationResultItem
    {
        public Product Product { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public double ValuePercentage { get; set; }
        public double CumulativePercentage { get; set; }
        public string AbcClass { get; set; } = "C";
    }

    public class DeadStockResultItem
    {
        public Product Product { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
    }

    public class ValuationResult
    {
        public List<ValuationResultItem> Items { get; set; } = new();
        public List<DeadStockResultItem> DeadStock { get; set; } = new();
    }

    public interface IValuationService
    {
        Task<ValuationResult> GetValuationAsync();
    }
}