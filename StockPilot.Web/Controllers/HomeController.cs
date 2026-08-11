using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IInventoryService _inventoryService;

        public HomeController(
            IProductService productService,
            IWarehouseService warehouseService,
            IInventoryService inventoryService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            var warehouses = await _warehouseService.GetAllAsync();

            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var movements = await _inventoryService.GetMovementsAsync(
                null, null, null);

            var criticalStocks = inventory
                .Where(stock => stock.Quantity <= stock.Product.ReorderLevel)
                .OrderBy(stock => stock.Quantity)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalProductCount = products.Count(product => product.IsActive),
                TotalWarehouseCount = warehouses.Count(warehouse => warehouse.IsActive),
                CriticalStockCount = criticalStocks.Count,
                TotalMovementCount = movements.Count,
                CriticalStocks = criticalStocks.Take(10).ToList(),
                RecentMovements = movements.Take(10).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}