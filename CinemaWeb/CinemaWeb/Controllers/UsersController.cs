using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CinemaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<User>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault() ?? "User";
                model.Add(user);
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            user.Role = userRoles.FirstOrDefault() ?? "User";

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "User", Text = "Користувач" },
                new SelectListItem { Value = "Admin", Text = "Адміністратор" }
            };

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Email,FullName,Role")] User model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            bool isMainAdmin = user.Email.Equals(
                "admin@cinemaweb.com",
                StringComparison.OrdinalIgnoreCase);

            if (isMainAdmin && model.Role != "Admin")
            {
                ModelState.AddModelError("Role",
                    "Неможливо зняти права адміністратора " +
                    "з головного облікового запису.");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (user.Id.ToString() == currentUserId && model.Role != "Admin")
            {
                ModelState.AddModelError("Role",
                    "Ви не можете позбавити прав адміністратора самі себе.");
            }

            if (ModelState.IsValid)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(
                            user, currentRoles);

                        await _userManager.AddToRoleAsync(
                            user, model.Role);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "User", Text = "Користувач" },
                new SelectListItem { Value = "Admin", Text = "Адміністратор" }
            };

            return View(model);
        }
    }
}