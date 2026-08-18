using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class SalesOrderController : Controller
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ICustomerService _customerService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly UserManager<AppUser> _userManager;

        public SalesOrderController(
            ISalesOrderService salesOrderService,
            ICustomerService customerService,
            IWarehouseService warehouseService,
            IProductService productService,
            UserManager<AppUser> userManager)
        {
            _salesOrderService = salesOrderService;
            _customerService = customerService;
            _warehouseService = warehouseService;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _salesOrderService.GetAllAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _salesOrderService.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSalesOrderViewModel();
            await PopulateSelectListsAsync(viewModel);
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSalesOrderViewModel viewModel)
        {
            var order = new SalesOrder
            {
                CustomerId = viewModel.CustomerId,
                WarehouseId = viewModel.WarehouseId,
                Note = string.IsNullOrWhiteSpace(viewModel.Note) ? null : viewModel.Note.Trim(),
                CreatedByUserId = _userManager.GetUserId(User),
                Items = viewModel.Items
                    .Where(item => item.ProductId > 0 && item.Quantity > 0)
                    .Select(item => new SalesOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };

            var result = await _salesOrderService.CreateAsync(order);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Order could not be created.");
                await PopulateSelectListsAsync(viewModel);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = "Sales order created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(int id)
        {
            var userId = _userManager.GetUserId(User);

            var result = await _salesOrderService.ShipAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Order shipped. Stock has been updated successfully.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _salesOrderService.CancelAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Sales order cancelled.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectListsAsync(CreateSalesOrderViewModel viewModel)
        {
            var customers = await _customerService.GetAllAsync();
            viewModel.Customers = customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CustomerId.ToString(), Text = c.Name })
                .ToList();

            var warehouses = await _warehouseService.GetAllAsync();
            viewModel.Warehouses = warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.WarehouseId.ToString(), Text = w.Name })
                .ToList();

            var products = await _productService.GetAllAsync();
            viewModel.Products = products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToList();

            ViewBag.ProductPrices = products
                .Where(p => p.IsActive)
                .ToDictionary(p => p.ProductId, p => p.UnitPrice);
        }
    }
}