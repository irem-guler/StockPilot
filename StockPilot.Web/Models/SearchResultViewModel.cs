using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Models
{
    public class SearchResultViewModel
    {
        public string Query { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new();

        public List<Warehouse> Warehouses { get; set; } = new();

        public int TotalResultCount => Products.Count + Warehouses.Count;
    }
}