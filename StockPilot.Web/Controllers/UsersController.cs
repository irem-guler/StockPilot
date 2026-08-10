using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockPilot.EntityLayer.Entities;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderBy(user => user.Email)
                .ToList();

            var userList = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "-",
                    IsActive = user.IsActive
                });
            }

            return View(userList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateRoles();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoles();

                return View(viewModel);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(viewModel.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This email address is already registered.");

                PopulateRoles();

                return View(viewModel);
            }

            var user = new AppUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user, viewModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                PopulateRoles();

                return View(viewModel);
            }

            await _userManager.AddToRoleAsync(user, viewModel.Role);

            TempData["SuccessMessage"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] =
                    "You cannot deactivate your own account.";

                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = user.IsActive
                ? "User activated successfully."
                : "User deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        private void PopulateRoles()
        {
            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "User", Text = "User" },
                new SelectListItem { Value = "Admin", Text = "Admin" }
            };
        }
    }
}