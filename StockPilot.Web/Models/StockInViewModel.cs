using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StockPilot.Web.Models
{
    public class StockInViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a warehouse.")]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public List<SelectListItem> Products { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();
    }
}