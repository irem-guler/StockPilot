using Microsoft.AspNetCore.Mvc.Rendering;

namespace StockPilot.Web.Models
{
    public class ReorderSuggestionItem
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public int CurrentStock { get; set; }

        public int ReorderLevel { get; set; }

        public int SuggestedQuantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class ReorderSuggestionViewModel
    {
        public List<ReorderSuggestionItem> Suggestions { get; set; } = new();

        public List<SelectListItem> Suppliers { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();
    }
}