using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.Web.Models;
using StockPilot.Web.Services;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly UserManager<AppUser> _userManager;
        private readonly OrderPdfService _pdfService;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IWarehouseService warehouseService,
            IProductService productService,
            OrderPdfService pdfService,
            UserManager<AppUser> userManager)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _warehouseService = warehouseService;
            _productService = productService;
            _userManager = userManager;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _purchaseOrderService.GetAllAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _purchaseOrderService.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var order = await _purchaseOrderService.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var pdfBytes = _pdfService.GeneratePurchaseOrderPdf(order);

            var fileName = $"PurchaseOrder_{order.PurchaseOrderId}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreatePurchaseOrderViewModel();
            await PopulateSelectListsAsync(viewModel);
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseOrderViewModel viewModel)
        {
            var order = new PurchaseOrder
            {
                SupplierId = viewModel.SupplierId,
                WarehouseId = viewModel.WarehouseId,
                Note = string.IsNullOrWhiteSpace(viewModel.Note) ? null : viewModel.Note.Trim(),
                CreatedByUserId = _userManager.GetUserId(User),
                Items = viewModel.Items
                    .Where(item => item.ProductId > 0 && item.Quantity > 0)
                    .Select(item => new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };

            var result = await _purchaseOrderService.CreateAsync(order);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Order could not be created.");
                await PopulateSelectListsAsync(viewModel);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = "Purchase order created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _purchaseOrderService.CancelAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Purchase order cancelled.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            var userId = _userManager.GetUserId(User);

            var result = await _purchaseOrderService.ReceiveAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Order received. Stock has been updated successfully.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateSelectListsAsync(CreatePurchaseOrderViewModel viewModel)
        {
            var suppliers = await _supplierService.GetAllAsync();
            viewModel.Suppliers = suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.Name })
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