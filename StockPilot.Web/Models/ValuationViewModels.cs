namespace StockPilot.Web.Models
{
    public class ProductValuationItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public int TotalQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }

        public double ValuePercentage { get; set; }
        public double CumulativePercentage { get; set; }

        public string AbcClass { get; set; } = "C";
    }

    public class DeadStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
    }

    public class ValuationViewModel
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalProductCount { get; set; }
        public int TotalUnits { get; set; }

        public int ClassACount { get; set; }
        public int ClassBCount { get; set; }
        public int ClassCCount { get; set; }

        public decimal ClassAValue { get; set; }
        public decimal ClassBValue { get; set; }
        public decimal ClassCValue { get; set; }

        public List<ProductValuationItem> Items { get; set; } = new();
        public List<DeadStockItem> DeadStock { get; set; } = new();
    }
}