using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        public SearchController(
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index(string? query)
        {
            var viewModel = new SearchResultViewModel
            {
                Query = query?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(viewModel);
            }

            var normalizedQuery = query.Trim().ToLower();

            var products = await _productService.GetAllAsync();

            viewModel.Products = products
                .Where(product =>
                    (product.Name != null &&
                     product.Name.ToLower().Contains(normalizedQuery)) ||
                    (product.SKU != null &&
                     product.SKU.ToLower().Contains(normalizedQuery)))
                .OrderBy(product => product.Name)
                .Take(20)
                .ToList();

            var warehouses = await _warehouseService.GetAllAsync();

            viewModel.Warehouses = warehouses
                .Where(warehouse =>
                    (warehouse.Name != null &&
                     warehouse.Name.ToLower().Contains(normalizedQuery)) ||
                    (warehouse.Location != null &&
                     warehouse.Location.ToLower().Contains(normalizedQuery)))
                .OrderBy(warehouse => warehouse.Name)
                .Take(20)
                .ToList();

            return View(viewModel);
        }
    }
}