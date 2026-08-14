using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var customers = await _customerService.GetAllAsync();

            var totalCount = customers.Count;

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

            var pagedCustomers = customers
                .OrderBy(customer => customer.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPageCount = totalPageCount;
            ViewBag.TotalCount = totalCount;

            return View(pagedCustomers);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Customer { IsActive = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            customer.IsActive = true;

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            await _customerService.AddAsync(customer);

            TempData["SuccessMessage"] = "Customer created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var existing = await _customerService.GetByIdAsync(customer.CustomerId);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = customer.Name;
            existing.ContactPerson = customer.ContactPerson;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.Address = customer.Address;
            existing.IsActive = customer.IsActive;

            await _customerService.UpdateAsync(existing);

            TempData["SuccessMessage"] = "Customer updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _customerService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Customer deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}