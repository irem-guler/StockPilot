using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StockPilot.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;

        public InventoryController(
            IInventoryService inventoryService,
            IWarehouseService warehouseService,
            IProductService productService)
        {
            _inventoryService = inventoryService;
            _warehouseService = warehouseService;
            _productService = productService;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? warehouseId,
            int page = 1)
        {
            const int pageSize = 10;

            var stocks = await _inventoryService.GetInventoryAsync(
                searchTerm,
                warehouseId);

            var warehouses = await _warehouseService.GetAllAsync();

            var activeWarehouses = warehouses
                .Where(warehouse => warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Name)
                .ToList();

            var totalStockCount = stocks.Count;

            var totalPageCount = (int)Math.Ceiling(
                totalStockCount / (double)pageSize);

            if (page < 1)
            {
                page = 1;
            }

            if (totalPageCount > 0 && page > totalPageCount)
            {
                page = totalPageCount;
            }

            var pagedStocks = stocks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new InventoryIndexViewModel
            {
                Stocks = pagedStocks,
                Warehouses = activeWarehouses,
                SearchTerm = searchTerm?.Trim(),
                WarehouseId = warehouseId,
                CurrentPage = page,
                TotalPageCount = totalPageCount,
                TotalStockCount = totalStockCount
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> StockIn()
        {
            var viewModel = new StockInViewModel();

            await PopulateSelectListsAsync(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockIn(StockInViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(viewModel);

                return View(viewModel);
            }

            var result = await _inventoryService.StockInAsync(
                viewModel.ProductId,
                viewModel.WarehouseId,
                viewModel.Quantity,
                viewModel.Note);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Stock in operation failed.");

                await PopulateSelectListsAsync(viewModel);

                return View(viewModel);
            }

            TempData["SuccessMessage"] =
                "Stock in operation completed successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectListsAsync(StockInViewModel viewModel)
        {
            var products = await _productService.GetAllAsync();

            viewModel.Products = products
                .Where(product => product.IsActive)
                .OrderBy(product => product.Name)
                .Select(product => new SelectListItem
                {
                    Value = product.ProductId.ToString(),
                    Text = $"{product.Name} ({product.SKU})"
                })
                .ToList();

            var warehouses = await _warehouseService.GetAllAsync();

            viewModel.Warehouses = warehouses
                .Where(warehouse => warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Name)
                .Select(warehouse => new SelectListItem
                {
                    Value = warehouse.WarehouseId.ToString(),
                    Text = warehouse.Name
                })
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> StockOut()
        {
            var viewModel = new StockOutViewModel();

            await PopulateSelectListsAsync(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockOut(StockOutViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(viewModel);

                return View(viewModel);
            }

            var result = await _inventoryService.StockOutAsync(
                viewModel.ProductId,
                viewModel.WarehouseId,
                viewModel.Quantity,
                viewModel.Note);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Stock out operation failed.");

                await PopulateSelectListsAsync(viewModel);

                return View(viewModel);
            }

            TempData["SuccessMessage"] =
                "Stock out operation completed successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var viewModel = new TransferViewModel();

            await PopulateTransferSelectListsAsync(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTransferSelectListsAsync(viewModel);

                return View(viewModel);
            }

            var result = await _inventoryService.TransferAsync(
                viewModel.ProductId,
                viewModel.SourceWarehouseId,
                viewModel.DestinationWarehouseId,
                viewModel.Quantity,
                viewModel.Note);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Transfer operation failed.");

                await PopulateTransferSelectListsAsync(viewModel);

                return View(viewModel);
            }

            TempData["SuccessMessage"] =
                "Transfer operation completed successfully.";

            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateTransferSelectListsAsync(
    TransferViewModel viewModel)
        {
            var products = await _productService.GetAllAsync();

            viewModel.Products = products
                .Where(product => product.IsActive)
                .OrderBy(product => product.Name)
                .Select(product => new SelectListItem
                {
                    Value = product.ProductId.ToString(),
                    Text = $"{product.Name} ({product.SKU})"
                })
                .ToList();

            var warehouses = await _warehouseService.GetAllAsync();

            var activeWarehouseItems = warehouses
                .Where(warehouse => warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Name)
                .Select(warehouse => new SelectListItem
                {
                    Value = warehouse.WarehouseId.ToString(),
                    Text = warehouse.Name
                })
                .ToList();

            viewModel.SourceWarehouses = activeWarehouseItems;

            viewModel.DestinationWarehouses = activeWarehouseItems
                .Select(item => new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text
                })
                .ToList();
        }
    }
}