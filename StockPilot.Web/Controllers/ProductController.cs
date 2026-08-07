using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var allProducts = await _productService.GetAllAsync();

            int totalProductCount = allProducts.Count;

            int totalPages = (int)Math.Ceiling(
                totalProductCount / (double)pageSize
            );

            if (page < 1)
            {
                page = 1;
            }

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var products = allProducts
                .OrderBy(product => product.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProductCount = totalProductCount;

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var product = new Product
            {
                IsActive = true
            };

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            if (await _productService.IsSkuInUseAsync(product.SKU))
            {
                ModelState.AddModelError(
                    nameof(product.SKU),
                    "This SKU is already in use by another product.");

                return View(product);
            }

            await _productService.AddAsync(product);

            TempData["SuccessMessage"] =
                "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var existingProduct =
        await _productService.GetByIdAsync(product.ProductId);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (await _productService.IsSkuInUseAsync(
                    product.SKU, product.ProductId))
            {
                ModelState.AddModelError(
                    nameof(product.SKU),
                    "This SKU is already in use by another product.");

                return View(product);
            }

            existingProduct.Name = product.Name;

            existingProduct.Name = product.Name;
            existingProduct.SKU = product.SKU;
            existingProduct.Description = product.Description;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.ReorderLevel = product.ReorderLevel;
            existingProduct.IsActive = product.IsActive;

            await _productService.UpdateAsync(existingProduct);

            TempData["SuccessMessage"] =
                "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _productService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Product deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}