using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var warehouses = await _warehouseService.GetAllAsync();

            var totalWarehouseCount = warehouses.Count;

            var totalPageCount = (int)Math.Ceiling(
                totalWarehouseCount / (double)pageSize);

            if (page < 1)
            {
                page = 1;
            }

            if (totalPageCount > 0 && page > totalPageCount)
            {
                page = totalPageCount;
            }

            var pagedWarehouses = warehouses
                .OrderBy(warehouse => warehouse.WarehouseId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPageCount = totalPageCount;
            ViewBag.TotalWarehouseCount = totalWarehouseCount;

            return View(pagedWarehouses);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var warehouse = new Warehouse
            {
                IsActive = true
            };

            return View(warehouse);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            warehouse.IsActive = true;

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(item => item.Value != null &&
                                   item.Value.Errors.Count > 0)
                    .SelectMany(item => item.Value!.Errors.Select(error =>
                        $"{item.Key}: {error.ErrorMessage}"))
                    .ToList();

                ViewBag.ErrorMessage = string.Join(" | ", errors);

                return View(warehouse);
            }

            await _warehouseService.AddAsync(warehouse);

            TempData["SuccessMessage"] =
                "Warehouse created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);

            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
            {
                return View(warehouse);
            }

            var existingWarehouse =
                await _warehouseService.GetByIdAsync(
                    warehouse.WarehouseId);

            if (existingWarehouse == null)
            {
                return NotFound();
            }

            existingWarehouse.Name = warehouse.Name;
            existingWarehouse.Location = warehouse.Location;
            existingWarehouse.Description = warehouse.Description;
            existingWarehouse.IsActive = warehouse.IsActive;

            await _warehouseService.UpdateAsync(existingWarehouse);

            TempData["SuccessMessage"] =
                "Warehouse updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result =
                await _warehouseService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Warehouse deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}