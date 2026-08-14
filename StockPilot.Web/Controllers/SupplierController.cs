using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var suppliers = await _supplierService.GetAllAsync();

            var totalCount = suppliers.Count;

            var totalPageCount = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            if (page < 1)
            {
                page = 1;
            }

            if (totalPageCount > 0 && page > totalPageCount)
            {
                page = totalPageCount;
            }

            var pagedSuppliers = suppliers
                .OrderBy(supplier => supplier.SupplierId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPageCount = totalPageCount;
            ViewBag.TotalCount = totalCount;

            return View(pagedSuppliers);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Supplier { IsActive = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            supplier.IsActive = true;

            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            await _supplierService.AddAsync(supplier);

            TempData["SuccessMessage"] = "Supplier created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            var existing = await _supplierService.GetByIdAsync(supplier.SupplierId);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = supplier.Name;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Email = supplier.Email;
            existing.Phone = supplier.Phone;
            existing.Address = supplier.Address;
            existing.IsActive = supplier.IsActive;

            await _supplierService.UpdateAsync(existing);

            TempData["SuccessMessage"] = "Supplier updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _supplierService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Supplier deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}