using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReorderController : Controller
    {
        private readonly IReorderService _reorderService;
        private readonly ISupplierService _supplierService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly UserManager<AppUser> _userManager;

        public ReorderController(
            IReorderService reorderService,
            ISupplierService supplierService,
            IWarehouseService warehouseService,
            IProductService productService,
            IPurchaseOrderService purchaseOrderService,
            UserManager<AppUser> userManager)
        {
            _reorderService = reorderService;
            _supplierService = supplierService;
            _warehouseService = warehouseService;
            _productService = productService;
            _purchaseOrderService = purchaseOrderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var suggestions = await _reorderService.GetSuggestionsAsync();

            var viewModel = new ReorderSuggestionViewModel
            {
                Suggestions = suggestions.Select(s => new ReorderSuggestionItem
                {
                    ProductId = s.Product.ProductId,
                    ProductName = s.Product.Name,
                    SKU = s.Product.SKU,
                    CurrentStock = s.CurrentStock,
                    ReorderLevel = s.Product.ReorderLevel,
                    SuggestedQuantity = s.SuggestedQuantity,
                    UnitPrice = s.Product.UnitPrice
                }).ToList()
            };

            await PopulateSelectListsAsync(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(
    int supplierId,
    int warehouseId,
    List<int> selectedProductIds)
        {
            if (supplierId <= 0 || warehouseId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a supplier and a warehouse.";
                return RedirectToAction(nameof(Index));
            }

            if (selectedProductIds == null || selectedProductIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one product to order.";
                return RedirectToAction(nameof(Index));
            }

            var order = new PurchaseOrder
            {
                SupplierId = supplierId,
                WarehouseId = warehouseId,
                Note = "Created from reorder suggestions",
                CreatedByUserId = _userManager.GetUserId(User),
                Items = new List<PurchaseOrderItem>()
            };

            var allProducts = await _productService.GetAllAsync();

            foreach (var productId in selectedProductIds)
            {
                var quantityKey = $"quantity_{productId}";
                var quantityValue = Request.Form[quantityKey].ToString();

                if (!int.TryParse(quantityValue, out var quantity) || quantity <= 0)
                {
                    continue;
                }

                var product = allProducts.FirstOrDefault(p => p.ProductId == productId);

                order.Items.Add(new PurchaseOrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product?.UnitPrice ?? 0
                });
            }

            var result = await _purchaseOrderService.CreateAsync(order);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Purchase order created from suggestions. You can review and receive it in Purchase Orders.";

            return RedirectToAction("Index", "PurchaseOrder");
        }

        private async Task PopulateSelectListsAsync(ReorderSuggestionViewModel viewModel)
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
        }
    }
}