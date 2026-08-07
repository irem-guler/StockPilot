using Microsoft.AspNetCore.Mvc.Rendering;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.Web.Models
{
    public class MovementHistoryViewModel
    {
        public List<StockMovement> Movements { get; set; } = new();

        public List<SelectListItem> Products { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();

        public int? ProductId { get; set; }

        public int? WarehouseId { get; set; }

        public StockMovementType? MovementType { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPageCount { get; set; }

        public int TotalMovementCount { get; set; }
    }
}