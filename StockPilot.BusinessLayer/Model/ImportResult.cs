namespace StockPilot.BusinessLayer.Models
{
    public class ProductImportRow
    {
        public int RowNumber { get; set; }

        public string? Name { get; set; }

        public string? SKU { get; set; }

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public int ReorderLevel { get; set; }
    }

    public class ImportResult
    {
        public int SuccessCount { get; set; }

        public int FailureCount { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}