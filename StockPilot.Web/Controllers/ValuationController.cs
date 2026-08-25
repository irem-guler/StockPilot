using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ValuationController : Controller
    {
        private readonly IValuationService _valuationService;

        public ValuationController(IValuationService valuationService)
        {
            _valuationService = valuationService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _valuationService.GetValuationAsync();

            var viewModel = new ValuationViewModel();

            viewModel.Items = result.Items.Select(i => new ProductValuationItem
            {
                ProductId = i.Product.ProductId,
                ProductName = i.Product.Name,
                SKU = i.Product.SKU,
                TotalQuantity = i.TotalQuantity,
                UnitPrice = i.Product.UnitPrice,
                TotalValue = i.TotalValue,
                ValuePercentage = i.ValuePercentage,
                CumulativePercentage = i.CumulativePercentage,
                AbcClass = i.AbcClass
            }).ToList();

            
            viewModel.DeadStock = result.DeadStock.Select(d => new DeadStockItem
            {
                ProductId = d.Product.ProductId,
                ProductName = d.Product.Name,
                SKU = d.Product.SKU,
                TotalQuantity = d.TotalQuantity,
                TotalValue = d.TotalValue,
                LastMovementDate = d.LastMovementDate,
                DaysSinceLastMovement = d.DaysSinceLastMovement
            }).ToList();

           
            viewModel.TotalInventoryValue = viewModel.Items.Sum(i => i.TotalValue);
            viewModel.TotalProductCount = viewModel.Items.Count;
            viewModel.TotalUnits = viewModel.Items.Sum(i => i.TotalQuantity);

            viewModel.ClassACount = viewModel.Items.Count(i => i.AbcClass == "A");
            viewModel.ClassBCount = viewModel.Items.Count(i => i.AbcClass == "B");
            viewModel.ClassCCount = viewModel.Items.Count(i => i.AbcClass == "C");

            viewModel.ClassAValue = viewModel.Items.Where(i => i.AbcClass == "A").Sum(i => i.TotalValue);
            viewModel.ClassBValue = viewModel.Items.Where(i => i.AbcClass == "B").Sum(i => i.TotalValue);
            viewModel.ClassCValue = viewModel.Items.Where(i => i.AbcClass == "C").Sum(i => i.TotalValue);

            return View(viewModel);
        }
    }
}